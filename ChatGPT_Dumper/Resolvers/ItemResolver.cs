using System.Text.RegularExpressions;
using ChatGPT_Dumper.Classes;
using ChatGPT_Dumper.Helpers;
using ChatGPT_Dumper.Utility;

namespace ChatGPT_Dumper.Resolvers
{
    public sealed class ItemResolver(
        string[] lines,
        Dictionary<string, ClassModel> classes,
        Dictionary<string, Dictionary<string, string>> output)
    {
        private readonly string[] _lines = lines;
        private readonly string _bigText = string.Join('\n', lines);
        private readonly Dictionary<string, ClassModel> _classes = classes;
        private readonly Dictionary<string, Dictionary<string, string>> _output = output;

        public void Run()
        {
            string encItemClass = FindEncryptedItemClass();
            if (encItemClass == "")
                return;

            Program.encryptedValues.Add(new ClassNames { Name = "Item", Value = encItemClass });

            var info = ResolveAllOffsets(encItemClass);

            Utils.Out("namespace Item {");
            Utils.Out($"    constexpr auto uid = 0x{info.uid}; // uid (resolved via GodRaysRenderer uid type)");
            Utils.Out($"    constexpr auto info = 0x{info.info}; // public ItemDefinition %");
            Utils.Out($"    constexpr auto contents = 0x{info.contents}; // contents (TriggerBase HashSet link)");
            Utils.Out($"    constexpr auto amount = 0x{info.amount}; // amount (AddPunch Vector3 param)");
            Utils.Out($"    constexpr auto position = 0x{info.position}; // position (MeshRendererBatch Vector3)");
            Utils.Out($"    constexpr auto condition = 0x{info.condition}; // first private float %");
            Utils.Out($"    constexpr auto maxCondition = 0x{info.maxCondition}; // second private float %");

            if (info.heldAlt == "0")
                Utils.Out($"    constexpr auto heldEntity = 0x{info.held}; // first private % % (ref) in Item");
            else
                Utils.Out($"    constexpr auto heldEntity = 0x{info.held}; // or 0x{info.heldAlt}");

            Utils.Out("};");
            Utils.Out();
        }

        private string FindEncryptedItemClass()
        {
            int start = ClassParser.FindClassStart(_lines,
                "public class EasterBasket : AttackEntity");

            if (start == -1)
                return "";

            int end = ClassParser.FindClassEnd(_lines, start);

            for (int i = start; i <= end; i++)
            {
                var m = R.EasterReturn().Match(_lines[i]);
                if (m.Success)
                    return m.Groups[1].Value;
            }

            return "";
        }

        private (string uid, string info, string contents, string amount,
                 string position, string condition, string maxCondition,
                 string held, string heldAlt)
            ResolveAllOffsets(string encClass)
        {
            string uidTypeName = FindUidTypeName();
            string contentsName = FindContentsName();
            string amountName = FindAmountName();
            string posName = FindPositionName();

            int start = ClassParser.FindClassStart(_lines, "public class " + encClass);
            if (start == -1)
                return ("0", "0", "0", "0", "0", "0", "0", "0", "0");

            int end = ClassParser.FindClassEnd(_lines, start);

            string uid = "0";
            string info = "0";
            string contents = "0";
            string amount = "0";
            string pos = "0";
            string cond = "0";
            string maxCond = "0";
            string held = "0";
            string heldAlt = "0";

            // for heldEntity candidates
            var heldCandidates = new List<(string fieldName, string offset)>();

            int floatIndex = 0;

            Regex? uidFieldRegex = null;
            if (uidTypeName != "")
                uidFieldRegex = new Regex(@"public\s+" +
                    Regex.Escape(uidTypeName) + @"\s+%[0-9a-fA-F]{40};");

            for (int i = start; i <= end; i++)
            {
                string trimmed = _lines[i].Trim();
                if (Utils.IsComment(trimmed))
                    continue;

                if (info == "0" && R.Info().IsMatch(trimmed))
                    info = Grab(trimmed);

                if (uid == "0" && uidFieldRegex != null && uidFieldRegex.IsMatch(trimmed))
                    uid = Grab(trimmed);

                if (contents == "0" && trimmed.Contains(contentsName))
                    contents = Grab(trimmed);

                if (amount == "0" && trimmed.Contains(amountName))
                    amount = Grab(trimmed);

                if (pos == "0" && trimmed.Contains(posName))
                    pos = Grab(trimmed);

                if (R.PrivateFloat().IsMatch(trimmed))
                {
                    floatIndex++;
                    var val = Grab(trimmed);
                    if (floatIndex == 1)
                        cond = val;
                    else if (floatIndex == 2)
                        maxCond = val;
                }

                var hh = R.Held().Match(trimmed);
                if (hh.Success && !trimmed.Contains("static"))
                {
                    var offset = Grab(trimmed);

                    // Get the second encrypted value in: private %TYPE %FIELD; // 0x..
                    // Example: "private %2d28... %09f5...; // 0x78"
                    var mEnc = Regex.Match(trimmed,
                        @"^private\s+%[0-9a-fA-F]{40}\s+(%[0-9a-fA-F]{40})\s*;");

                    if (mEnc.Success)
                    {
                        var fieldName = mEnc.Groups[1].Value; // %09f5..., %a3a0...
                        heldCandidates.Add((fieldName, offset));
                    }
                }

                // Decide held / heldAlt based on how often the encrypted field name is used
                if (heldCandidates.Count > 0)
                {
                    var ordered = heldCandidates
                        .OrderByDescending(c => CountEncryptedUsage(c.fieldName))
                        .ToList();

                    held = ordered[0].offset;

                    if (ordered.Count > 1)
                        heldAlt = ordered[1].offset;
                }


            }

            return (uid, info, contents, amount, pos, cond, maxCond, held, heldAlt);
        }


        private int CountEncryptedUsage(string encryptedName)
        {
            if (string.IsNullOrEmpty(encryptedName))
                return 0;

            int count = 0;
            ReadOnlySpan<char> span = _bigText.AsSpan();

            while (true)
            {
                int index = span.IndexOf(encryptedName);
                if (index < 0)
                    break;

                count++;
                span = span.Slice(index + encryptedName.Length);
            }

            return count;
        }


        private static string Grab(string line)
        {
            var m = R.Offset().Match(line);
            return m.Success ? m.Groups[1].Value : "0";
        }

        private string FindUidTypeName()
        {
            int start = ClassParser.FindClassStart(_lines,
                "public class GodRaysRenderer");

            if (start == -1)
                return "";

            int end = ClassParser.FindClassEnd(_lines, start);

            // 1) Old behavior: try the field based regex first, in case it ever comes back
            for (int i = start; i <= end; i++)
            {
                var mOld = R.UidType().Match(_lines[i]);
                if (mOld.Success)
                    return mOld.Groups[1].Value;
            }

            // 2) New behavior: find the second <%...> inside the class
            // Example line:
            // |-GodRaysRenderer.%52125dfcf779ad8e5266144ac74172cc655cc485<%c8cfc78589ed9969fcb2dc576aaacfc99d196743>
            //
            // We want the value inside the angle brackets:
            // %c8cfc78589ed9969fcb2dc576aaacfc99d196743

            var bracketRegex = new Regex(@"<(%[0-9a-fA-F]{40})>");

            int matchCount = 0;

            for (int i = start; i <= end; i++)
            {
                var line = _lines[i];

                var matches = bracketRegex.Matches(line);
                foreach (Match m in matches)
                {
                    matchCount++;

                    if (matchCount == 2)
                    {
                        // This gives you "%c8cfc7..." including the leading '%'
                        return m.Groups[1].Value;

                        // If you ever want it without the '%', use:
                        // return m.Groups[1].Value.TrimStart('%');
                    }
                }
            }

            return "";
        }

        private string FindContentsName()
        {
            int start = ClassParser.FindClassStart(_lines, "public class TriggerBase");
            if (start == -1)
                return "";

            int end = ClassParser.FindClassEnd(_lines, start);

            for (int i = start; i <= end; i++)
            {
                var m = R.Contents().Match(_lines[i]);
                if (m.Success)
                    return m.Groups[1].Value;
            }

            return "";
        }

        private string FindAmountName()
        {
            foreach (var line in _lines)
            {
                var m = R.AddPunch().Match(line);
                if (m.Success)
                    return m.Groups[1].Value;
            }
            return "";
        }

        private string FindPositionName()
        {
            int start = ClassParser.FindClassStart(_lines,
                "public class MeshRendererBatch");

            if (start == -1)
                return "";

            int end = ClassParser.FindClassEnd(_lines, start);

            for (int i = start; i <= end; i++)
            {
                var m = R.Pos().Match(_lines[i]);
                if (m.Success)
                    return m.Groups[1].Value;
            }

            return "";
        }
    }
}

using ChatGPT_Dumper.Classes;
using ChatGPT_Dumper.Helpers;
using ChatGPT_Dumper.Utility;

namespace ChatGPT_Dumper.Resolvers
{
    public sealed class TodSkyResolver(
        string[] lines,
        Dictionary<string, ClassModel> classes,
        Dictionary<string, Dictionary<string, string>> output)
    {
        private readonly string[] _lines = lines;
        private readonly Dictionary<string, ClassModel> _classes = classes;
        private readonly Dictionary<string, Dictionary<string, string>> _output = output;

        public void Run()
        {
            if (!_classes.TryGetValue("TOD_Sky", out _))
                return;

            string initializedEnc = FindMuscleBoolName();
            if (initializedEnc == "")
                return;

            string offset = FindSkyBoolOffset(initializedEnc);
            if (offset == "0")
                return;

            if (_output.TryGetValue("TOD_Sky", out var dict))
                dict["Initialized"] = offset;
        }

        private string FindMuscleBoolName()
        {
            int start = ClassParser.FindClassStart(_lines,
                "public class FMuscle_Vector3");

            if (start == -1)
                return "";

            int end = ClassParser.FindClassEnd(_lines, start);
            if (end == -1)
                return "";

            for (int i = start; i <= end; i++)
            {
                var m = R.MuscleBool().Match(_lines[i]);
                if (m.Success)
                    return m.Groups[1].Value;
            }

            return "";
        }

        private string FindSkyBoolOffset(string encName)
        {
            int start = ClassParser.FindClassStart(_lines,
                "public class TOD_Sky : MonoBehaviour");

            if (start == -1)
                return "0";

            int end = ClassParser.FindClassEnd(_lines, start);

            var regex = new System.Text.RegularExpressions.Regex(
                @"private\s+bool\s+" +
                System.Text.RegularExpressions.Regex.Escape(encName) + @";");

            for (int i = start; i <= end; i++)
            {
                string line = _lines[i].Trim();
                if (Utils.IsComment(line))
                    continue;

                if (regex.IsMatch(line))
                {
                    var m = R.Offset().Match(line);
                    if (m.Success)
                        return m.Groups[1].Value;
                }
            }

            return "0";
        }
    }
}

using ChatGPT_Dumper.Classes;
using ChatGPT_Dumper.Helpers;
using ChatGPT_Dumper.Utility;

namespace ChatGPT_Dumper.Resolvers
{
    public sealed class BaseNetworkableResolver
    {
        private readonly string[] _lines;
        private readonly Dictionary<string, ClassModel> _classes;
        private readonly Dictionary<string, Dictionary<string, string>> _output;

        public BaseNetworkableResolver(
            string[] lines,
            Dictionary<string, ClassModel> classes,
            Dictionary<string, Dictionary<string, string>> output)
        {
            _lines = lines;
            _classes = classes;
            _output = output;
        }

        private string FindFirstEncryptedProperty(ClassModel model)
        {
            foreach (var line in model.Enumerate())
            {
                if (Utils.IsComment(line))
                    continue;

                var m = R.EncryptedProperty().Match(line);
                if (!m.Success)
                    continue;

                if (line.Length != 99)
                    continue;

                // m.Groups[1] is the type, m.Groups[2] is the property name
                return m.Groups[1].Value;   // or Groups[1] if you care about the type instead
            }

            return "";
        }

        public void Run()
        {
            if (!_classes.TryGetValue("BaseNetworkable", out var model))
                return;

            var encPropName = FindFirstEncryptedProperty(model);
            if (encPropName != "")
                Program.encryptedValues.Add(new ClassNames { Name = "BaseNetworkable2", Value = encPropName });

            string netOffset = "0";
            string parentOffset = "0";
            string destroyedOffset = "0";

            string encryptedType = FindEncryptedType();
            if (encryptedType != "")
            {
                (netOffset, parentOffset, destroyedOffset) =
                    ResolveOffsets(model, encryptedType);
            }

            if (_output.TryGetValue("BaseNetworkable", out var dict))
            {
                if (netOffset != "0")
                    dict["net"] = netOffset;
                if (parentOffset != "0")
                    dict["parentEntity"] = parentOffset;
                if (destroyedOffset != "0")
                    dict["IsDestroyedk__BackingField"] = destroyedOffset;
            }
        }

        private string FindEncryptedType()
        {
            for (int i = 0; i < _lines.Length; i++)
            {
                var mHeader = R.IfaceHeader().Match(_lines[i]);
                if (!mHeader.Success)
                    continue;

                int start = i;
                int end = ClassParser.FindClassEnd(_lines, start);
                if (end == -1)
                    continue;

                bool hasAbstractBool = false;
                for (int j = start; j <= end; j++)
                {
                    if (_lines[j].Contains("public abstract bool"))
                    {
                        hasAbstractBool = true;
                        break;
                    }
                }
                if (hasAbstractBool)
                    continue;

                var list = new List<string>();
                for (int j = start; j <= end; j++)
                {
                    var m = R.AbstractIfaceVoid().Match(_lines[j]);
                    if (m.Success)
                        list.Add(m.Groups[1].Value);
                }

                if (list.Count == 2)
                    return list[0];
            }

            return "";
        }

        private (string netOff, string parentOff, string destroyedOff)
            ResolveOffsets(ClassModel model, string encType)
        {
            string net = "0";
            string parent = "0";
            string destroyed = "0";

            var privateBoolOffsets = new List<int>();

            foreach (var line in model.Enumerate())
            {
                if (Utils.IsComment(line))
                    continue;

                var enc = R.PublicEncField().Match(line);
                if (enc.Success)
                {
                    string typeName = enc.Groups[1].Value;

                    var om = R.Offset().Match(line);
                    if (om.Success)
                    {
                        string off = om.Groups[1].Value;

                        if (typeName == encType)
                            net = off;
                        else
                            parent = off;
                    }
                }
            }

            foreach (var raw in model.Enumerate())
            {
                var m = R.PrivateBool().Match(raw);
                if (!m.Success)
                    continue;

                var off = R.Offset().Match(raw);
                if (!off.Success)
                    continue;

                if (int.TryParse(off.Groups[1].Value,
                    System.Globalization.NumberStyles.HexNumber,
                    null, out int offInt))
                {
                    privateBoolOffsets.Add(offInt);
                }
            }

            privateBoolOffsets.Sort();

            if (privateBoolOffsets.Count == 2)
            {
                int small = privateBoolOffsets[0];
                int large = privateBoolOffsets[1];
                destroyed = $"{large:X}; // also has 0x{small:X}";
            }

            return (net, parent, destroyed);
        }
    }
}

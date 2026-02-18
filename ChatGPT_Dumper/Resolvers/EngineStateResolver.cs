using ChatGPT_Dumper.Classes;
using ChatGPT_Dumper.Helpers;
using ChatGPT_Dumper.Utility;

namespace ChatGPT_Dumper.Resolvers
{
    public sealed class EngineStateResolver(
        string[] lines,
        Dictionary<string, ClassModel> classes,
        Dictionary<string, Dictionary<string, string>> output)
    {
        private readonly string[] _lines = lines;
        private readonly Dictionary<string, ClassModel> _classes = classes;
        private readonly Dictionary<string, Dictionary<string, string>> _output = output;

        public void Run()
        {
            string encParam = FindEncryptedFlagsName();
            if (encParam == "")
            {
                Utils.Out("// ModelState: EngineState param not found");
                return;
            }

            string offset = FindRealFlagsOffset(encParam);
            if (offset == "0")
            {
                Utils.Out("// ModelState: flags offset not found");
                return;
            }

            Utils.Out("namespace ModelState {");
            Utils.Out($"    constexpr auto flags = 0x{offset}; // public int {encParam};");
            Utils.Out("};");
            Utils.Out();
        }

        // Step 1: Find %encryptedFlagsName in EngineState method signature
        private string FindEncryptedFlagsName()
        {
            foreach (var line in _lines)
            {
                var m = R.EngineStateMethod().Match(line);
                if (m.Success)
                    return m.Groups[1].Value;
            }
            return "";
        }

        // Step 2: Scan classes to find "public int %encryptedName;"
        private string FindRealFlagsOffset(string encFieldName)
        {
            var pattern = "public int " + encFieldName;

            for (int i = 0; i < _lines.Length; i++)
            {
                if (!_lines[i].TrimStart().StartsWith("public class "))
                    continue;

                int start = i;
                int end = ClassParser.FindClassEnd(_lines, start);
                if (end == -1)
                    continue;

                for (int j = start; j <= end; j++)
                {
                    string line = _lines[j].Trim();
                    if (line.Contains(pattern))
                    {
                        var m = R.Offset().Match(line);
                        if (m.Success)
                            return m.Groups[1].Value;
                    }
                }
            }

            return "0";
        }
    }
}

using System.Text.RegularExpressions;
using ChatGPT_Dumper.Classes;
using ChatGPT_Dumper.Helpers;
using ChatGPT_Dumper.Utility;

namespace ChatGPT_Dumper.Resolvers
{
    public sealed class PlayerModelResolver(
        string[] lines,
        Dictionary<string, ClassModel> classes,
        Dictionary<string, Dictionary<string, string>> output)
    {
        private readonly string[] _lines = lines;
        private readonly Dictionary<string, ClassModel> _classes = classes;
        private readonly Dictionary<string, Dictionary<string, string>> _output = output;

        public void Run()
        {
            string vecName = FindVector3ParamName();
            if (vecName == "")
                return;

            string offset = FindPlayerModelOffset(vecName);
            if (offset == "0")
                return;

            Utils.Out("namespace PlayerModel {");
            Utils.Out($"    constexpr auto newVelocity = 0x{offset}; // private Vector3 % from ViewmodelBob link");
            Utils.Out("};");
            Utils.Out();
        }

        private string FindVector3ParamName()
        {
            int start = ClassParser.FindClassStart(_lines,
                "public class ViewmodelBob");
            if (start == -1)
                return "";

            int end = ClassParser.FindClassEnd(_lines, start);

            for (int i = start; i <= end; i++)
            {
                var m = R.VmMethod().Match(_lines[i]);
                if (m.Success)
                    return m.Groups[1].Value;
            }

            return "";
        }

        private string FindPlayerModelOffset(string encName)
        {
            var reg = new Regex(
                @"private\s+(UnityEngine\.)?Vector3\s+" +
                Regex.Escape(encName) + ";");

            foreach (var line in _lines)
            {
                string trimmed = line.Trim();
                if (Utils.IsComment(trimmed))
                    continue;

                if (reg.IsMatch(trimmed))
                {
                    var m = R.Offset().Match(trimmed);
                    if (m.Success)
                        return m.Groups[1].Value;
                }
            }

            return "0";
        }
    }
}

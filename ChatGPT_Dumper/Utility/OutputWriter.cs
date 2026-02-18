using ChatGPT_Dumper.Classes;

namespace ChatGPT_Dumper.Utility
{
    public static class OutputWriter
    {
        public static void PrintAll(Dictionary<string, Dictionary<string, string>> output)
        {
            foreach (var kvp in output)
            {
                string className = kvp.Key;
                var offsets = kvp.Value;

                Utils.Out($"namespace {className} {{");
                foreach (var f in offsets)
                {
                    Utils.Out($"    constexpr auto {f.Key} = 0x{f.Value}; // {ResolveComment(className, f.Key)}");
                }
                Utils.Out("};");
                Utils.Out();
            }
        }

        private static string ResolveComment(string className, string constName)
        {
            if (!DumperConfig.FieldPatterns.TryGetValue(className, out var list))
                return "";

            foreach (var p in list)
                if (p.ConstName == constName)
                    return p.Comment;

            return "";
        }
    }
}

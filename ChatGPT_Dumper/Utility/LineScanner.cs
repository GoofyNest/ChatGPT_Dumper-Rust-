using ChatGPT_Dumper.Classes;

namespace ChatGPT_Dumper.Utility
{
    public static class LineScanner
    {
        public static string[] LoadDumpLines()
        {
            if (!File.Exists(DumperConfig.DumpPath))
            {
                Utils.Out("Dump.cs not found:");
                Utils.Out(DumperConfig.DumpPath);
                return [];
            }

            return File.ReadAllLines(DumperConfig.DumpPath);
        }

        public static string[] LoadScriptLines()
        {
            if (!File.Exists(DumperConfig.ScriptPath))
            {
                Utils.Out("script.json not found:");
                Utils.Out(DumperConfig.ScriptPath);
                return [];
            }

            return File.ReadAllLines(DumperConfig.ScriptPath);
        }
    }
}

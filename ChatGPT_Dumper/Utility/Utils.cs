using ChatGPT_Dumper.Classes;

namespace ChatGPT_Dumper.Utility
{
    public static class Utils
    {
        public static void Out(string msg = "")
        {
            if (DumperConfig.HideHelpers && msg.Contains("//"))
                msg = msg.Split('/')[0].TrimEnd();

            Console.WriteLine(msg);
        }

        public static bool IsComment(string line)
            => string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//");
    }
}

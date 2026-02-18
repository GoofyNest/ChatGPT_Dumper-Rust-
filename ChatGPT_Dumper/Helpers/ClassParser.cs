using ChatGPT_Dumper.Classes;

namespace ChatGPT_Dumper.Helpers
{
    public static class ClassParser
    {
        public static int FindClassStart(string[] lines, string header)
        {
            string target = header.Trim();

            for (int i = 0; i < lines.Length; i++)
            {
                var t = lines[i].TrimStart();
                if (t.StartsWith(target, StringComparison.Ordinal))
                    return i;
            }
            return -1;
        }

        public static int FindClassEnd(string[] lines, int start)
        {
            int depth = 0;
            bool inside = false;

            for (int i = start; i < lines.Length; i++)
            {
                foreach (char c in lines[i])
                {
                    if (c == '{')
                    {
                        depth++;
                        inside = true;
                    }
                    else if (c == '}')
                    {
                        depth--;
                    }
                }

                if (inside && depth == 0)
                    return i;
            }
            return -1;
        }

        public static ClassModel? ParseClass(string[] lines, string header)
        {
            int start = FindClassStart(lines, header);
            if (start == -1)
                return null;

            int end = FindClassEnd(lines, start);
            if (end == -1)
                return null;

            var block = new List<string>();
            for (int i = start; i <= end; i++)
                block.Add(lines[i]);

            // class name is the dictionary key, so we pass that in Program.cs
            return new ClassModel(header, start, end, block);
        }
    }
}

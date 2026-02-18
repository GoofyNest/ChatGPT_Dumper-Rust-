namespace ChatGPT_Dumper.Classes
{
    public sealed class ClassModel(string name, int start, int end, List<string> lines)
    {
        public string Name { get; } = name;
        public int Start { get; } = start;
        public int End { get; } = end;

        public string Namespace { get; set; }

        public readonly List<string> Lines = lines;

        public IEnumerable<string> Enumerate()
        {
            foreach (var l in Lines)
                yield return l.Trim();
        }
    }
}

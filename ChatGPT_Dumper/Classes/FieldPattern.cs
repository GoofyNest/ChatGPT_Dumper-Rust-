using System.Text.RegularExpressions;

namespace ChatGPT_Dumper.Classes
{
    public sealed class FieldPattern(
        string constName,
        string pattern,
        string comment,
        RegexOptions options = RegexOptions.None,
        int occurrenceIndex = 0)
    {
        public string ConstName { get; } = constName;
        public string Comment { get; } = comment;
        public Regex Regex { get; } = new Regex(pattern, options);
        public int OccurrenceIndex { get; } = occurrenceIndex;
        public int SeenCount;
    }
}

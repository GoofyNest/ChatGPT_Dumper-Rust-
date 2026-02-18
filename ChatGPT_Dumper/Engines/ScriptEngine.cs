using System.Collections.Generic;
using System.Text.RegularExpressions;
using ChatGPT_Dumper.Classes;

namespace ChatGPT_Dumper.Engines
{
    public sealed class ScriptEngine(string[] lines)
    {
        private readonly string[] _lines = lines;
        private readonly List<ScriptEntry> _entries = [];

        public IReadOnlyList<ScriptEntry> Entries => _entries;

        public void Run()
        {
            if (_lines.Length == 0)
                return;

            ParseEntries();

            var byNamespace = new Dictionary<ScriptNamespace, List<(ScriptSymbolPattern pattern, ScriptEntry entry)>>();

            string? baseNetworkAble = Program.encryptedValues?.Find(m => m.Name == "BaseNetworkable")?.Value;
            if(!string.IsNullOrEmpty(baseNetworkAble))
            {
                ScriptConfig.Symbols.Add(new ScriptSymbolPattern { MatchSignature = $"BaseNetworkable_{baseNetworkAble}_c*", ConstName = "BaseNetworkable_TypeInfo", Namespace = ScriptNamespace.S });
            }

            string? baseNetworkAble2 = Program.encryptedValues?.Find(m => m.Name == "BaseNetworkable2")?.Value;
            string? baseNetworkAble3 = Program.encryptedValues?.Find(m => m.Name == "BaseNetworkable3")?.Value;
            if (!string.IsNullOrEmpty(baseNetworkAble2) && !string.IsNullOrEmpty(baseNetworkAble3))
            {
                ScriptConfig.Symbols.Add(new ScriptSymbolPattern { MatchName = $"{baseNetworkAble2}.{baseNetworkAble3}_TypeInfo", ConstName = "GameManager_TypeInfo", Namespace = ScriptNamespace.S });
            }

            var foliage = Program.encryptedValues?.FindAll(m => !string.IsNullOrEmpty(m.Name) && m.Name.StartsWith("Foliage"));

            if (foliage != null)
            {
                foreach (var d in foliage)
                {
                    ScriptConfig.Symbols.Add(new ScriptSymbolPattern { MatchName = $"PlayerEyes$${d.Value}", ConstName = "PlayerEyes_GetPosition", Namespace = ScriptNamespace.F });
                }
            }

            foreach (var pattern in ScriptConfig.Symbols)
            {
                ScriptEntry? match = null;

                if (!string.IsNullOrEmpty(pattern.MatchName))
                {
                    if (pattern.IsRegex)
                    {
                        var rx = new Regex(pattern.MatchName, RegexOptions.Compiled);
                        match = _entries.FirstOrDefault(e => e.Name is not null && rx.IsMatch(e.Name));
                    }
                    else
                    {
                        match = _entries.FirstOrDefault(e => e.Name == pattern.MatchName);
                    }
                }
                else if(!string.IsNullOrEmpty(pattern.MatchSignature))
                {
                    if (pattern.IsRegex)
                    {
                        var rx = new Regex(pattern.MatchSignature, RegexOptions.Compiled);
                        match = _entries.FirstOrDefault(e => e.Signature is not null && rx.IsMatch(e.Signature));
                    }
                    else
                    {
                        match = _entries.FirstOrDefault(e => e.Signature == pattern.MatchSignature);
                    }
                }

                if (match == null)
                    continue;

                if (!byNamespace.TryGetValue(pattern.Namespace, out var list))
                {
                    list = [];
                    byNamespace[pattern.Namespace] = list;
                }

                list.Add((pattern, match));
            }

            // Output F namespace
            if (byNamespace.TryGetValue(ScriptNamespace.F, out var fList))
            {
                Console.WriteLine("namespace F {");
                foreach (var (pattern, entry) in fList)
                {
                    Console.WriteLine($"    constexpr auto {pattern.ConstName} = 0x{entry.Address:X};");
                }
                Console.WriteLine("}");
            }

            // Output S namespace
            if (byNamespace.TryGetValue(ScriptNamespace.S, out var sList))
            {
                Console.WriteLine("namespace S {");
                foreach (var (pattern, entry) in sList)
                {
                    Console.WriteLine($"    constexpr auto {pattern.ConstName} = 0x{entry.Address:X};");
                }
                Console.WriteLine("}");
                Console.WriteLine();
            }
        }

        private void ParseEntries()
        {
            ScriptEntry current = null!;
            bool hasCurrent = false;

            void CommitCurrent()
            {
                if (!hasCurrent)
                    return;

                // Only keep entries that at least have an address and a name
                if (current.Address != 0 && !string.IsNullOrEmpty(current.Name))
                    _entries.Add(current);

                hasCurrent = false;
            }

            foreach (var rawLine in _lines)
            {
                var line = rawLine.Trim();
                if (line.Length == 0)
                    continue;

                // Address starts a new block. If we already have one, commit it first.
                var addrMatch = R.ScriptAddress().Match(line);
                if (addrMatch.Success)
                {
                    CommitCurrent();

                    current = new ScriptEntry();
                    hasCurrent = true;

                    if (long.TryParse(addrMatch.Groups[1].Value, out var addr))
                        current.Address = addr;

                    continue;
                }

                if (!hasCurrent)
                    continue;

                var nameMatch = R.ScriptName().Match(line);
                if (nameMatch.Success)
                {
                    current.Name = nameMatch.Groups[1].Value;
                    continue;
                }

                var sigMatch = R.ScriptSignature().Match(line);
                if (sigMatch.Success)
                {
                    current.Signature = sigMatch.Groups[1].Value;
                    continue;
                }

                var typeSigMatch = R.ScriptTypeSignature().Match(line);
                if (typeSigMatch.Success)
                {
                    current.TypeSignature = typeSigMatch.Groups[1].Value;
                    continue;
                }

                // Treat a closing brace as a soft end of entry.
                // We do not care about commas or outer array brackets.
                if (line.StartsWith('}'))
                {
                    CommitCurrent();
                }
            }

            // Flush last pending entry
            CommitCurrent();
        }
    }

    public sealed class ScriptEntry
    {
        public long Address { get; set; }
        public string Name { get; set; } = "";
        public string Signature { get; set; } = "";
        public string TypeSignature { get; set; } = "";
    }
}

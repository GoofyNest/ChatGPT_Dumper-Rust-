using ChatGPT_Dumper.Classes;
using ChatGPT_Dumper.Helpers;
using ChatGPT_Dumper.Resolvers;
using ChatGPT_Dumper.Utility;

namespace ChatGPT_Dumper.Engines
{
    public sealed class DumperEngine(string[] lines)
    {
        private readonly string[] _lines = lines;

        // All parsed classes by name → ClassModel
        private readonly Dictionary<string, ClassModel> _classes = [];

        // Output: class → (constant name → offset)
        private readonly Dictionary<string, Dictionary<string, string>> _output =
            [];

        private void FindBaseNetworkableNames()
        {
            string? baseNetworkable2 = null;
            bool haveBaseNetworkable3 = false;

            foreach (var line in _lines)
            {
                // 1) First: find the initial BaseNetworkable encrypted class once
                if (baseNetworkable2 == null)
                {
                    var m = R.BaseNetworkableClass().Match(line);
                    if (m.Success)
                    {
                        // Whatever you previously used as "BaseNetworkable2"
                        // If you used Replace("%", "_"), keep it here.
                        baseNetworkable2 = m.Groups[1].Value;

                        Program.encryptedValues.Add(new ClassNames
                        {
                            Name = "BaseNetworkable2",
                            Value = baseNetworkable2
                        });

                        // No "continue" needed strictly, but it avoids falling into the second part
                        continue;
                    }
                }

                // 2) Once we know BaseNetworkable2, look for:
                //    public static class <BaseNetworkable2>.<encrypted>
                if (!haveBaseNetworkable3 && baseNetworkable2 != null &&
                    line.StartsWith("public static class "))
                {
                    // Strip the prefix
                    const string prefix = "public static class ";
                    var afterClass = line[prefix.Length..].Trim();

                    // We expect: "<BaseNetworkable2>.<Something> {"
                    if (!afterClass.StartsWith(baseNetworkable2 + "."))
                        continue;

                    // Extract the part after "<BaseNetworkable2>."
                    var dotIndex = afterClass.IndexOf('.');
                    if (dotIndex < 0)
                        continue;

                    // Up to first space or '{'
                    var spaceIndex = afterClass.IndexOfAny(new[] { ' ', '{' }, dotIndex + 1);
                    if (spaceIndex < 0)
                        spaceIndex = afterClass.Length;

                    var encPart = afterClass.Substring(dotIndex + 1, spaceIndex - dotIndex - 1).Trim();
                    if (encPart.Length == 0)
                        continue;

                    Program.encryptedValues.Add(new ClassNames
                    {
                        Name = "BaseNetworkable3",
                        Value = encPart
                    });

                    haveBaseNetworkable3 = true;

                    // You can break if you know there is only one such class
                    // if (haveBaseNetworkable3) break;
                }
            }
        }

        private string FindFoliageClass()
        {
            for (int i = 0; i < _lines.Length; i++)
            {
                var ctorMatch = R.FoliageCtor().Match(_lines[i]);
                if (!ctorMatch.Success)
                    continue;

                // We found the unique ctor line, now walk backwards
                for (int j = i; j >= 0; j--)
                {
                    var structMatch = R.FoliageStructHeader().Match(_lines[j]);
                    if (structMatch.Success)
                    {
                        return "FoliageGrid." + structMatch.Groups[1].Value;
                    }
                }

                // If we did not find the struct going up, no need to keep searching
                break;
            }

            return "";
        }

        private void ProcessFoliage(string foliageClassName)
        {
            if (string.IsNullOrWhiteSpace(foliageClassName))
                return;

            int d = 0;
            // e.g. foliageClassName = "FoliageGrid.%be98a52c..."
            string header = "private struct " + foliageClassName;

            int start = ClassParser.FindClassStart(_lines, header);
            if (start == -1)
                return;

            int end = ClassParser.FindClassEnd(_lines, start);
            if (end == -1)
                return;

            for (int i = start; i <= end; i++)
            {
                var ctorMatch = R.FoliageCtor().Match(_lines[i]);
                if (ctorMatch.Success)
                {
                    // do ctor stuff here
                    // Console.WriteLine("CTOR: " + _lines[i]);
                }

                var vecMatch = R.PublicVec3Method().Match(_lines[i]);
                if (vecMatch.Success)
                {
                    Program.encryptedValues.Add(new ClassNames
                    {
                        Name = "Foliage"+d,
                        Value = vecMatch.Groups[1].Value
                    });

                    d++;
                }
            }
        }

        private void BruteForce()
        {
            // 1) Find the Dictionary<string, Vector3> field and grab its encrypted type name
            string itemEntryType = "";
            for (int i = 0; i < _lines.Length; i++)
            {
                var m = R.DictVec3().Match(_lines[i]);
                if (m.Success)
                {
                    // %encrypted type used by Dictionary<string, Vector3>
                    itemEntryType = m.Groups[1].Value;
                    break;
                }
            }

            if (itemEntryType == "")
                return;

            // 2) Find where this type is used as public List<itemEntryType>
            int listLineIndex = -1;
            for (int i = 0; i < _lines.Length; i++)
            {
                // you can use a regex, but string.Contains is enough here
                if (_lines[i].Contains("public List<") && _lines[i].Contains(itemEntryType))
                {
                    listLineIndex = i;
                    break;
                }
            }

            if (listLineIndex == -1)
                return;

            // 3) Walk backwards to the ItemContainer class header: public class %HASH : IDisposable
            string itemContainerClassName = "";
            for (int i = listLineIndex; i >= 0; i--)
            {
                var m = R.ItemContainerClassHeader().Match(_lines[i]);
                if (m.Success)
                {
                    itemContainerClassName = m.Groups[1].Value; // %4f1b8b...
                    break;
                }
            }

            if (itemContainerClassName == "")
                return;

            // At this point we have the encrypted ItemContainer class name.
            // 4) Get the class body range
            int start = ClassParser.FindClassStart(_lines, "public class " + itemContainerClassName);
            if (start == -1)
                return;

            int end = ClassParser.FindClassEnd(_lines, start);

            // 5) Inside ItemContainer, find all public List<%ENC> fields
            var candidateEntryClasses = new HashSet<string>();

            for (int i = start; i <= end; i++)
            {
                var m = R.ListOfEncrypted().Match(_lines[i]);
                if (m.Success)
                {
                    string encClass = m.Groups[1].Value; // %encryptedClass from List<%encryptedClass>
                    candidateEntryClasses.Add(encClass);
                }
            }

            if (candidateEntryClasses.Count == 0)
                return;

            // 6) For each candidate entry class, inspect its fields for 2 bool, 1 string, 1 encrypted value
            foreach (var encClass in candidateEntryClasses)
            {
                int cs = ClassParser.FindClassStart(_lines, "public class " + encClass);
                if (cs == -1)
                    continue;

                int ce = ClassParser.FindClassEnd(_lines, cs);

                int publicBoolCount = 0;
                int privateBoolCount = 0;
                int stringCount = 0;
                int encryptedFieldCount = 0;

                for (int i = cs; i <= ce; i++)
                {
                    string t = _lines[i].Trim();

                    if (t.StartsWith("// Methods"))
                        break;

                    if (t.StartsWith("public class"))
                        continue;

                    if (Utils.IsComment(t))
                        continue;

                    if (t.StartsWith("public bool "))
                        publicBoolCount++;
                    else if (t.StartsWith("private bool "))
                        privateBoolCount++;
                    else if (t.StartsWith("public string "))
                        stringCount++;
                    else if (R.EncryptedField().IsMatch(t))
                        encryptedFieldCount++;


                }

                if (publicBoolCount == 1 && privateBoolCount == 1 && stringCount == 1 && encryptedFieldCount == 1)
                {
                    // Now get the offset of the List<encClass> field inside the ItemContainer class
                    string itemListOffset = "0";

                    for (int i = start; i <= end; i++)
                    {
                        string line = _lines[i];

                        // Find: public List<%encClass> %something; // 0xOFFSET
                        if (line.Contains("public List<" + encClass + ">"))
                        {
                            itemListOffset = Grab(line);

                            Program.ItemContainer = itemListOffset;
                            break;
                        }
                    }

                    return;
                }
            }
        }

        private static string Grab(string line)
        {
            var m = R.Offset().Match(line);
            return m.Success ? m.Groups[1].Value : "0";
        }

        public void Run()
        {
            if (_lines.Length == 0)
                return;

            ParseAllConfiguredClasses();
            
            RunStandardPatterns();
            
            BruteForce();

            RunSpecialResolvers();

            FindBaseNetworkableNames();
            ProcessFoliage(FindFoliageClass());
            
            //Utils.Out("Output:\n");

            OutputWriter.PrintAll(_output);
        }

        // --------------------------------------------------------------
        // CLASS PARSING
        // --------------------------------------------------------------
        private void ParseAllConfiguredClasses()
        {
            foreach (var kvp in DumperConfig.ClassHeaders)
            {
                string className = kvp.Key;
                string header = kvp.Value;

                var model = ClassParser.ParseClass(_lines, header);

                if (model == null)
                {
                    Utils.Out($"// Class {className} not found.");
                    Utils.Out();
                    continue;
                }

                _classes[className] = model;
            }
        }

        // --------------------------------------------------------------
        // STANDARD FIELD-PATTERN RESOLUTION
        // --------------------------------------------------------------
        private void RunStandardPatterns()
        {
            foreach (var kvp in DumperConfig.FieldPatterns)
            {
                string className = kvp.Key;
                var patterns = kvp.Value;

                if (!_classes.TryGetValue(className, out var model))
                {
                    Utils.Out($"// Skipping {className} (class not found)");
                    continue;
                }

                var offsets = new Dictionary<string, string>();
                foreach (var p in patterns)
                    offsets[p.ConstName] = "0";

                foreach (var line in model.Enumerate())
                {
                    if (Utils.IsComment(line))
                        continue;

                    foreach (var p in patterns)
                    {
                        if (offsets[p.ConstName] != "0")
                            continue;

                        if (p.Regex.IsMatch(line))
                        {
                            if (p.SeenCount == p.OccurrenceIndex)
                            {
                                var m = R.Offset().Match(line);
                                if (m.Success)
                                    offsets[p.ConstName] = m.Groups[1].Value;
                            }
                            p.SeenCount++;
                        }
                    }
                }

                _output[className] = offsets;
            }
        }

        // --------------------------------------------------------------
        // SPECIAL RESOLVERS
        // --------------------------------------------------------------
        private void RunSpecialResolvers()
        {
            new ItemContainerResolver(_lines, _classes, _output).Run();
            new BaseNetworkableResolver(_lines, _classes, _output).Run();
            new TodSkyResolver(_lines, _classes, _output).Run();
            new EngineStateResolver(_lines, _classes, _output).Run();
            new ItemResolver(_lines, _classes, _output).Run();
            new PlayerModelResolver(_lines, _classes, _output).Run();
        }
    }
}

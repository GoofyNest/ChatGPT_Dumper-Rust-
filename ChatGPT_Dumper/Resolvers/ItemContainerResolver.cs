using System.Text.RegularExpressions;
using ChatGPT_Dumper.Classes;
using ChatGPT_Dumper.Helpers;
using ChatGPT_Dumper.Utility;

namespace ChatGPT_Dumper.Resolvers
{
    public sealed class ItemContainerResolver(
        string[] lines,
        Dictionary<string, ClassModel> classes,
        Dictionary<string, Dictionary<string, string>> output)
    {
        private readonly string[] _lines = lines;
        public void Run()
        {
            var param = FindItemContainerCtorLastIntParam();
            //Console.WriteLine(param);

            var (encryptedClassName, classStart, classEnd) = FindItemContainerClass();
            if (encryptedClassName is null || classStart < 0 || classEnd < 0)
                return;

            // uid fields (%1680... and %0acff... inside this class)
            var uidFields = FindFieldsInsideItemContainer(encryptedClassName, classStart, classEnd);
            uidFields = OrderUidFieldsByEquatableUsage(uidFields);

            // capacity ints inside THIS class
            var capacityOffsets = FindCapacityOffsetsInClass(classStart, classEnd, param);

            // itemList: public List<%...> %field; // 0xNN
            var itemList = FindItemListInClass(classStart, classEnd);

            Utils.Out("namespace ItemContainer {");
            Utils.Out($"    //public sealed class {encryptedClassName}");

            // uid
            if (uidFields.Count == 1)
            {
                var f = uidFields[0];
                Utils.Out($"    constexpr auto uid = 0x{f.OffsetHex}; // {f.EncryptedFieldName}");
            }
            else if (uidFields.Count >= 2)
            {
                var main = uidFields[0];
                var other = uidFields[1];
                Utils.Out($"    constexpr auto uid = 0x{main.OffsetHex}; // or 0x{other.OffsetHex}");
            }

            // capacity
            if (capacityOffsets.Count == 1)
            {
                Utils.Out($"    constexpr auto capacity = 0x{capacityOffsets[0]};");
            }
            else if (capacityOffsets.Count >= 2)
            {
                var first = capacityOffsets[0];
                var extras = string.Join(" or ",
                    capacityOffsets.Skip(1).Select(o => $"0x{o}"));

                Utils.Out($"    constexpr auto capacity = 0x{first}; // or {extras}");
            }

            // itemList
            if (itemList.HasValue)
            {
                var (fieldName, offsetHex) = itemList.Value;
                Utils.Out($"    constexpr auto itemList = 0x{offsetHex}; // {fieldName}");
            }

            Utils.Out("};");
            Utils.Out();
        }

        private List<FieldInfo> OrderUidFieldsByEquatableUsage(List<FieldInfo> fields)
        {
            if (fields.Count <= 1)
                return fields;

            return fields
                .OrderByDescending(f => IsUsedInIEquatable(f.EncryptedFieldName))
                .ToList();
        }

        private bool IsUsedInIEquatable(string encryptedName)
        {
            // Look for IEquatable<%xxxxxxxx...> anywhere in the file
            var pattern = new Regex(
                $@"IEquatable<{Regex.Escape(encryptedName)}>");

            foreach (var line in _lines)
            {
                if (pattern.IsMatch(line))
                    return true;
            }

            return false;
        }


        private readonly record struct FieldInfo(string EncryptedFieldName, string OffsetHex);

        private (string? className, int classStart, int classEnd) FindItemContainerClass()
        {
            var hashSetFieldRegex = new Regex(
                @"^\s*public\s+HashSet<ItemDefinition>\s+(\S+)\s*;",
                RegexOptions.Compiled);

            var sealedClassRegex = new Regex(
                @"^\s*public\s+sealed\s+class\s+(\S+)\s*:",
                RegexOptions.Compiled);

            for (int i = 0; i < _lines.Length; i++)
            {
                var trimmed = _lines[i].Trim();
                if (Utils.IsComment(trimmed))
                    continue;

                var fieldMatch = hashSetFieldRegex.Match(trimmed);
                if (!fieldMatch.Success)
                    continue;

                // walk upwards to class decl
                for (int j = i; j >= 0; j--)
                {
                    var line = _lines[j].Trim();
                    if (Utils.IsComment(line))
                        continue;

                    var classMatch = sealedClassRegex.Match(line);
                    if (!classMatch.Success)
                        continue;

                    var className = classMatch.Groups[1].Value;
                    int classStartIndex = j;
                    int classEndIndex = ClassParser.FindClassEnd(_lines, classStartIndex);
                    return (className, classStartIndex, classEndIndex);
                }
            }

            return (null, -1, -1);
        }

        private (string fieldName, string offsetHex)? FindItemListInClass(
    int classStartIndex,
    int classEndIndex)
        {
            // public List<%something> %fieldName; // 0xNN
            var regex = new Regex(
                @"^\s*public\s+List<%[^\s>]+>\s+(\S+)\s*;\s*//\s*0x([0-9a-fA-F]+)\b",
                RegexOptions.Compiled);

            for (int i = classStartIndex; i <= classEndIndex && i < _lines.Length; i++)
            {
                var trimmed = _lines[i].Trim();
                if (Utils.IsComment(trimmed))
                    continue;

                var m = regex.Match(trimmed);
                if (!m.Success)
                    continue;

                var fieldName = m.Groups[1].Value;
                var offsetHex = m.Groups[2].Value;
                return (fieldName, offsetHex);
            }

            return null;
        }

        private string? FindItemContainerCtorLastIntParam()
        {
            // public void .ctor(Mesh %hash1, Material %hash2, int %hash3, int %hash4, int %hash5)
            var regex = new Regex(
                @"^\s*public\s+void\s+\.ctor\(Mesh\s+(%[A-Za-z0-9]{40}),\s*Material\s+(%[A-Za-z0-9]{40}),\s*int\s+(%[A-Za-z0-9]{40}),\s*int\s+(%[A-Za-z0-9]{40}),\s*int\s+(%[A-Za-z0-9]{40})\)",
                RegexOptions.Compiled);

            foreach (var line in _lines)
            {
                var trimmed = line.Trim();
                if (Utils.IsComment(trimmed))
                    continue;

                var m = regex.Match(trimmed);
                if (!m.Success)
                    continue;

                // 5th capture group = last int %<encrypted>
                return m.Groups[5].Value;
            }

            return null;
        }


        private List<string> FindCapacityOffsetsInClass(
            int classStartIndex,
            int classEndIndex,
            string? ctorParamName)
        {
            // List of (encryptedFieldName, offsetHex)
            var list = new List<(string field, string offset)>();

            // public int %abcd...; // 0xNN
            var regex = new Regex(
                @"^\s*public\s+int\s+(%[^\s;]+)\s*;\s*//\s*0x([0-9a-fA-F]+)\b",
                RegexOptions.Compiled);

            for (int i = classStartIndex; i <= classEndIndex && i < _lines.Length; i++)
            {
                var trimmed = _lines[i].Trim();
                if (Utils.IsComment(trimmed))
                    continue;

                var m = regex.Match(trimmed);
                if (!m.Success)
                    continue;

                var fieldName = m.Groups[1].Value;
                var offsetHex = m.Groups[2].Value;

                list.Add((fieldName, offsetHex));
            }

            // If ctor param is known, sort so matching field is FIRST
            if (!string.IsNullOrEmpty(ctorParamName))
            {
                list = list
                    .OrderByDescending(x => x.field == ctorParamName) // put match first
                    .ThenBy(x => x.offset) // keep stable order
                    .ToList();
            }

            // Return only offsets in order
            return list.Select(x => x.offset).ToList();
        }


        private List<FieldInfo> FindFieldsInsideItemContainer(
            string encryptedClassName,
            int classStartIndex,
            int classEndIndex)
        {
            var result = new List<FieldInfo>();

            // More relaxed: allow anything after the offset comment
            // public <type> <field>; // 0xNN[anything...]
            var fieldWithOffsetRegex = new Regex(
                @"^\s*public\s+(\S+)\s+(\S+)\s*;\s*//\s*0x([0-9a-fA-F]+)\b",
                RegexOptions.Compiled);

            for (int i = classStartIndex; i <= classEndIndex && i < _lines.Length; i++)
            {
                var line = _lines[i];
                var trimmed = line.Trim();
                if (Utils.IsComment(trimmed))
                    continue;

                // Skip statics
                if (trimmed.StartsWith("public static "))
                    continue;

                var m = fieldWithOffsetRegex.Match(trimmed);
                if (!m.Success)
                    continue;

                var typeName = m.Groups[1].Value;
                var fieldName = m.Groups[2].Value;
                var offsetHex = m.Groups[3].Value;

                // Only encrypted object like types:
                // starts with '%' and no generics, nested types, or arrays
                if (!typeName.StartsWith("%"))
                    continue;

                if (typeName.Contains('<') || typeName.Contains('>') ||
                    typeName.Contains('.') || typeName.Contains('[') || typeName.Contains(']'))
                    continue;

                result.Add(new FieldInfo(fieldName, offsetHex));
            }

            return result;
        }

        private static string MakeIdentifierSafe(string encryptedName)
        {
            return "Field_" + encryptedName.TrimStart('%');
        }
    }
}

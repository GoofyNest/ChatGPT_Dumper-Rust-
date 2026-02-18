using System.Text.RegularExpressions;

namespace ChatGPT_Dumper
{
    public static partial class R
    {
        // public class %HASH : IDisposable
        [GeneratedRegex(@"public\s+class\s+(%[0-9a-fA-F]{40})\s*:\s*IDisposable")]
        public static partial Regex ItemContainerClassHeader();

        // public List<%HASH> %field;
        [GeneratedRegex(@"public\s+List<(%[0-9a-fA-F]{40})>\s+%[0-9a-fA-F]{40};")]
        public static partial Regex ListOfEncrypted();

        // public %HASH %field;
        [GeneratedRegex(@"public\s+(%[0-9a-fA-F]{40})\s+%[0-9a-fA-F]{40};")]
        public static partial Regex EncryptedField();

        [GeneratedRegex(@"//\s*0x([0-9A-Fa-f]+)")]
        public static partial Regex Offset();

        [GeneratedRegex(@"public\s+interface\s+(\%[0-9a-fA-F]{40})")]
        public static partial Regex IfaceHeader();

        [GeneratedRegex(@"public\s+abstract\s+void\s+%[0-9a-fA-F]{40}\s*\(\s*(%[0-9a-fA-F]{40})\s+%[0-9a-fA-F]{40},\s*%[0-9a-fA-F]{40}<%[0-9a-fA-F]{40}>\s+%[0-9a-fA-F]{40}\s*\)")]
        public static partial Regex AbstractIfaceVoid();

        [GeneratedRegex(@"public\s+(%[0-9a-fA-F]{40})\s+%[0-9a-fA-F]{40};")]
        public static partial Regex PublicEncField();

        [GeneratedRegex(@"private\s+bool\s+%[0-9a-fA-F]{40};")]
        public static partial Regex PrivateBool();

        [GeneratedRegex(@"private\s+bool\s+(%[0-9a-fA-F]{40});")]
        public static partial Regex MuscleBool();

        [GeneratedRegex(@"public\s+const\s+(%[0-9a-fA-F]+)\.Flag\s+HasParachute\s*=\s*32768;")]
        public static partial Regex HasParachute();

        [GeneratedRegex(@"public\s+(%[0-9a-fA-F]{40})\s+%[0-9a-fA-F]{40}\s*\(")]
        public static partial Regex EasterReturn();

        [GeneratedRegex(@"public\s+%[0-9a-fA-f]{40}\.EngineState<TOwner>\s+%[0-9a-fA-F]{40}\s*\(\s*BaseEntity\.Flags\s+(%[0-9a-fA-F]{40})\s*\)")]
        public static partial Regex EngineStateMethod();

        [GeneratedRegex(@"public\s+static\s+%[0-9a-fA-F]{40}<(%[0-9a-fA-F]{40})>\s+%[0-9a-fA-F]{40};")]
        public static partial Regex UidType();

        [GeneratedRegex(@"public\s+HashSet<GameObject>\s+(%[0-9a-fA-F]{40});")]
        public static partial Regex Contents();

        [GeneratedRegex(@"public\s+virtual\s+void\s+AddPunch\s*\(\s*Vector3\s+(%[0-9a-fA-F]{40})\s*,\s*float\s+%[0-9a-fA-F]{40}\s*\)")]
        public static partial Regex AddPunch();

        [GeneratedRegex(@"private\s+Vector3\s+(%[0-9a-fA-F]{40});")]
        public static partial Regex Pos();

        [GeneratedRegex(@"public\s+ItemDefinition\s+%[0-9a-fA-F]{40};")]
        public static partial Regex Info();

        [GeneratedRegex(@"private\s+float\s+%[0-9a-fA-F]{40};")]
        public static partial Regex PrivateFloat();

        [GeneratedRegex(@"private\s+(%[0-9a-fA-F]{40})\s+(%[0-9a-fA-F]{40});")]
        public static partial Regex Held();

        [GeneratedRegex(@"public\s+Dictionary<string,\s*Vector3>\s+(%[0-9a-fA-F]{40});")]
        public static partial Regex DictVec3();

        [GeneratedRegex(@"public\s+Dictionary<string,\s*MissionEntity>\s+(%[0-9a-fA-F]{40});")]
        public static partial Regex DictMission();

        [GeneratedRegex(@"public\s+%[0-9a-fA-F]{40}\s+(%[0-9a-fA-F]{40});")]
        public static partial Regex CapacityName();

        [GeneratedRegex(@"private\s+ViewmodelBob\.%[0-9a-fA-F]{40}\s+%[0-9a-fA-F]{40}\s*\(\s*Vector3\s+(%[0-9a-fA-F]{40})")]
        public static partial Regex VmMethod();

        [GeneratedRegex(@"^\s*""Address""\s*:\s*(\d+)", RegexOptions.Compiled)]
        public static partial Regex ScriptAddress();

        [GeneratedRegex(@"^\s*""Name""\s*:\s*""([^""]*)""", RegexOptions.Compiled)]
        public static partial Regex ScriptName();

        [GeneratedRegex(@"^\s*""Signature""\s*:\s*""([^""]*)""", RegexOptions.Compiled)]
        public static partial Regex ScriptSignature();

        [GeneratedRegex(@"^\s*""TypeSignature""\s*:\s*""([^""]*)""", RegexOptions.Compiled)]
        public static partial Regex ScriptTypeSignature();

        [GeneratedRegex(@"public\s+static\s+class\s+BaseNetworkable\.(%[0-9a-fA-F]{40})")]
        public static partial Regex BaseNetworkableClass();

        [GeneratedRegex(@"public\s+(%[0-9a-fA-F]{40})\s+(%[0-9a-fA-F]{40})\s*\{\s*get;\s*}")]
        public static partial Regex EncryptedProperty();

        [GeneratedRegex(@"public\s+void\s+\.ctor\s*\(\s*Camera\s+(%[0-9a-fA-F]{40})\s*,\s*Transform\s+(%[0-9a-fA-F]{40})\s*\)")]
        public static partial Regex FoliageCtor();

        [GeneratedRegex(@"private\s+struct\s+FoliageGrid\.(%[0-9a-fA-F]{40})")]
        public static partial Regex FoliageStructHeader();

        [GeneratedRegex(@"public\s+Vector3\s+(%[0-9a-fA-F]{40})\s*\(")]
        public static partial Regex PublicVec3Method();

    }
}

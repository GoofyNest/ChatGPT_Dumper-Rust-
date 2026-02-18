using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGPT_Dumper.Classes
{
    public enum ScriptNamespace
    {
        S, // Structs
        F  // Functions
    }

    public sealed class ScriptSymbolPattern
    {
        public string? MatchName { get; init; }  // exact name or regex source
        public string? MatchSignature { get; init; }  // exact name or regex source
        public bool IsRegex { get; init; } = false;

        public required ScriptNamespace Namespace { get; init; }
        public required string ConstName { get; init; }
    }

    public static class ScriptConfig
    {
        public static readonly List<ScriptSymbolPattern> Symbols =
        [
            new ScriptSymbolPattern
            {
                MatchName = "BasePlayer$$OnViewModeChanged",
                Namespace = ScriptNamespace.F,
                ConstName = "BasePlayer_OnViewModeChanged"
            },


            new ScriptSymbolPattern
            {
                MatchName = "SingletonComponent\\u003CCameraMan\\u003E_TypeInfo",
                Namespace = ScriptNamespace.S,
                ConstName = "SingletonComponentCameraMan_TypeInfo"
            },

            new ScriptSymbolPattern
            {
                MatchName = "BasePlayer_TypeInfo",
                Namespace = ScriptNamespace.S,
                ConstName = "BasePlayer_TypeInfo"
            },

            new ScriptSymbolPattern
            {
                MatchName = "BaseProjectile_TypeInfo",
                Namespace = ScriptNamespace.S,
                ConstName = "BaseProjectile_TypeInfo"
            },

            new ScriptSymbolPattern
            {
                MatchName = "PlayerWalkMovement_TypeInfo",
                Namespace = ScriptNamespace.S,
                ConstName = "PlayerWalkMovement_TypeInfo"
            },
        ];
    }
}

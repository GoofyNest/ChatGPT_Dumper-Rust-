using System.IO;
using ChatGPT_Dumper.Classes;

namespace ChatGPT_Dumper.Classes
{
    public class ClassNames
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
    }

    public static class DumperConfig
    {
        // Global path
        public const string Folder =
            @"C:\Users\user\Downloads\Il2CppDumper-win-v6.7.46";

        public static readonly string DumpPath = Path.Combine(Folder, "Dump.cs");
        public static readonly string ScriptPath = Path.Combine(Folder, "Script.json");
        public static readonly bool HideHelpers = false;

        // Class headers to match
        public static readonly Dictionary<string, string> ClassHeaders = new()
        {
            ["LootableCorpse"]    = "public class LootableCorpse : BaseCorpse, LootPanel.%",
            ["BasePlayer"]        = "public class BasePlayer : BaseCombatEntity",
            ["BaseMovement"]      = "public class BaseMovement : MonoBehaviour",
            ["BaseEntity"]        = "public class BaseEntity : BaseNetworkable, %",
            ["BaseProjectile"]    = "public class BaseProjectile : AttackEntity",
            ["Projectile"]        = "public class Projectile : ListComponent<Projectile>,",
            ["ItemModProjectile"] = "public class ItemModProjectile : MonoBehaviour",
            ["BaseCombatEntity"]  = "public class BaseCombatEntity : BaseEntity",
            ["Model"]             = "public class Model : MonoBehaviour, %",
            ["ItemDefinition"]    = "public class ItemDefinition : MonoBehaviour",
            ["PlayerInventory"]   = "public class PlayerInventory : EntityComponent<BasePlayer>,",
            ["TOD_Sky"]           = "public class TOD_Sky : MonoBehaviour",
            ["BaseNetworkable"]   = "public abstract class BaseNetworkable : BaseMonoBehaviour, %",
            ["PlayerInput"]       = "public class PlayerInput : EntityComponent<BasePlayer>",
        };

        public static readonly Dictionary<string, List<FieldPattern>> FieldPatterns =
            new()
            {
                // -------- LootableCorpse --------
                ["LootableCorpse"] =
                [
                new FieldPattern(
                    "_playerName",
                    @"public\s+string\s+%[0-9a-fA-F]{40};",
                    "public string % // _playerName"
                ),
                ],

                // -------- BasePlayer --------
                ["BasePlayer"] =
                [
                new FieldPattern(
                    "input",
                    @"public\s+PlayerInput\s+%[\w]+;",
                    "public PlayerInput %"
                ),
                new FieldPattern(
                    "playerModel",
                    @"public\s+PlayerModel\s+%[\w]+;",
                    "public PlayerModel %"
                ),
                new FieldPattern(
                    "movement",
                    @"public\s+BaseMovement\s+%[\w]+;",
                    "public BaseMovement %"
                ),
                new FieldPattern(
                    "_displayName",
                    @"protected\s+string\s+%[\w]+;",
                    "protected string %"
                ),
                new FieldPattern(
                    "UserIDString",
                    @"public\s+string\s+%[\w]+;",
                    "public string %",
                    occurrenceIndex: 0
                ),
                new FieldPattern(
                    "playerFlags",
                    @"BasePlayer\.PlayerFlags\s+playerFlags;",
                    "[InspectorFlags] public BasePlayer.PlayerFlags playerFlags;"
                ),
                new FieldPattern(
                    "currentTeam",
                    @"public\s+ulong\s+currentTeam;",
                    "public ulong currentTeam;"
                ),
                new FieldPattern(
                    "playerInventory",
                    @"private\s+%[0-9a-fA-F]{40}\s*<PlayerInventory>\s+%[0-9a-fA-F]{40}\s*;",
                    "private %<PlayerInventory> % // playerInventory"
                ),
                ],

                // -------- BaseMovement --------
                ["BaseMovement"] =
                [
                new FieldPattern(
                    "adminCheat",
                    @"public\s+bool\s+%[\w]+;",
                    "public bool adminCheat;",
                    occurrenceIndex: 0
                ),
                new FieldPattern(
                    "adminSpeed",
                    @"public\s+float\s+%[\w]+;",
                    "public float adminSpeed;",
                    occurrenceIndex: 0
                ),
                new FieldPattern(
                    "Ownerk__BackingField",
                    @"private\s+BasePlayer\s+%[\w]+;",
                    "private BasePlayer Ownerk__BackingField;",
                    occurrenceIndex: 0
                ),
                new FieldPattern(
                    "InheritedVelocityk__BackingField",
                    @"private\s+UnityEngine\.Vector3\s+%[\w]+;|private\s+Vector3\s+%[\w]+;",
                    "private Vector3 InheritedVelocityk__BackingField;",
                    occurrenceIndex: 0
                ),
                new FieldPattern(
                    "TargetMovementk__BackingField",
                    @"private\s+UnityEngine\.Vector3\s+%[\w]+;|private\s+Vector3\s+%[\w]+;",
                    "private Vector3 TargetMovementk__BackingField;",
                    occurrenceIndex: 1
                ),
                new FieldPattern(
                    "Runningk__BackingField",
                    @"private\s+float\s+%[\w]+;",
                    "private float Runningk__BackingField;",
                    occurrenceIndex: 0
                ),
                new FieldPattern(
                    "Duckingk__BackingField",
                    @"private\s+float\s+%[\w]+;",
                    "private float Duckingk__BackingField;",
                    occurrenceIndex: 1
                ),
                new FieldPattern(
                    "Crawlingk__BackingField",
                    @"private\s+float\s+%[\w]+;",
                    "private float Crawlingk__BackingField;",
                    occurrenceIndex: 2
                ),
                new FieldPattern(
                    "Groundedk__BackingField",
                    @"private\s+float\s+%[\w]+;",
                    "private float Groundedk__BackingField;",
                    occurrenceIndex: 3
                ),
                new FieldPattern(
                    "lastTeleportedTime",
                    @"private\s+float\s+%[\w]+;",
                    "private float lastTeleportedTime;",
                    occurrenceIndex: 4
                ),
                ],

                // -------- BaseEntity --------
                ["BaseEntity"] =
                [
                new FieldPattern(
                    "model",
                    @"public\s+Model\s+model;",
                    "public Model model;"
                )
                ],

                // -------- BaseProjectile --------
                ["BaseProjectile"] =
                [
                new FieldPattern(
                    "projectileVelocityScale",
                    @"public\s+float\s+projectileVelocityScale;",
                    "public float projectileVelocityScale;"
                ),
                new FieldPattern(
                    "automatic",
                    @"public\s+bool\s+automatic;",
                    "public bool automatic;"
                ),
                new FieldPattern(
                    "isBurstWeapon",
                    @"public\s+bool\s+isBurstWeapon;",
                    "public bool isBurstWeapon;"
                ),
                new FieldPattern(
                    "primaryMagazine",
                    @"public\s+BaseProjectile\.Magazine\s+primaryMagazine;",
                    "public BaseProjectile.Magazine primaryMagazine;"
                ),
                new FieldPattern(
                    "MuzzlePoint",
                    @"public\s+Transform\s+MuzzlePoint;",
                    "public Transform MuzzlePoint;"
                )
                ],

                // -------- Projectile --------
                ["Projectile"] =
                [
                new FieldPattern(
                    "initialVelocity",
                    @"public\s+UnityEngine\.Vector3\s+initialVelocity;|public\s+Vector3\s+initialVelocity;",
                    "public Vector3 initialVelocity;"
                ),
                new FieldPattern(
                    "drag",
                    @"public\s+float\s+drag;",
                    "public float drag;"
                ),
                new FieldPattern(
                    "gravityModifier",
                    @"public\s+float\s+gravityModifier;",
                    "public float gravityModifier;"
                ),
                new FieldPattern(
                    "thickness",
                    @"public\s+float\s+thickness;",
                    "public float thickness;"
                ),
                new FieldPattern(
                    "changeInitialOrientation",
                    @"public\s+bool\s+changeInitialOrientation;",
                    "public bool changeInitialOrientation;"
                ),
                new FieldPattern(
                    "initialDistance",
                    @"public\s+float\s+initialDistance;",
                    "public float initialDistance;"
                ),
                new FieldPattern(
                    "initialOrientation",
                    @"public\s+UnityEngine\.Vector3\s+initialOrientation;|public\s+Vector3\s+initialOrientation;",
                    "public Vector3 initialOrientation;"
                ),
                new FieldPattern(
                    "penetrationPower",
                    @"public\s+float\s+penetrationPower;",
                    "public float penetrationPower;"
                ),
                new FieldPattern(
                    "noGravity",
                    @"public\s+bool\s+%[\w]+;",
                    "public bool noGravity;",
                    occurrenceIndex: 4
                )
                ],

                // -------- ItemModProjectile --------
                ["ItemModProjectile"] =
                [
                new FieldPattern(
                    "projectileObject",
                    @"public\s+GameObjectRef\s+projectileObject;",
                    "public GameObjectRef projectileObject;"),
                new FieldPattern(
                    "mods",
                    @"public\s+ItemModProjectileMod\[\]\s+mods;",
                    "public ItemModProjectileMod[] mods;"),
                new FieldPattern(
                    "ammoType",
                    @"public\s+AmmoTypes\s+ammoType;",
                    "public AmmoTypes ammoType;"),
                new FieldPattern(
                    "numProjectiles",
                    @"public\s+int\s+numProjectiles;",
                    "public int numProjectiles;"),
                new FieldPattern(
                    "projectileSpread",
                    @"public\s+float\s+projectileSpread;",
                    "public float projectileSpread;"),
                new FieldPattern(
                    "projectileVelocity",
                    @"public\s+float\s+projectileVelocity;",
                    "public float projectileVelocity;"),
                new FieldPattern(
                    "projectileVelocitySpread",
                    @"public\s+float\s+projectileVelocitySpread;",
                    "public float projectileVelocitySpread;"),
                new FieldPattern(
                    "useCurve",
                    @"public\s+bool\s+useCurve;",
                    "public bool useCurve;"),
                new FieldPattern(
                    "spreadScalar",
                    @"public\s+AnimationCurve\s+spreadScalar;",
                    "public AnimationCurve spreadScalar;"),
                new FieldPattern(
                    "attackEffectOverride",
                    @"public\s+GameObjectRef\s+attackEffectOverride;",
                    "public GameObjectRef attackEffectOverride;"),
                new FieldPattern(
                    "barrelConditionLoss",
                    @"public\s+float\s+barrelConditionLoss;",
                    "public float barrelConditionLoss;"),
                new FieldPattern(
                    "category",
                    @"public\s+string\s+category;",
                    "public string category;")
                ],

                // -------- BaseCombatEntity --------
                ["BaseCombatEntity"] =
                [
                new FieldPattern(
                    "lifestate",
                    @"public\s+BaseCombatEntity\.LifeState\s+lifestate;",
                    "public BaseCombatEntity.LifeState lifestate;")
                ],

                // -------- Model --------
                ["Model"] =
                [
                new FieldPattern(
                    "boneTransforms",
                    @"public\s+Transform\[\]\s+boneTransforms;",
                    "public Transform[] boneTransforms;"
                )
                ],

                // -------- ItemDefinition --------
                ["ItemDefinition"] =
                [
                new FieldPattern(
                    "itemid",
                    @"public\s+int\s+itemid;",
                    "public int itemid;"
                ),
                new FieldPattern(
                    "shortname",
                    @"public\s+string\s+shortname;",
                    "public string shortname;"
                ),
                new FieldPattern(
                    "displayName",
                    @"public\s+%[0-9a-fA-F]{40}\.Phrase\s+displayName;",
                    "public Phrase displayName;"
                ),
                new FieldPattern(
                    "displayDescription",
                    @"public\s+%[0-9a-fA-F]{40}\.Phrase\s+displayDescription;",
                    "public Phrase displayDescription;"
                ),
                new FieldPattern(
                    "category",
                    @"public\s+ItemCategory\s+category;",
                    "public ItemCategory category;"
                ),
                new FieldPattern(
                    "condition",
                    @"public\s+ItemDefinition\.Condition\s+condition;",
                    "public ItemDefinition.Condition condition;"
                ),
                new FieldPattern(
                    "Children",
                    @"public\s+ItemDefinition\[\]\s+%[0-9a-fA-F]{40};",
                    "public ItemDefinition[] % // Children"
                ),
                ],

                // -------- PlayerInventory --------
                ["PlayerInventory"] =
                [
                new FieldPattern(
                    "container1",
                    @"public\s+%[0-9a-fA-F]{40}\s+%[0-9a-fA-F]{40};",
                    "public % % // container 1",
                    occurrenceIndex: 0
                ),
                new FieldPattern(
                    "container2",
                    @"public\s+%[0-9a-fA-F]{40}\s+%[0-9a-fA-F]{40};",
                    "public % % // container 2",
                    occurrenceIndex: 1
                ),
                new FieldPattern(
                    "container3",
                    @"public\s+%[0-9a-fA-F]{40}\s+%[0-9a-fA-F]{40};",
                    "public % % // container 3",
                    occurrenceIndex: 2
                ),
                new FieldPattern(
                    "crafting",
                    @"public\s+ItemCrafter\s+crafting;",
                    "public ItemCrafter crafting;"
                ),
                new FieldPattern(
                    "loot",
                    @"public\s+PlayerLoot\s+loot;",
                    "public PlayerLoot loot;"
                ),
                new FieldPattern(
                    "returnItems",
                    @"private\s+List<%[0-9a-fA-F]{40}>\s+%[0-9a-fA-F]{40};",
                    "private List<Item> % // returnItems",
                    occurrenceIndex: 0
                ),
                ],

                // -------- PlayerInput --------
                ["PlayerInput"] =
                [
                new FieldPattern(
                    "state",
                    @"public\s+%[0-9a-fA-F]{40}\s+%[0-9a-fA-F]{40};",
                    "public % % // state",
                    occurrenceIndex: 0
                ),
                new FieldPattern(
                    "bodyAngles",
                    @"private\s+(?:UnityEngine\.)?Vector3\s+%[0-9a-fA-F]{40};",
                    "private Vector3 % // bodyAngles",
                    occurrenceIndex: 0
                ),
                new FieldPattern(
                    "recoilAngles",
                    @"public\s+%[0-9a-fA-F]{40}<(?:(?:UnityEngine\.)?Vector3)>\s+%[0-9a-fA-F]{40};",
                    "public %<Vector3> % // recoilAngles",
                    occurrenceIndex: 0
                ),
                ],

                // -------- TOD_Sky --------
                ["TOD_Sky"] =
                [
                new FieldPattern(
                    "Initialized",
                    @"^$",
                    "private bool % // Initialized"
                ),
                new FieldPattern(
                    "CycleParameters",
                    @"public\s+TOD_CycleParameters\s+Cycle;",
                    "public TOD_CycleParameters Cycle;"
                ),
                new FieldPattern(
                    "Atmosphere",
                    @"public\s+TOD_AtmosphereParameters\s+Atmosphere;",
                    "public TOD_AtmosphereParameters Atmosphere;"
                ),
                new FieldPattern(
                    "Day",
                    @"public\s+TOD_DayParameters\s+Day;",
                    "public TOD_DayParameters Day;"
                ),
                new FieldPattern(
                    "Night",
                    @"public\s+TOD_NightParameters\s+Night;",
                    "public TOD_NightParameters Night;"
                ),
                new FieldPattern(
                    "Stars",
                    @"public\s+TOD_StarParameters\s+Stars;",
                    "public TOD_StarParameters Stars;"
                ),
                new FieldPattern(
                    "Ambient",
                    @"public\s+TOD_AmbientParameters\s+Ambient;",
                    "public TOD_AmbientParameters Ambient;"
                ),
                new FieldPattern(
                    "timeSinceAmbientUpdate",
                    @"^$",
                    "private float % // timeSinceAmbientUpdate"
                ),
                new FieldPattern(
                    "timeSinceReflectionUpdate",
                    @"^$",
                    "private float % // timeSinceReflectionUpdate"
                ),
                ],

                // -------- BaseNetworkable --------
                ["BaseNetworkable"] =
                [
                new FieldPattern(
                    "prefabID",
                    @"public\s+uint\s+prefabID;",
                    "public uint prefabID;"
                ),
                new FieldPattern(
                    "children",
                    @"public\s+readonly\s+List<BaseEntity>\s+%[0-9a-fA-F]{40};",
                    "public readonly List<BaseEntity> % // children"
                ),
                new FieldPattern(
                    "net",
                    @"^$",
                    "encrypted net field"
                ),
                new FieldPattern(
                    "parentEntity",
                    @"^$",
                    "encrypted parentEntity field"
                ),
                new FieldPattern(
                    "IsDestroyedk__BackingField",
                    @"^$",
                    "private bool % // IsDestroyedk__BackingField"
                ),
                ],
            };
    }
}

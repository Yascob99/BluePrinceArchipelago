#if Bep
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
#endif
#if ML
using MelonLoader;
using Il2Cpp;
#endif
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Archipelago.Commands;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
using UnityEngine;
using BluePrinceArchipelago.FsmMethods;
using HarmonyPatch = HarmonyLib.Harmony;
#if ML
[assembly: MelonInfo(typeof(BluePrinceArchipelago.Plugin), "BluePrinceArchipelago", "0.1.5", "Yascob", null)]
    [assembly: MelonGame("Dogubomb", "BLUE PRINCE")]
#endif
namespace BluePrinceArchipelago {
#if Bep
    /// <summary>
    ///     The base of the Plugin/Mod.
    /// </summary>
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BasePlugin
    {
#endif
#if ML
    public class Plugin : MelonMod
    {
#endif
        public const string PluginGUID = "com.Yascob.BluePrinceArchipelago";
        public const string PluginName = "BluePrinceArchipelago";
        public const string PluginVersion = "0.1.5";

        private static Plugin _instance;
        public static Plugin Instance => _instance;

        public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
        public const string APDisplayInfo = $"Archipelago v{ArchipelagoClient.APVersion}";
        public static AssetBundle AssetBundle { get; private set; }
#if Bep
        public ManualLogSource LogSource => Log;
        public HarmonyLib.Harmony HarmonyInstance => s_harmony;
        private static readonly HarmonyLib.Harmony s_harmony = new(PluginGUID);
#endif
        public static ArchipelagoClient ArchipelagoClient;
        public static GameObject ModObject;

        public static GameObject FsmMethods;
        public static UniqueItemManager UniqueItemManager;

        public static HarmonyPatch Harmony { get; } = new HarmonyPatch(PluginGUID);

        public void StartLoad() {
            Logging.SetLogLevel("Entrance Hall", LogLevel.Info);
            Logging.SetLogLevel("Cloister", LogLevel.Info);
            Logging.SetLogLevel("Basement", LogLevel.Info);
            Logging.SetLogLevel("The Well", LogLevel.Info);
            Logging.SetLogLevel("RoomHandler", LogLevel.Info);
            Logging.SetLogLevel("ArchipelagoOptions", LogLevel.Info);
            Logging.SetLogLevel("DeathLink", LogLevel.Info);
            Logging.SetLogLevel("ModRoomManager", LogLevel.Info);
            //Logging.SetLogLevel("Items", LogLevel.Info);
            Logging.SetLogLevel("Locations", LogLevel.Info);
            Logging.SetLogLevel("Trades", LogLevel.Info);
            Logging.SetLogLevel("Rooms", LogLevel.Info);
            //Logging.SetLogLevel("Events", LogLevel.Info);
            Logging.SetLogLevel("StatEvents", LogLevel.Info);
            Logging.SetLogLevel("Connection", LogLevel.Info);
            //Logging.SetLogLevel("APData", LogLevel.Info);
            Logging.SetLogLevel("ArchipelagoConsole", LogLevel.Info);
            //Logging.SetLogLevel("ItemQueue", LogLevel.Info);
            Logging.SetLogLevel("ArchipelagoEvents", LogLevel.Info);
            Logging.SetLogLevel("CustomFsmMethods", LogLevel.Info);

            // Plugin startup logic
            ArchipelagoClient = new ArchipelagoClient();
            UniqueItemManager = new UniqueItemManager();
            _instance = this;
            AssetBundle = AssetExtensions.LoadAssetBundleFromAssembly(AssetExtensions.GetResourceNameFromPath("assets/apprefabs"));
            Logging.LogWarning($"Plugin {PluginGUID} is loaded!");

            //Inject custom Object for Mod Handling
#if Bep
            ClassInjector.RegisterTypeInIl2Cpp<ModInstance>();
            ClassInjector.RegisterTypeInIl2Cpp<CustomFsmMethods>();
#endif
            ModObject = new GameObject("Archipelago");
            FsmMethods = new GameObject("CustomFsmMethods");

            GameObject.DontDestroyOnLoad(ModObject);
            ModObject.hideFlags = HideFlags.HideAndDontSave; //The mod breaks if this is removed. Unsure if different flags could be used to make this more visible.
            ModObject.AddComponent<ModInstance>();
            ModObject.AddComponent<PlayMakerFSM>(); //Add A PlayMakerFSM to be used for Events.
            FsmMethods.AddComponent<CustomFsmMethods>();
            FsmMethods.transform.parent = ModObject.transform;
            State.Initialize();
            ArchipelagoConsole.Awake();
            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");
            ArchipelagoConsole.UpdateWindow();
            CommandManager.initializeLocalCommands();
        }
#if Bep
        /// <summary>
        ///     Handles the initial load of the plugin (not the game).
        /// </summary>
        public override void Load()
        {
            StartLoad();
        }
#endif
#if ML
        public override void OnInitializeMelon()
        {
            StartLoad();
            LoggerInstance.Msg($"Plugin {PluginGUID} is loaded!");
        }
#endif
        }
}
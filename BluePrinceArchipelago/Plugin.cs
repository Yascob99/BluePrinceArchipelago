using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Rooms;
using BluePrinceArchipelago.Utils;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace BluePrinceArchipelago {

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BasePlugin
    {
        public const string PluginGUID = "com.Yascob.BluePrinceArchipelago";
        public const string PluginName = "BluePrinceArchipelago";
        public const string PluginVersion = "0.1.0";

        private static Plugin _instance;
        public static Plugin Instance => _instance;

        public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
        public const string APDisplayInfo = $"Archipelago v{ArchipelagoClient.APVersion}";
        public static AssetBundle AssetBundle { get; private set; }
        public ManualLogSource LogSource => Log;
        public static ArchipelagoClient ArchipelagoClient;
        public static GameObject ModObject;
        public static ModRoomManager ModRoomManager;
        public static ModItemManager ModItemManager;
        public static UniqueItemManager UniqueItemManager;
        public override void Load()
        {
            Logging.SetLogLevel("Entrance Hall", LogLevel.Info);
            Logging.SetLogLevel("Cloister", LogLevel.Info);
            Logging.SetLogLevel("Basement", LogLevel.Info);
            Logging.SetLogLevel("The Well", LogLevel.Info);
            Logging.SetLogLevel("RoomHandler", LogLevel.Info);
            Logging.SetLogLevel("ArchipelagoOptions", LogLevel.Info);
            Logging.SetLogLevel("DeathLink", LogLevel.Info);
            Logging.SetLogLevel("ModRoomManager", LogLevel.Info);
            //Logging.SetLogLevel("Items", LogLevel.Info);
            //Logging.SetLogLevel("Rooms", LogLevel.Info);
            //Logging.SetLogLevel("Events", LogLevel.Info);
            Logging.SetLogLevel("StatEvents", LogLevel.Info);
            Logging.SetLogLevel("Connection", LogLevel.Info);
            //Logging.SetLogLevel("APData", LogLevel.Info);
            Logging.SetLogLevel("ArchipelagoConsole", LogLevel.Info);
            //Logging.SetLogLevel("ItemQueue", LogLevel.Info);
            Logging.SetLogLevel("ArchipelagoEvents", LogLevel.Info);

            // Plugin startup logic
            ArchipelagoClient = new ArchipelagoClient();
            ModRoomManager = new ModRoomManager();
            ModItemManager = new ModItemManager();
            UniqueItemManager = new UniqueItemManager();
            _instance = this;
            AssetBundle = AssetBundle.LoadAssetFromAssembly(AssetExtensions.GetResourceNameFromPath("assets/apprefabs"));
            Log.LogInfo($"Plugin {PluginGUID} is loaded!");
            //Inject custom Object for Mod Handling
            ClassInjector.RegisterTypeInIl2Cpp<ModInstance>();
            ModObject = new GameObject("Archipelago");
            GameObject.DontDestroyOnLoad(ModObject);
            ModObject.hideFlags = HideFlags.HideAndDontSave; //The mod breaks if this is removed. Unsure if different flags could be used to make this more visible.
            ModObject.AddComponent<ModInstance>();
            ModObject.AddComponent<PlayMakerFSM>(); //Add A PlayMakerFSM to be used for Events.
            State.Initialize();
            ArchipelagoConsole.Awake();
            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");
            CommandManager.initializeLocalCommands();
        }
    }

}
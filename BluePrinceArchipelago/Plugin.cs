using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Core;
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
        public ManualLogSource LogSource => Log;
        public static ArchipelagoClient ArchipelagoClient;
        public static GameObject ModObject;
        public static ModRoomManager ModRoomManager;
        public static ModItemManager ModItemManager;
        public override void Load()
        {
            // Plugin startup logic
            ArchipelagoClient = new ArchipelagoClient();
            ModRoomManager = new ModRoomManager();
            ModItemManager = new ModItemManager();
            _instance = this;
            Log.LogInfo($"Plugin {PluginGUID} is loaded!");
            //Inject custom Object for Mod Handling
            ClassInjector.RegisterTypeInIl2Cpp<ModInstance>();
            ModObject = new GameObject("Archipelago");
            GameObject.DontDestroyOnLoad(ModObject);
            ModObject.hideFlags = HideFlags.HideAndDontSave; //The mod breaks if this is removed. Unsure if different flags could be used to make this more visible.
            ModObject.AddComponent<ModInstance>();
            State.Initialize();
            ArchipelagoConsole.Awake();
            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");
            CommandManager.initializeLocalCommands();
        }
    }

}
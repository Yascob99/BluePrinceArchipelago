using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.ModRooms;
using BluePrinceArchipelago.Utils;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
        public static ManualLogSource BepinLogger;
        public static ArchipelagoClient ArchipelagoClient;
        public static GameObject ModObject;
        public static ModRoomManager ModRoomManager;
        public override void Load()
        {
            // Plugin startup logic
            BepinLogger = Log;
            ArchipelagoClient = new ArchipelagoClient();
            ModRoomManager = new ModRoomManager();
            ArchipelagoConsole.Awake();
            _instance = this;
            Log.LogInfo($"Plugin {PluginGUID} is loaded!");

            //Inject custom Object for Mod Handling
            ClassInjector.RegisterTypeInIl2Cpp<ModInstance>();
            ModObject = new GameObject("Archipelago");
            GameObject.DontDestroyOnLoad(ModObject);
            ModObject.hideFlags = HideFlags.HideAndDontSave; //The mod breaks if this is removed. Unsure if different flags could be used to make this more visible.
            ModObject.AddComponent<ModInstance>();
            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");
        }
    }

}
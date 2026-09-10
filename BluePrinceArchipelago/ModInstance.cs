using BepInEx.Unity.IL2CPP.Utils.Collections;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Patches;
using BluePrinceArchipelago.Rooms;
using BluePrinceArchipelago.Triggers;
using BluePrinceArchipelago.Utils;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BluePrinceArchipelago
{
    /// <summary>
    ///     The actual instance of the mod.
    /// </summary>
    internal class ModInstance : MonoBehaviour
    {
        // A reference to the instance of this MonoBehavior.
        public static ModInstance Instance;

        // Handlers and Managers.
        public static ModEventHandler ModEventHandler = new ModEventHandler();
        public static ArchipelagoQueueManager QueueManager = new ArchipelagoQueueManager();
        public static TrunkManager TrunkManager = new();

        // Game Objects
        public static GameObject PlanPicker = new();
        public static GameObject Inventory = new();
        public static GameObject RoomsInHouse = new();
        public static GameObject StatsLogger = new();
        public static GameObject PickupSpawnPool = new();
        public static GameObject Prefabs = new();
        public static GameObject UpgradeDisksObj = new();

        // FSMs
        public static PlayMakerFSM GemManager = new();
        public static PlayMakerFSM StepManager = new();
        public static PlayMakerFSM GoldManager = new();
        public static PlayMakerFSM DiceManager = new();
        public static PlayMakerFSM KeyManager = new();
        public static PlayMakerFSM StarManager = new();
        public static PlayMakerFSM LuckManager = new();
        public static PlayMakerFSM GlobalPersistentManager = new();
        public static PlayMakerFSM GlobalManager = new();
        public static PlayMakerFSM TheGrid = new();
        public static PlayMakerFSM MasterPicker = new();
        public static PlayMakerFSM LocksmithMenu = new();
        public static PlayMakerFSM CommissaryMenu = new();
        public static PlayMakerFSM TradingPostSelection = new();
        public static PlayMakerFSM EndGameClicker = new();
        public static PlayMakerFSM RoomText = new();
        public static PlayMakerFSM APEventFSM = new();
        public static PlayMakerFSM RunningEngine = new();
        public static PlayMakerFSM DigEngine = new();
        public static PlayMakerFSM ChessKing = new();

        // Other
        public static RoomDraftHelper RDHelper = new(); 

        // Transforms
        public static Transform YouFoundText = new();

        // FSM actions.
        public static FsmStateAction DraftValidationAction = new();

        // Bools
        public static bool IsArchipelagoMode { get; private set; } = false;
        public static bool StateLoaded { get; private set; } = false;
        public static bool SceneLoaded { get; private set; } = false;
        public static bool IsInRun { get; set; } = false;
        public static bool HasInitializedRooms { get; private set; } = false;
        public static bool ArchipelagoPrefabsLoaded { get; private set; } = false;

        // Other
        public static int SaveSlot = 5; // Will be used to better confirm the loaded archipelago run.

        public static HashSet<string> SanctumsSolved = [];

        public static string PreviousSceneName { get; private set; } = "";
        public static bool AppliedHarmony { get; private set; } = false;

        public static bool FirstLoad { get; set; } = true;
        public static bool RanStartOfDay { get; set; } = false;

        public static int LoadCount = 0;

        /// <summary>
        ///     Initializing the instance.
        /// </summary>
        /// <param name="ptr">The ptr for IL2Cpp.</param>
        public ModInstance(IntPtr ptr) : base(ptr)
        {
            Instance = this; //Set the modInstance for easy access.
        }

        /// <summary>
        ///     Unity Monobehavior start.
        /// </summary>
        private void Start()
        {
            SceneManager.sceneLoaded += (Action<Scene, LoadSceneMode>)OnSceneLoaded;
            APEventFSM = Plugin.ModObject.GetComponent<PlayMakerFSM>();
            Harmony.CreateAndPatchAll(typeof(RoomPatches), "RoomPatches");
            Harmony.CreateAndPatchAll(typeof(ItemPatches), "ItemPatches");
            FSMEventHandler.RegisterEvents();
            Prefabs = GameObject.Instantiate(new GameObject("Prefabs"), Plugin.ModObject.transform);
            Prefabs.name = "prefabs";
        }
        /// <summary>
        ///     An enumerator for loading any bundled mod assets.
        /// </summary>
        /// <returns>The current asset.</returns>
        IEnumerator LoadAllAssets()
        {
            AssetBundle bundle = Plugin.AssetBundle;
            foreach (string asset in bundle.GetAllAssetNames())
            {
                // Only load the prefabs for instantiation.
                if (asset.Contains("prefab"))
                {
                    var loadAsset = bundle.LoadAssetAsync<GameObject>(asset);

                    yield return loadAsset.asset;

                    // Make the prefab a child of the modobject so it is preloaded and not deloaded on scene transitions.
                    GameObject assetGameObject = loadAsset.asset.TryCast<GameObject>();
                    GameObject obj = GameObject.Instantiate(loadAsset.asset.TryCast<GameObject>(), Prefabs.transform);
                    obj.name = assetGameObject.name;
                    obj.SetActive(false);
                }
            }
            ArchipelagoPrefabsLoaded = true;
        }
        /// <summary>
        ///     Called whenver a scene is loaded (triggered by the scene manager).
        /// </summary>
        /// <param name="scene">The current Unity Scene</param>
        /// <param name="mode">The mod the scene is loaded in (Single/Additive).</param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logging.Log($"Scene: {scene.name} loaded in {mode}");
            if (scene.name.Equals("Main Menu"))
            {
                if (!AppliedHarmony) {
                    Harmony.CreateAndPatchAll(typeof(EventPatches), "EventPatches"); //Apply event patches on the main menu to get some data that is not accessible later. 
                    Instance.StartCoroutine(Instance.LoadAllAssets().WrapToIl2Cpp());
                    AppliedHarmony = true;
                }
               
                FSMPatches.IntroSkip();
            }
            if (scene.name.Equals("Mount Holly Estate"))
            {
                SceneLoaded = true;
                RanStartOfDay = false;
                Unlocks.HasPrepatched = false;
                LoadCount++;
                if (LoadCount > 1)
                {
                    FirstLoad = false;
                }

                //Initialize all of the GameObjects
                PlanPicker = GameObject.Find("__SYSTEM/THE DRAFT/PLAN PICKER").gameObject;
                Inventory = GameObject.Find("__SYSTEM/Inventory").gameObject;
                RoomsInHouse = GameObject.Find("__SYSTEM/Room Lists/Rooms in House").gameObject;
                StatsLogger = GameObject.Find("StatsLogger").gameObject;
                GemManager = GameObject.Find("__SYSTEM/HUD/Gems")?.GetFsm("Gem Manager");
                StepManager = GameObject.Find("__SYSTEM/HUD/Steps")?.GetFsm("Steps Manager");
                GoldManager = GameObject.Find("__SYSTEM/HUD/Gold")?.GetFsm("Gold Manager");
                DiceManager = GameObject.Find("__SYSTEM/HUD/Bones")?.GetFsm("Bone Manager");
                KeyManager = GameObject.Find("__SYSTEM/HUD/Keys")?.GetFsm("Key Manager");
                StarManager = GameObject.Find("__SYSTEM/HUD/Stars")?.GetFsm("FSM");
                YouFoundText = GameObject.Find("/UI OVERLAY CAM/You Found Text").transform;
                LuckManager = GameObject.Find("__SYSTEM/Luck Calculator")?.GetFsm("Luck Calculator");
                GlobalManager = GameObject.Find("Global Manager")?.GetComponent<PlayMakerFSM>();
                GlobalPersistentManager = GameObject.Find("Global Persitent Manager")?.GetComponent<PlayMakerFSM>();
                TheGrid = GameObject.Find("__SYSTEM/THE GRID")?.GetComponent<PlayMakerFSM>();
                MasterPicker = GameObject.Find("__SYSTEM/THE DRAFT/PLAN PICKER/MASTER PICKER - OVERRIDE")?.GetComponent<PlayMakerFSM>();
                LocksmithMenu = GameObject.Find("UI OVERLAY CAM/Locksmith Menu")?.GetComponent<PlayMakerFSM>();
                CommissaryMenu = GameObject.Find("UI OVERLAY CAM/Commissary Menu")?.GetComponent<PlayMakerFSM>();
                EndGameClicker = GameObject.Find("ROOMS/Antechamber/NON STATIC/DOOR 46/grey door/End Game Clicker")?.GetComponent<PlayMakerFSM>();//TODO get the full proper path name for this GameObject.
                DraftValidationAction = MasterPicker.GetState("3").GetFirstActionOfType<CallMethod>();
                RoomText = GameObject.Find("__SYSTEM/HUD/Room Text")?.GetComponent<PlayMakerFSM>();
                PickupSpawnPool = GameObject.Find("__SYSTEM/Pickup Spawn Pools").gameObject;
                RunningEngine = GameObject.Find("__SYSTEM/RUN ENGINE/Running Engine")?.GetComponent<PlayMakerFSM>();
                DigEngine = GameObject.Find("__SYSTEM/Utility/Dig Engine")?.GetComponent<PlayMakerFSM>();
                ChessKing = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /TEXT/INVENTORY INSPECT/Inventory Descriptions/CHESS KING").GetComponent<PlayMakerFSM>();
                UpgradeDisksObj = GameObject.Find("__SYSTEM/Upgrade Disks");
                RDHelper = GameObject.Find("__SYSTEM/THE DRAFT/Draft Code").GetComponent<RoomDraftHelper>();
                FSMPatches.RoomForcer(MasterPicker); //Applies the Room Forcing patch (which also removes the forced Day 1 Draft 1 draft).
                ModRoomManager.LoadArrays();
                ModRoomManager.Reset(); // Clear stale room state from any previous scene load
                ModRoomManager.InitializeRooms();
                //ModRoomManager.SetAllVanilla();
                // If already connected to Archipelago when loading in, sync after a delay
                // to ensure the game has finished initializing all draft pools
                if (scene.name != PreviousSceneName)
                {
                    ModEventHandler.LocationFound += ArchipelagoTriggers.OnLocalLocationSent;
                    TrunkManager.Initialize();
                    if (ArchipelagoClient.Authenticated)
                    {
                        ; // Register the initial state of the items.
                        Logging.Log("Scheduling delayed sync after scene load...");
                    }
                }
                
                UpgradeDisks.InitializeUpgradeDiskNotifications();
                HasInitializedRooms = true;
            }
            else {
                // hackish, but based on my knowledge only one scene is loaded at a time.
                SceneLoaded = false;
            }

            PreviousSceneName = scene.name;
        }
        /// <summary>
        ///     Occurrs if the mod instance is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= (Action<Scene, LoadSceneMode>)OnSceneLoaded;
            Harmony.UnpatchID("ItemPatches");
            Harmony.UnpatchID("EventPatches");
            Harmony.UnpatchID("RoomPatches");
            Harmony.UnpatchID("FsmRoomPatch");
        }

        /// <summary>
        ///     Runs every Game Tick.
        /// </summary>
        private void Update() {
            if (IsInRun && ArchipelagoClient.Authenticated)
            {
                QueueManager.DequeueUsedUpgrade();
                QueueManager.DequeueItem();
                QueueManager.DequeueLocation();
            }
        }

        /// <summary>
        ///     Unity's inbuilt OnGui call. Intended for Unity GUI updates.
        /// </summary>
        private void OnGUI()
        {
            ArchipelagoConsole.OnGUI();
        }
    }
}

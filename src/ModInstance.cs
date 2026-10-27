
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Patches;
using BluePrinceArchipelago.Rooms;
using BluePrinceArchipelago.Rooms.Pickers;
using BluePrinceArchipelago.Triggers;
using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ML
using Il2Cpp;
using MelonLoader;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
#endif
using HarmonyPatch = HarmonyLib.Harmony;

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
            Plugin.Harmony.PatchAll(typeof(RoomPatches));
            Plugin.Harmony.PatchAll(typeof(ItemPatches));
            Prefabs = GameObject.Instantiate(new GameObject("Prefabs"), Plugin.ModObject.transform);
            Prefabs.name = "prefabs";
        }
        /// <summary>
        ///     An enumerator for loading any bundled mod assets.
        /// </summary>
        /// <returns>The current asset.</returns>
        System.Collections.IEnumerator LoadAllAssets()
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
            Logging.LogWarning($"Scene: {scene.name} loaded in {mode}");
            if (scene.name.Equals("Main Menu"))
            {
                if (!AppliedHarmony) {
                    Plugin.Harmony.PatchAll(typeof(EventPatches));
                    //Apply event patches on the main menu to get some data that is not accessible later. 
                    Instance.StartCoroutine(Instance.LoadAllAssets());
                    AppliedHarmony = true;
                }
                //UIManager.InitializeMainMenu();
                FSMPatches.IntroSkip();
                //FSMPatches.MainMenuHooks();
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
                PlanPicker = GameObjectExtensions.FindInHierarchy("__SYSTEM/THE DRAFT/PLAN PICKER").gameObject;
                Inventory = GameObjectExtensions.FindInHierarchy("__SYSTEM/Inventory").gameObject;
                RoomsInHouse = GameObjectExtensions.FindInHierarchy("__SYSTEM/Room Lists/Rooms in House").gameObject;
                StatsLogger = GameObjectExtensions.FindInHierarchy("StatsLogger").gameObject;
                GemManager = GameObjectExtensions.FindInHierarchy("__SYSTEM/HUD/Gems")?.GetFsm("Gem Manager");
                StepManager = GameObjectExtensions.FindInHierarchy("__SYSTEM/HUD/Steps")?.GetFsm("Steps Manager");
                GoldManager = GameObjectExtensions.FindInHierarchy("__SYSTEM/HUD/Gold")?.GetFsm("Gold Manager");
                DiceManager = GameObjectExtensions.FindInHierarchy("__SYSTEM/HUD/Bones")?.GetFsm("Bone Manager");
                KeyManager = GameObjectExtensions.FindInHierarchy("__SYSTEM/HUD/Keys")?.GetFsm("Key Manager");
                StarManager = GameObjectExtensions.FindInHierarchy("__SYSTEM/HUD/Stars")?.GetFsm("FSM");
                YouFoundText = GameObject.Find("/UI OVERLAY CAM/You Found Text").transform;
                LuckManager = GameObjectExtensions.FindInHierarchy("__SYSTEM/Luck Calculator")?.GetFsm("Luck Calculator");
                GlobalManager = GameObjectExtensions.FindInHierarchy("Global Manager")?.GetComponent<PlayMakerFSM>();
                GlobalPersistentManager = GameObjectExtensions.FindInHierarchy("Global Persitent Manager")?.GetComponent<PlayMakerFSM>();
                TheGrid = GameObjectExtensions.FindInHierarchy("__SYSTEM/THE GRID")?.GetComponent<PlayMakerFSM>();
                MasterPicker = GameObjectExtensions.FindInHierarchy("__SYSTEM/THE DRAFT/PLAN PICKER/MASTER PICKER - OVERRIDE")?.GetComponent<PlayMakerFSM>();
                LocksmithMenu = GameObject.Find("UI OVERLAY CAM/Locksmith Menu")?.GetComponent<PlayMakerFSM>();
                CommissaryMenu = GameObject.Find("UI OVERLAY CAM/Commissary Menu")?.GetComponent<PlayMakerFSM>();
                EndGameClicker = GameObjectExtensions.FindInHierarchy("ROOMS/Antechamber/NON STATIC/DOOR 46/grey door/End Game Clicker")?.GetComponent<PlayMakerFSM>();//TODO get the full proper path name for this GameObject.
                RoomText = GameObjectExtensions.FindInHierarchy("__SYSTEM/HUD/Room Text")?.GetComponent<PlayMakerFSM>();
                PickupSpawnPool = GameObjectExtensions.FindInHierarchy("__SYSTEM/Pickup Spawn Pools").gameObject;
                RunningEngine = GameObjectExtensions.FindInHierarchy("__SYSTEM/RUN ENGINE/Running Engine")?.GetComponent<PlayMakerFSM>();
                DigEngine = GameObjectExtensions.FindInHierarchy("__SYSTEM/Utility/Dig Engine")?.GetComponent<PlayMakerFSM>();
                ChessKing = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /TEXT/INVENTORY INSPECT/Inventory Descriptions/CHESS KING").GetComponent<PlayMakerFSM>(); // Needs to use GameObject.Find();
                UpgradeDisksObj = GameObjectExtensions.FindInHierarchy("__SYSTEM/Upgrade Disks");
                RDHelper = GameObjectExtensions.FindInHierarchy("__SYSTEM/THE DRAFT/Draft Code").GetComponent<RoomDraftHelper>(); //Applies the Room Forcing patch (which also removes the forced Day 1 Draft 1 draft).
                PickerManager.LoadArrays();
                ModRoomManager.Reset(); // Clear stale room state from any previous scene load
                ModRoomManager.InitializeRooms();
                //ModRoomManager.SetAllVanilla();
                // If already connected to Archipelago when loading in, sync after a delay
                // to ensure the game has finished initializing all draft pools
                if (scene.name != PreviousSceneName)
                {
                    ModEventHandler.LocationFound += ArchipelagoTriggers.OnLocalLocationSent;
                }
                UpgradeDisks.InitializeUpgradeDiskNotifications();
                FSMPatches.AllowanceEnvelopeSpawnFix();
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

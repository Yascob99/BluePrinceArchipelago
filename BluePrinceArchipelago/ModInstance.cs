using BepInEx;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Core;
using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Models;
using BluePrinceArchipelago.Utils;
using BluePrinceArchipelago.Utils.Actions;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Rewired.Integration.PlayMaker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BluePrinceArchipelago
{
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
        public static PlayMakerFSM CommissaryMenu = new();
        public static PlayMakerFSM TradingPostSelection = new();
        public static PlayMakerFSM EndGameClicker = new();

        // Transforms
        public static Transform YouFoundText = new();

        // FSM actions.
        public static FsmStateAction DraftValidationAction = new();

        // Bools
        public static bool IsArchipelagoMode { get; private set; } = false;
        public static bool StateLoaded { get; private set; } = false;
        public static bool SceneLoaded { get; private set; } = false;
        public static bool IsInRun { get; private set; } = false;
        public static bool HasInitializedRooms { get; private set; } = false;

        // Other
        public static Dictionary<string, PlayMakerArrayListProxy> PickerDict = [];
        public static int SaveSlot = 5; // Will be used to better confirm the loaded archipelago run.

        
        public ModInstance(IntPtr ptr) : base(ptr)
        {
            Instance = this; //Set the modInstance for easy access.
        }
        private void Start()
        {
            SceneManager.sceneLoaded += (Action<Scene, LoadSceneMode>)OnSceneLoaded;
        }
        // Called whenver a scene is loaded (triggered by the scene manager).
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logging.Log($"Scene: {scene.name} loaded in {mode}");
            if (scene.name.Equals("Main Menu"))
            {
                Harmony.CreateAndPatchAll(typeof(EventPatches), "EventPatches"); //Apply event patches on the main menu to get some data that is not accessible later.  
                
                // Menu Logo Skips
                var menuSystem = GameObject.Find("/Menu System");
                var fsm = menuSystem.GetComponent<PlayMakerFSM>();

                // Replace the transition to go to State 8 rather than Logo Slates
                fsm.Fsm.GetState("EnterMainMenu").GetTransition(0).ToFsmState = fsm.Fsm.GetState("State 8");
                fsm.Fsm.GetState("EnterMainMenu").GetTransition(1).ToFsmState = fsm.Fsm.GetState("State 8");

                // Because we skip Logo Slates we have to copy the music start action here.
                // This just replaces a fade to black that would've been removed anyway
                fsm.Fsm.GetState("State 8").actions[0] = fsm.Fsm.GetState("Logo Slates").actions[1];

                // Remove the 3 second delay
                var wait = fsm.Fsm.GetState("State 8").actions[2].Cast<Wait>();
                wait.time = new FsmFloat(0f);
            }
            if (scene.name.Equals("Mount Holly Estate"))
            {
                SceneLoaded = true;
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
                StarManager = GameObject.Find("__SYSTEM/HUD/Stars")?.GetFsm("Stars");
                YouFoundText = GameObject.Find("/UI OVERLAY CAM/You Found Text").transform;
                LuckManager = GameObject.Find("__SYSTEM/Luck Calculator")?.GetFsm("Luck Calculator");
                GlobalPersistentManager = GameObject.Find("Global Persistent Manager")?.GetComponent<PlayMakerFSM>();
                GlobalManager = GameObject.Find("Global Manager")?.GetComponent<PlayMakerFSM>();
                TheGrid = GameObject.Find("__SYSTEM/THE GRID")?.GetComponent<PlayMakerFSM>();
                MasterPicker = GameObject.Find("__SYSTEM/THE DRAFT/PLAN PICKER/MASTER PICKER - OVERRIDE")?.GetComponent<PlayMakerFSM>();
                CommissaryMenu = GameObject.Find("UI OVERLAY CAM/Commissary Menu/")?.GetComponent<PlayMakerFSM>();
                EndGameClicker = GameObject.Find("ROOMS/Antechamber/NON STATIC/DOOR 46/grey door/End Game Clicker")?.GetComponent<PlayMakerFSM>();//TODO get the full proper path name for this GameObject.
                DraftValidationAction = MasterPicker.GetState("3").GetFirstActionOfType<CallMethod>();
                AddRoomForcer(MasterPicker);
                LoadArrays();
                Plugin.ModRoomManager.Reset(); // Clear stale room state from any previous scene load
                InitializeRooms();
                Plugin.ModRoomManager.SetAllVanilla();
                TrunkManager.Initialize();
                RegisterItems.Register(); // Register the initial state of the items.
                HasInitializedRooms = true;
                ModEventHandler.LocationFound += OnLocalLocationSent;
                Harmony.CreateAndPatchAll(typeof(RoomPatches), "RoomPatches");
                Harmony.CreateAndPatchAll(typeof(ItemPatches), "ItemPatches");
                // If already connected to Archipelago when loading in, sync after a delay
                // to ensure the game has finished initializing all draft pools
                if (ArchipelagoClient.Authenticated)
                {
                    Logging.Log("Scheduling delayed sync after scene load...");
                    // Use Invoke to delay the sync - increased to 1 second for safety
                    Instance.Invoke(nameof(PerformDelayedSync), 1.0f);
                }
                else
                {
                    Logging.Log("Not authenticated yet - delayed sync will happen when day starts or when connecting");
                }
            }
            else {
                // hackish, but based on my knowledge only one scene is loaded at a time.
                SceneLoaded = false;
            }
        }
        // Handles the mod object being destroyed somehow.
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= (Action<Scene, LoadSceneMode>)OnSceneLoaded;
            Harmony.UnpatchID("ItemPatches");
            Harmony.UnpatchID("EventPatches");
            Harmony.UnpatchID("RoomPatches");
        }
        // Fires off when an event is sent from an FSM to an FSM or GameObject. Sometime fails,
        public static void OnEventSend(FsmEventTarget target, FsmEvent sendEvent, FsmFloat delay, DelayedEvent delayedEvent, GameObject owner, bool isDelayed) {
            string eventName = sendEvent.name;
            string targetType = target?.target.ToString() ?? "";
            string targetName = target?.gameObject?.gameObject?.name ?? "";
            // Attempt to find the name of the GameObject being targetted.
            if (targetName.Trim() == "")
            {
                GameObject targetObj = target?.gameObject?.gameObject?.value;
                if (targetObj != null && ! isDelayed)
                {
                    targetName = targetObj.name;
                }
                else if (isDelayed) {
                    targetName = delayedEvent?.eventTarget?.gameObject?.gameObject?.name ?? "";
                    if (targetName.Trim() == "") {
                        targetName = delayedEvent?.eventTarget?.gameObject?.gameObject?.value?.name ?? "";
                    }
                }
            }
            if (targetName == "Trunk Counter" && eventName == "Update Subtract") {
                TrunkManager.OnTrunkOpen();
            }
            if (targetName == "Global Manager" && eventName.Contains("Pickup")) {
                UniqueItem item = Plugin.UniqueItemManager.GetIfSpawned(eventName);
                if (item != null)
                {
                    // Handle the rare case of the item being spawned and the unlock for that item arriving before it has been picked up.
                    if (item.IsUnlocked) {
                        // Re-enable the logic that adds the item to inventory. (Will not cause issues if already enabled).
                        FsmState state = Plugin.UniqueItemManager.GetPickupState(item.Name);
                        if (state != null)
                        {
                            state.EnableActionsOfType<ArrayListAdd>();
                        }
                    }
                    item.HasBeenFound = true;
                }
            }

            string SenderName = owner != null ? owner.name ?? owner.gameObject.name : "Unknown";
            //Logging.Log($"{SenderName} Sending {eventName} to {targetType}: {targetName}");
        }
        public static void OnRoomSpawned(GameObject obj, GameObject transformObj) {

            if (obj != null)
            {
                Logging.Log($"Room: {obj.name}");
                if (Plugin.ModRoomManager.ForcedRoom != null)
                {
                    if (obj.name.ToUpper() == Plugin.ModRoomManager.ForcedRoom.Name.ToUpper())
                    {
                        MasterPicker.GetBoolVariable("ForceDraft").Value = false;
                        ModRoomManager.ForceRoomQueue.Remove(Plugin.ModRoomManager.ForcedRoom);
                        Plugin.ModRoomManager.ForcedRoom = null;
                    }
                }
            }
            if (transformObj != null)
            {
                Logging.Log($"Transform: {transformObj.name} - {transformObj.transform.position.ToString()}");
            }
            ModRoom room = Plugin.ModRoomManager.GetRoomByName(obj.name.ToUpper().Trim());
            if (room != null) {
                room.Handler?.OnRoomDrafted(obj);
                if (!room.HasBeenDrafted)
                {
                    room.HasBeenDrafted = true; //This triggers the Location found Event.
                }
            }
        }
        public static void OnOtherSpawn(GameObject obj, string poolName, GameObject transformObj) {
            Logging.Log($"Pool Name: {poolName}");
            if (obj != null)
            {
                Logging.Log($"Item: {obj.name}");
            }
            if (transformObj != null) {
                Logging.Log($"Transform: {transformObj.name} - {transformObj.transform.position.ToString()}");
            }
        }
        // Handles Day start events.
        public static void OnDayStart(int dayNum) {
            IsInRun = true;
            // Reload the inventories on day start (in case a scene transition happened).
            ModItemManager.LoadInventories();

            // Reset room in-house counts and reload arrays — game resets pools at the start of each day
            Plugin.ModRoomManager.ResetRoomInHouseCounts();
            ReloadArrays();

            // Sync room pools with Archipelago at the start of each day, regardless of when auth happened
            SyncRoomPoolsWithArchipelago();

            if (ArchipelagoClient.Authenticated)
            {
                // Release items that were queued while offline/before the run started
                QueueManager.ReleaseAllQueuedItems();
                QueueManager.ReleaseAllQueuedLocations();

                // Handle Start of day code for Permanent items (and maybe curses later).
                Plugin.ModItemManager.StartOfDay(dayNum);
                Plugin.UniqueItemManager.StartOfDay();
            }
        }

        /// <summary>
        /// Called after a delay when the scene loads to sync room pools.
        /// This ensures the game has finished initializing all draft pools before we modify them.
        /// </summary>
        private void PerformDelayedSync()
        {
            Logging.Log($"PerformDelayedSync called - Authenticated: {ArchipelagoClient.Authenticated}, HasInitializedRooms: {HasInitializedRooms}, OptionsLoaded: {ArchipelagoOptions.IsLoaded}");

            if (!ArchipelagoClient.Authenticated)
            {
                Logging.Log("PerformDelayedSync skipped - not authenticated");
                return;
            }

            if (!HasInitializedRooms)
            {
                Logging.Log("PerformDelayedSync skipped - rooms not initialized");
                return;
            }

            // If options aren't loaded yet, default to syncing (RoomDraftSanity defaults to true)
            // This handles the case where options load is delayed
            if (ArchipelagoOptions.IsLoaded && !ArchipelagoOptions.RoomDraftSanity)
            {
                Logging.Log("RoomDraftSanity is disabled - skipping room pool sync");
                return;
            }

            Logging.Log("Performing delayed sync after scene load...");
            ReloadArrays();
            SyncRoomPoolsWithArchipelago();
            Logging.Log("Delayed sync complete.");
        }

        /// <summary>
        /// Re-loads the picker arrays. Call this when arrays may have been reset by the game.
        /// </summary>
        public static void ReloadArrays()
        {
            Logging.Log("Reloading picker arrays...");
            PickerDict.Clear();
            LoadArrays();
            Logging.Log($"Reloaded {PickerDict.Count} picker arrays.");
        }

        /// <summary>
        /// Syncs room pools with Archipelago received items. 
        /// Should be called at the start of each day when connected to Archipelago.
        /// Only operates if RoomDraftSanity option is enabled.
        /// Even with no items received, this will lock all rooms for Archipelago mode.
        /// </summary>
        public static void SyncRoomPoolsWithArchipelago()
        {
            if (!ArchipelagoClient.Authenticated) return;

            // Skip room pool sync if RoomDraftSanity is disabled (and options are loaded)
            if (ArchipelagoOptions.IsLoaded && !ArchipelagoOptions.RoomDraftSanity)
            {
                Logging.Log("RoomDraftSanity is disabled - using vanilla room draft behavior");
                return;
            }

            Logging.Log("Auto-syncing room pools with Archipelago...");

            // Clear all rooms for Archipelago mode (resets counts, locks all rooms)
            Plugin.ModRoomManager.ClearAllRoomsForArchipelago();

            // Unlock rooms we've received from Archipelago (if any)
            var receivedItems = ArchipelagoClient.ServerData.ReceivedItems;
            int unlockedCount = 0;
            if (receivedItems != null && receivedItems.Count > 0)
            {
                foreach (string itemName in receivedItems)
                {
                    if (Plugin.ModRoomManager.UnlockRoomForArchipelago(itemName))
                    {
                        unlockedCount++;
                    }
                }
            }

            // Update the actual picker arrays
            Plugin.ModRoomManager.UpdateRoomPools();

            Logging.Log($"Auto-sync complete: {unlockedCount} rooms unlocked from Archipelago.");
        }

        /// <summary>
        /// Lightweight method to ensure room unlock states match Archipelago received items.
        /// Unlike full sync, this doesn't reset counts or clear rooms — just ensures unlock states are correct.
        /// Call this before UpdateRoomPools() when a draft is about to start.
        /// Only operates if RoomDraftSanity option is enabled.
        /// </summary>
        public static void EnsureRoomUnlockStates()
        {
            if (!ArchipelagoClient.Authenticated) return;

            // Skip if RoomDraftSanity is disabled
            if (!ArchipelagoOptions.RoomDraftSanity) return;

            var receivedItems = ArchipelagoClient.ServerData.ReceivedItems;
            if (receivedItems == null || receivedItems.Count == 0) return;

            // Lock all rooms that aren't using vanilla handling
            foreach (var room in Plugin.ModRoomManager.Rooms)
            {
                if (!room.UseVanilla)
                {
                    room.IsUnlocked = false;
                }
            }

            // Unlock rooms we've received from Archipelago
            foreach (string itemName in receivedItems)
            {
                Plugin.ModRoomManager.UnlockRoomForArchipelago(itemName);
            }
        }

        // Handles End of Day code, Currently unsure if this is good timing.
        public static void OnDayEnd() {
            IsInRun = false;
            Plugin.UniqueItemManager.EndOfDay();
        }
        public static void OnDraftBeforeInitialize(RoomDraftHelper instance)
        {
        }

        // Handles initializing rooms. Called when a draft is about to start (e.g., player opens a door).
        public static void OnDraftInitialize(RoomDraftHelper helper) 
        {
            if (HasInitializedRooms)
            {
                // Skip Archipelago room pool management if RoomDraftSanity is disabled
                if (!ArchipelagoOptions.RoomDraftSanity)
                {
                    // Still allow force room draft for other purposes if needed
                    Plugin.ModRoomManager.CheckForceRoomDraft();
                    return;
                }

                // Reload arrays to ensure we have fresh references (game may have reset them)
                ReloadArrays();

                // If connected to Archipelago, ensure room unlock states are correct
                if (ArchipelagoClient.Authenticated)
                {
                    // Only set unlock states, don't update pools yet (we'll do that below)
                    EnsureRoomUnlockStates();
                }

                Plugin.ModRoomManager.CheckForceRoomDraft();
                Logging.Log("Updating Rooms for draft");
                Plugin.ModRoomManager.UpdateRoomPools();
            }
            else {
                Logging.Log("Unable to update Room Pool because Rooms have not been initialized.");
            }
        }

        public static void OnOuterDraftStart(OuterDraftManager draftManager) {
            if (HasInitializedRooms) {
                // Skip Archipelago room pool management if RoomDraftSanity is disabled
                if (!ArchipelagoOptions.RoomDraftSanity)
                {
                    return;
                }

                // Reload arrays to ensure we have fresh references
                ReloadArrays();

                // If connected to Archipelago, ensure room unlock states are correct
                if (ArchipelagoClient.Authenticated)
                {
                    EnsureRoomUnlockStates();
                }

                Logging.Log("Updating Rooms for outer draft");
                Plugin.ModRoomManager.UpdateRoomPools();
            }
            else
            {
                Logging.Log("Unable to update Room Pool because Rooms have not been initialized.");
            }
        }

        private void OnGUI()
        {
            // show the mod is currently loaded in the corner
            GUI.Label(new Rect(16, 116, 300, 20), Plugin.ModDisplayInfo);
            ArchipelagoConsole.OnGUI();

            // Prevents tabbing from affecting the GUI fields (Was getting really annoying with alt-tabbing)
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t'))
            {
                Event.current.Use(); // Marks the event as used, stopping propagation
            }

            string statusMessage;
            // show the Archipelago Version and whether we're connected or not
            if (ArchipelagoClient.Authenticated)
            {
                // if your game doesn't usually show the cursor this line may be necessary
                Cursor.visible = false;

                statusMessage = " Status: Connected";
                GUI.Label(new Rect(16, 150, 300, 20), Plugin.APDisplayInfo + statusMessage);
            }
            else
            {
                // if your game doesn't usually show the cursor this line may be necessary
                Cursor.visible = true;

                statusMessage = " Status: Disconnected";
                GUI.Label(new Rect(16, 150, 300, 20), Plugin.APDisplayInfo + statusMessage);
                GUI.Label(new Rect(16, 170, 150, 20), "Host: ");
                GUI.Label(new Rect(16, 190, 150, 20), "Player Name: ");
                GUI.Label(new Rect(16, 210, 150, 20), "Password: ");

                ArchipelagoClient.ServerData.Uri = GUI.TextField(new Rect(150, 170, 150, 20),
                    ArchipelagoClient.ServerData.Uri);
                ArchipelagoClient.ServerData.SlotName = GUI.TextField(new Rect(150, 190, 150, 20),
                    ArchipelagoClient.ServerData.SlotName);
                ArchipelagoClient.ServerData.Password = GUI.TextField(new Rect(150, 210, 150, 20),
                    ArchipelagoClient.ServerData.Password);

                // requires that the player at least puts *something* in the slot name
                if (GUI.Button(new Rect(16, 230, 100, 20), "Connect") &&
                    !ArchipelagoClient.ServerData.SlotName.IsNullOrWhiteSpace())
                {
                    ConnectionData connData = new ConnectionData();
                    connData.Uri = ArchipelagoClient.ServerData.Uri;
                    connData.SlotName = ArchipelagoClient.ServerData.SlotName;
                    connData.Password = ArchipelagoClient.ServerData.Password;
                    State.UpdateServerDetails(connData);
                    Plugin.ArchipelagoClient.Connect();
                }
            }
        }
        public static void OnLocalLocationSent(System.Object sender, LocationEventArgs e)
        {
            Logging.Log($"Location sent: {e.LocationName} of the location type: {e.LocationType}");
            if (ArchipelagoClient.Authenticated)
            {
                Plugin.ArchipelagoClient.CheckLocation(e.LocationName);
            }
        }
        public static void OnDraftBeforePick(RoomDraftHelper instance)
        {
            
        }

        public static void OnRecordEvent(EventID id)
        {
            Logging.Log($"Stats being recorded for {id.ToString()}.");
            if (ArchipelagoClient.Authenticated && id == EventID.Room_46_reached)
            {
                if (ArchipelagoOptions.GoalType == GoalType.option_room46) {
                    Plugin.ArchipelagoClient.GoalCompleted();
                }
            }
        }

        //TODO update this to be less hacky.
        // loads the list of picker arrays the rooms can be added to. May rewrite to use names instead of the id of the child for better forward compatibility.
        private static void LoadArrays() {
            // Core picker arrays (indexes 2-32, 55-56, 58-61)
            List<int> coreChildIDs = [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 55, 56, 58, 59, 60, 61];
            for (int i = 0; i < coreChildIDs.Count; i++) {
                PlayMakerArrayListProxy array = PlanPicker.transform.GetChild(coreChildIDs[i]).gameObject.GetComponent<PlayMakerArrayListProxy>();
                if (array != null) {
                    PickerDict[array.name.Trim()] = array;
                }
            }

            // Additional arrays that may be needed for special drafts (like Entrance Hall, first draft, etc.)
            List<int> additionalChildIDs = [0, 33, 34, 35, 36, 37, 38, 39, 40, 44, 45, 57];
            for (int i = 0; i < additionalChildIDs.Count; i++) {
                PlayMakerArrayListProxy array = PlanPicker.transform.GetChild(additionalChildIDs[i]).gameObject?.GetComponent<PlayMakerArrayListProxy>();
                if (array != null) {
                    PickerDict[array.name.Trim()] = array;
                    Logging.Log($"Loaded additional array: {array.name} with {array.GetCount()} rooms");
                }
            }
        }

        //This additionally prevents the Day 1 Draft 1 forced draft.
        private void AddRoomForcer(PlayMakerFSM fsm)
        {
            FsmBool isDraftForced = fsm.AddFsmBool("ForceDraft", false);
            FsmState ForceDraft = fsm.AddState("Force Room Draft");
            FsmState DraftForcedCheck = fsm.AddState("Draft Forced Check");
            FsmGameObject ForcedRoom = fsm.AddFsmGameObject("ForcedRoom", null);
            DraftForcedCheck.RemoveTransitionsTo("FINISHED");
            DraftForcedCheck.AddTransition("Continue Draft", "SLOT 2");
            DraftForcedCheck.AddTransition("Force Draft", "Force Room Draft");
            DraftForcedCheck.AddLastAction(new BoolTest() { boolVariable = isDraftForced, isFalse = FsmEvent.GetFsmEvent("Continue Draft"), isTrue = FsmEvent.GetFsmEvent("Force Draft"), everyFrame = false });
            ForceDraft.AddLastAction(new SetGameObject() { everyFrame = false, gameObject = MasterPicker.GetGameObjectVariable("RoomEngine"), variable = ForcedRoom });
            SendEvent PlanSelected = fsm.GetState("Slot 1").GetFirstActionOfType<SendEvent>();
            ForceDraft.AddLastAction(PlanSelected);
            ForceDraft.RemoveTransitionsTo("FINISHED");
            FsmState DraftCodeStart = fsm.GetState("Draft Code Start");
            DraftCodeStart.ChangeTransition("FINISHED", "Draft Forced Check");
            FsmState PickAnother = fsm.GetState("Pick Another ");
            PickAnother.ChangeTransition("FINISHED", "Draft Forced Check");
        }
        public static void OnConnectToArchipelago() {
            // Only sync if rooms are already initialized (connected mid-run, not from main menu)
            if (HasInitializedRooms)
            {
                SyncRoomPoolsWithArchipelago();
            }
        }
        
        private static void InitializeRooms()
        {
            Logging.Log("Initializing Rooms");

            if (Plugin.ModRoomManager != null)
            {
                Plugin.ModRoomManager.AddRoom("AQUARIUM", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("ARCHIVES", ["CENTER - Tier 2"], true);
                Plugin.ModRoomManager.AddRoom("ATTIC", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("BALLROOM", ["FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGECREEP - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("BEDROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("BILLIARD ROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("BOILER ROOM", ["CENTER - Tier 2 G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G"], true);
                Plugin.ModRoomManager.AddRoom("BOOKSHOP", [""], true, false);
                Plugin.ModRoomManager.AddRoom("BOUDOIR", ["SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("BUNK ROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("CASINO", ["FRONTBACK G - RARE", "EDGEPIERCE G", "EDGE ADVANCE EASTWING - G", "EDGE ADVANCE WESTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "NORTH PIERCE G", "CENTER - Tier 1 G", "CORNER - Tier 1 G"], false);
                Plugin.ModRoomManager.AddRoom("CHAMBER OF MIRRORS", ["CENTER - Tier 2"], true);
                Plugin.ModRoomManager.AddRoom("CHAPEL", ["FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                // CLASSROOM is a single room that can appear as different "grades" (1-9) when drafted
                // All "Classroom X" items from Archipelago map to this single CLASSROOM entry
                Plugin.ModRoomManager.AddRoom("CLASSROOM", ["CENTER - Tier 1 G", "FRONT - Tier 1 G", "CORNER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true, false);
                Plugin.ModRoomManager.AddRoom("CLOCK TOWER", ["CENTER - Tier 2 G", "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], false);
                Plugin.ModRoomManager.AddRoom("CLOISTER", ["CENTER - Tier 2 G"], true);
                Plugin.ModRoomManager.AddRoom("CLOSED EXHIBIT", ["FRONTBACK - RARE", "NORTH PIERCE", "EDGEPIERCE - RARE", "EDGECREEP - RARE", "CENTER - Tier 2"], false);
                Plugin.ModRoomManager.AddRoom("CLOSET", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("COAT CHECK", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("COMMISSARY", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("CONFERENCE ROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
                Plugin.ModRoomManager.AddRoom("CONSERVATORY", ["CORNER - Tier 1 G"], false);
                Plugin.ModRoomManager.AddRoom("CORRIDOR", ["FRONT - Tier 1", "FRONTBACK - RARE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST"], true);
                Plugin.ModRoomManager.AddRoom("COURTYARD", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("DARKROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("DEN", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("DINING ROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1", "EDGECREEP - RARE", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("DORMITORY", ["CORNER - Tier 1", "FRONTBACK - RARE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], false);
                Plugin.ModRoomManager.AddRoom("DOVECOTE", ["EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE", "CENTER - Tier 2"], false);
                Plugin.ModRoomManager.AddRoom("DRAFTING STUDIO", ["FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGECREEP - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("DRAWING ROOM", ["FRONT - Tier 1 G", "FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("EAST WING HALL", ["EDGECREEP EAST", "EDGEPIERCE EAST"], true);
                Plugin.ModRoomManager.AddRoom("FOYER", ["FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGECREEP - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("FURNACE", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 3", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
                Plugin.ModRoomManager.AddRoom("FREEZER", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("GALLERY", ["FRONT - Tier 1", "FRONTBACK - RARE", "CENTER - Tier 3", "EDGECREEP - RARE"], false);
                Plugin.ModRoomManager.AddRoom("GARAGE", ["EDGE ADVANCE WESTWING - G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("GIFT SHOP", ["CENTER - Tier 2", "FRONT - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], false);
                Plugin.ModRoomManager.AddRoom("GREAT HALL", ["CENTER - Tier 3"], true);
                Plugin.ModRoomManager.AddRoom("GREENHOUSE", ["EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G"], true);
                Plugin.ModRoomManager.AddRoom("GUEST BEDROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("GYMNASIUM", ["FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 1", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
                Plugin.ModRoomManager.AddRoom("HALLWAY", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1"], true);
                Plugin.ModRoomManager.AddRoom("HER LADYSHIP'S CHAMBER", ["EDGE RETREAT WESTWING -  G"], true);
                Plugin.ModRoomManager.AddRoom("HOVEL", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
                Plugin.ModRoomManager.AddRoom("KITCHEN", ["FRONT - Tier 1 G", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("LABORATORY", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("LAUNDRY ROOM", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("LAVATORY", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("LIBRARY", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("LOCKER ROOM", ["FRONT - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "CENTER - Tier 2 G"], false);
                Plugin.ModRoomManager.AddRoom("LOCKSMITH", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("LOST & FOUND", ["FRONTBACK - RARE", "CORNER - Tier 1", "EDGECREEP WEST", "EDGECREEP EAST", "EDGEPIERCE WEST", "EDGEPIERCE EAST", "SOUTH PIERCE", "CENTER - Tier 2"], false);
                Plugin.ModRoomManager.AddRoom("MAID'S CHAMBER", ["FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
                Plugin.ModRoomManager.AddRoom("MAIL ROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 3", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
                Plugin.ModRoomManager.AddRoom("MASTER BEDROOM", ["EDGE ADVANCE EASTWING - G", "EDGE RETREAT EASTTWING -  G"], true);
                Plugin.ModRoomManager.AddRoom("MECHANARIUM", ["CENTER - Tier 2"], false);
                Plugin.ModRoomManager.AddRoom("MORNING ROOM", ["EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], false, false);
                Plugin.ModRoomManager.AddRoom("MUSIC ROOM", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("NOOK", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("NURSERY", ["FRONT - Tier 1 G", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("OBSERVATORY", ["FRONT - Tier 1 G", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("OFFICE", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G", "Center Rare G"], true);
                Plugin.ModRoomManager.AddRoom("PANTRY", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("PARLOR", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("PASSAGEWAY", ["CENTER - Tier 1 G"], true);
                Plugin.ModRoomManager.AddRoom("PATIO", ["EDGE ADVANCE WESTWING - G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("PLANETARIUM", ["CENTER - Tier 2", "FRONT - Tier 1", "CORNER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE"], false);
                Plugin.ModRoomManager.AddRoom("PUMP ROOM", ["FRONTBACK - RARE", "CORNER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE", "CENTER - Tier 2"], true, false);
                Plugin.ModRoomManager.AddRoom("ROOM 8", [], false, false);
                Plugin.ModRoomManager.AddRoom("ROOT CELLAR", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
                Plugin.ModRoomManager.AddRoom("ROTUNDA", ["CENTER - Tier 2 G"], true);
                Plugin.ModRoomManager.AddRoom("RUMPUS ROOM", ["FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "Center Rare G"], true);
                Plugin.ModRoomManager.AddRoom("SAUNA", ["CENTER - Tier 1", "FRONT - Tier 1", "CORNER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE"], true, false);
                Plugin.ModRoomManager.AddRoom("SCHOOLHOUSE", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
                Plugin.ModRoomManager.AddRoom("SECRET GARDEN", [""], true, false);
                Plugin.ModRoomManager.AddRoom("SECRET PASSAGE", ["CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "Center Rare G"], true);
                Plugin.ModRoomManager.AddRoom("SECURITY", ["NORTH PIERCE G", "CENTER - Tier 1 G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("SERVANT'S QUARTERS", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 2 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("BOMB SHELTER", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
                Plugin.ModRoomManager.AddRoom("SHOWROOM", ["FRONTBACK G - RARE", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "Center Rare G"], true);
                Plugin.ModRoomManager.AddRoom("SHRINE", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
                Plugin.ModRoomManager.AddRoom("SOLARIUM", ["CORNER - RARE G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G", "NORTH PIERCE G", "CENTER - Tier 2 G"], false);
                Plugin.ModRoomManager.AddRoom("SPARE ROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST"], true);
                Plugin.ModRoomManager.AddRoom("STOREROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("STUDY", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE - RARE", "Center Rare"], true);
                Plugin.ModRoomManager.AddRoom("TERRACE", ["EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("THE ARMORY", ["CENTER - Tier 1 G", "CORNER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G", "NORTH PIERCE G"], false, false);
                Plugin.ModRoomManager.AddRoom("THE FOUNDATION", ["CENTER - Tier 1", "CENTER - Tier 2", "CENTER - Tier 3"], true);
                Plugin.ModRoomManager.AddRoom("THE KENNEL", ["FRONT - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "CENTER - Tier 1"], false);
                Plugin.ModRoomManager.AddRoom("THE POOL", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G", "Center Rare G"], true);
                Plugin.ModRoomManager.AddRoom("THRONE ROOM", ["EDGEPIERCE - RARE G", "CENTER - Tier 2 G"], false);
                Plugin.ModRoomManager.AddRoom("TOMB", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
                Plugin.ModRoomManager.AddRoom("TOOLSHED", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
                Plugin.ModRoomManager.AddRoom("TRADING POST", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
                Plugin.ModRoomManager.AddRoom("TREASURE TROVE", ["FRONTBACK G - RARE","CORNER - RARE G","EDGECREEP - RARE G","EDGEPIERCE - RARE G","NORTH PIERCE G","CENTER - Tier 3 G"], false);
                Plugin.ModRoomManager.AddRoom("TROPHY ROOM", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G", "Center Rare G"], true);
                Plugin.ModRoomManager.AddRoom("TUNNEL", ["CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST"], false);
                Plugin.ModRoomManager.AddRoom("UTILITY CLOSET", ["FRONT - Tier 1", "FRONTBACK - RARE", "CORNER - Tier 1", "CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("VAULT", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 2 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G", "Center Rare G"], true);
                Plugin.ModRoomManager.AddRoom("VERANDA", ["EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G"], true);
                Plugin.ModRoomManager.AddRoom("VESTIBULE", ["CENTER - Tier 1 G"], false);
                Plugin.ModRoomManager.AddRoom("WALK-IN CLOSET", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 2 G", "EDGECREEP - RARE G", "EDGEPIERCE G", "Center Rare G"], true);
                Plugin.ModRoomManager.AddRoom("WEIGHT ROOM", ["CENTER - Tier 3", "Center Rare"], true);
                Plugin.ModRoomManager.AddRoom("WEST WING HALL", ["EDGECREEP WEST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("WINE CELLAR", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 1", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
                Plugin.ModRoomManager.AddRoom("WORKSHOP", ["FRONT - Tier 1", "FRONTBACK - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "Center Rare"], true);
                Plugin.ModRoomManager.AddRoom("ANTECHAMBER", [], true, false);
                Plugin.ModRoomManager.AddRoom("ROOM 46", [], true, false);
                Plugin.ModRoomManager.AddRoom("ENTRANCE HALL", [], true, false);
            }
        }
    }
}

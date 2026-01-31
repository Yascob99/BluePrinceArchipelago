using BepInEx;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.ModRooms;
using BluePrinceArchipelago.Utils;
using HarmonyLib;
using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BluePrinceArchipelago
{
    internal class ModInstance : MonoBehaviour
    {
        public static Dictionary<string, PlayMakerArrayListProxy> PickerDict = [];
        private static GameObject _PlanPicker = new();
        public static GameObject PlanPicker {
            get { return _PlanPicker; }
            set { _PlanPicker = value; }
        }
        private static GameObject _Inventory = new();
        public static GameObject Inventory {
            get { return _Inventory; }
            set { _Inventory = value;  }
        }

        private static bool _HasInitializedRooms = false;
        public static bool HasInitializedRooms
        {
            get { return _HasInitializedRooms; }
            set { _HasInitializedRooms = value; }
        }
        public ModInstance(IntPtr ptr) : base(ptr)
        {
        }
        private void Start()
        {
            SceneManager.sceneLoaded += (Action<Scene, LoadSceneMode>)OnSceneLoaded;
        }
        // Called whenver a scene is loaded (triggered by the scene manager).
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Plugin.BepinLogger.LogMessage($"Scene: {scene.name} loaded in {mode}");
            if (scene.name.Equals("Mount Holly Estate"))
            {
                _PlanPicker = GameObject.Find("PLAN PICKER").gameObject;
                _Inventory = GameObject.Find("Inventory").gameObject;
                LoadArrays();
                InitializeRooms();
                HasInitializedRooms = true;
                Harmony.CreateAndPatchAll(typeof(ItemPatches), "ItemPatches"); //Specify type of patches so they can be applied and removed as required.
                Harmony.CreateAndPatchAll(typeof(EventPatches), "EventPatches");
            }
        }
        // Handles the mod object being destroyed somehow.
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= (Action<Scene, LoadSceneMode>)OnSceneLoaded;
            Harmony.UnpatchID("ItemPatches");
            Harmony.UnpatchID("EventPatches");
        }
        // Fires off when an event is sent from an FSM to an FSM or GameObject. Currently just for testing. It is pretty buggy.
        public static void OnEventSend(FsmEventTarget target, FsmEvent sendEvent, FsmFloat delay, DelayedEvent delayedEvent, GameObject owner, bool isDelayed) {
            string eventName = sendEvent.name;
            string targetType = target.target.ToString();
            string targetName = target.gameObject.gameObject.name;
            if (targetName.Trim() == "")
            {
                GameObject targetObj = target.gameObject.gameObject.value;
                if (targetObj != null && ! isDelayed)
                {
                    targetName = targetObj.name;
                }
            }
            Plugin.BepinLogger.LogMessage($"Sending {sendEvent.name} to {targetType}: {targetName}");
        }
        //Called by the item patch whenever an item is spawned.
        public static void OnItemSpawn(GameObject obj, string poolName, GameObject transformObj, FsmGameObject spawnedObj) {
            if (obj != null)
            {
                Plugin.BepinLogger.LogMessage($"Item: {obj.name}");
            }
            if (transformObj != null)
            {
                Plugin.BepinLogger.LogMessage($"Transform: {transformObj.name} - {transformObj.transform.position.ToString()}");
            }
            if (spawnedObj != null) {
                if (spawnedObj.value != null) {
                    Plugin.BepinLogger.LogMessage($"SpawnedObj: {spawnedObj.value.name} - {transformObj.transform.position.ToString()}");
                }
            }
        }

        // Handles initializing rooms.
        public static void OnDraftInitialize(RoomDraftHelper helper) 
        {
            if (HasInitializedRooms)
            {

                Plugin.ModRoomManager.UpdateRoomPools();
            }
            else {
                Plugin.BepinLogger.LogMessage("Unable to update Room Pool because Rooms have not been initialized.");
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
                // Cursor.visible = false;

                statusMessage = " Status: Connected";
                GUI.Label(new Rect(16, 150, 300, 20), Plugin.APDisplayInfo + statusMessage);
            }
            else
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = true;

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
                    Plugin.ArchipelagoClient.Connect();
                }
            }
            // this is a good place to create and add a bunch of debug buttons
        }
        // loads the list of picker arrays the rooms can be added to. May rewrite to use names instead of the id of the child for better forward compatibility.
        private static void LoadArrays() {
            List<int> childIDs = [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 55, 56, 58, 59, 60, 61];

            for (int i = 0; i < childIDs.Count; i++) {
                PlayMakerArrayListProxy array = _PlanPicker.transform.GetChild(childIDs[i]).gameObject.GetComponent<PlayMakerArrayListProxy>();
                PickerDict[array.name.Trim()] = array;
            }
            
        }
        //TODO add Archipelago seed logic to this function. Also should be used to handle recconnects.
        private static void InitializeRooms()
        {
            Plugin.BepinLogger.LogMessage("Initializing Rooms");

            if (Plugin.ModRoomManager != null)
            {
                Plugin.ModRoomManager.AddRoom("AQUARIUM", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("ARCHIVES", ["CENTER - Tier 2"], true);
                Plugin.ModRoomManager.AddRoom("ATTIC", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("BALLROOM", ["FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGECREEP - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("BEDROOM", ["FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("BILLIARD ROOM", ["FRONTBACK - RARE", "NORTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("BOILER ROOM", ["CENTER - Tier 2 G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G"], true);
                Plugin.ModRoomManager.AddRoom("BOOKSHOP", [""], true, false);
                Plugin.ModRoomManager.AddRoom("BOUDOIR", ["SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("BUNK ROOM", ["FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("CASINO", ["FRONTBACK G - RARE", "EDGEPIERCE G", "EDGE ADVANCE EASTWING - G", "EDGE ADVANCE WESTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "NORTH PIERCE G", "CENTER - Tier 1 G", "CORNER - Tier 1 G"], false);
                Plugin.ModRoomManager.AddRoom("CHAMBER OF MIRRORS", ["CENTER - Tier 2"], true);
                Plugin.ModRoomManager.AddRoom("CHAPEL", ["FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("CLASSROOM", ["CENTER - Tier 1 G", "FRONT - Tier 1 G", "CORNER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true, false);
                Plugin.ModRoomManager.AddRoom("CLOCK TOWER", ["CENTER - Tier 2 G", "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], false);
                Plugin.ModRoomManager.AddRoom("CLOISTER", ["CENTER - Tier 2 G"], true);
                Plugin.ModRoomManager.AddRoom("CLOSED EXHIBIT", ["FRONTBACK - RARE", "NORTH PIERCE", "EDGEPIERCE - RARE", "EDGECREEP - RARE", "CENTER - Tier 2"], false);
                Plugin.ModRoomManager.AddRoom("CLOSET", ["FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("COAT CHECK", ["FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("COMMISSARY", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("CONFERENCE ROOM", ["FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
                Plugin.ModRoomManager.AddRoom("CONSERVATORY", ["CORNER - Tier 1 G"], false);
                Plugin.ModRoomManager.AddRoom("CORRIDOR", ["FRONTBACK - RARE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST"], true);
                Plugin.ModRoomManager.AddRoom("COURTYARD", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("DARKROOM", ["FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("DEN", ["FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("DINING ROOM", ["FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1", "EDGECREEP - RARE", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("DORMITORY", ["CORNER - Tier 1", "FRONTBACK - RARE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], false);
                Plugin.ModRoomManager.AddRoom("DOVECOTE", ["EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE", "CENTER - Tier 2"], false);
                Plugin.ModRoomManager.AddRoom("DRAFTING STUDIO", ["FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGECREEP - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("DRAWING ROOM", ["FRONT - Tier 1 G", "FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("EAST WING HALL", ["EDGECREEP EAST", "EDGEPIERCE EAST"], true);
                Plugin.ModRoomManager.AddRoom("FOYER", ["FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGECREEP - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("FURNACE", ["FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 3", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
                Plugin.ModRoomManager.AddRoom("FREEZER", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("GALLERY", ["FRONTBACK - RARE", "CENTER - Tier 3", "EDGECREEP - RARE"], false);
                Plugin.ModRoomManager.AddRoom("GARAGE", ["EDGE ADVANCE WESTWING - G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("GIFT SHOP", ["CENTER - Tier 2", "FRONT - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], false);
                Plugin.ModRoomManager.AddRoom("GREAT HALL", ["CENTER - Tier 3"], true);
                Plugin.ModRoomManager.AddRoom("GREENHOUSE", ["EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G"], true);
                Plugin.ModRoomManager.AddRoom("GUEST BEDROOM", ["FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("GYMNASIUM", ["FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 1", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
                Plugin.ModRoomManager.AddRoom("HALLWAY", ["FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1"], true);
                Plugin.ModRoomManager.AddRoom("HER LADYSHIP'S CHAMBER", ["EDGE RETREAT WESTWING -  G"], true);
                Plugin.ModRoomManager.AddRoom("HOVEL", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
                Plugin.ModRoomManager.AddRoom("KITCHEN", ["FRONT - Tier 1 G", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("LABORATORY", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("LAUNDRY ROOM", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("LAVATORY", ["FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("LIBRARY", ["FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("LOCKER ROOM", ["FRONT - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "CENTER - Tier 2 G"], false);
                Plugin.ModRoomManager.AddRoom("LOCKSMITH", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("LOST & FOUND", ["FRONTBACK - RARE", "CORNER - Tier 1", "EDGECREEP WEST", "EDGECREEP EAST", "EDGEPIERCE WEST", "EDGEPIERCE EAST", "SOUTH PIERCE", "CENTER - Tier 2"], true);
                Plugin.ModRoomManager.AddRoom("MAID'S CHAMBER", ["FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
                Plugin.ModRoomManager.AddRoom("MAIL ROOM", ["FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 3", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
                Plugin.ModRoomManager.AddRoom("MASTER BEDROOM", ["EDGE ADVANCE EASTWING - G", "EDGE RETREAT EASTTWING -  G"], true);
                Plugin.ModRoomManager.AddRoom("MECHANARIUM", ["CENTER - Tier 2"], false);
                Plugin.ModRoomManager.AddRoom("MORNING ROOM", ["EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], false, false);
                Plugin.ModRoomManager.AddRoom("MUSIC ROOM", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
                Plugin.ModRoomManager.AddRoom("NOOK", ["FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("NURSERY", ["FRONT - Tier 1 G", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("OBSERVATORY", ["FRONT - Tier 1 G", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("OFFICE", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G", "Center Rare G"], true);
                Plugin.ModRoomManager.AddRoom("PANTRY", ["FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("PARLOR", ["FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("PASSAGEWAY", ["CENTER - Tier 1 G"], true);
                Plugin.ModRoomManager.AddRoom("PATIO", ["EDGE ADVANCE WESTWING - G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
                Plugin.ModRoomManager.AddRoom("PLANETARIUM", ["CENTER - Tier 2", "FRONT - Tier 1", "CORNER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE"], false);
                Plugin.ModRoomManager.AddRoom("PUMP ROOM", ["FRONTBACK - RARE", "CORNER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE", "CENTER - Tier 2"], true, false);
                Plugin.ModRoomManager.AddRoom("ROOM 8", ["DOWSING NOMBOS"], false, false);
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
                Plugin.ModRoomManager.AddRoom("SOLARIUM", ["CORNER - RARE G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G", "NORTH PIERCE G", "CENTER - Tier 2 G"], true);
                Plugin.ModRoomManager.AddRoom("SPARE ROOM", ["FRONTBACK - RARE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST"], true);
                Plugin.ModRoomManager.AddRoom("STOREROOM", ["FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("STUDY", ["FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE - RARE", "Center Rare"], true);
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
                Plugin.ModRoomManager.AddRoom("UTILITY CLOSET", ["FRONTBACK - RARE", "CORNER - Tier 1", "CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("VAULT", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 2 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G", "Center Rare G"], true);
                Plugin.ModRoomManager.AddRoom("VERANDA", ["EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G"], true);
                Plugin.ModRoomManager.AddRoom("VESTIBULE", ["CENTER - Tier 1 G"], false);
                Plugin.ModRoomManager.AddRoom("WALK-IN CLOSET", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 2 G", "EDGECREEP - RARE G", "EDGEPIERCE G", "Center Rare G"], true);
                Plugin.ModRoomManager.AddRoom("WEIGHT ROOM", ["CENTER - Tier 3", "Center Rare"], true);
                Plugin.ModRoomManager.AddRoom("WEST WING HALL", ["EDGECREEP WEST", "EDGEPIERCE WEST"], true);
                Plugin.ModRoomManager.AddRoom("WINE CELLAR", ["FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 1", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
                Plugin.ModRoomManager.AddRoom("WORKSHOP", ["FRONTBACK - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "Center Rare"], true);
                Plugin.ModRoomManager.AddRoom("ANTECHAMPER", [], true, false);
                Plugin.ModRoomManager.AddRoom("ROOM 46", [], true, false);
                Plugin.ModRoomManager.AddRoom("ENTRANCE HALL", [], true, false);
            }
        }
    }
}

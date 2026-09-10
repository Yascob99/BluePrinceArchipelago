using BluePrinceArchipelago.Rooms.RoomHandlers;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using StableNameDotNet;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms
{
    /// <summary>
    ///     A Manager that handles operations related to rooms or drafting.
    /// </summary>
    public class ModRoomManager {
        private List<ModRoom> _Rooms = [];
        public List<ModRoom> Rooms {
            get { return _Rooms; }
            set { _Rooms = value; }
        }
        public ModRoom ForcedRoom = null;
        public bool IsForcingDraft = false;

        public static List<string> VanillaRooms = [];
        public static List<string> CantCopy = ["ANTECHAMBER", "ENTRANCE HALL", "ROOM 46", "FOUNDATION", ""];
        public static List<string> FoundFloorplans = ["PLANETARIUM", "CONSERVATORY", "TUNNEL", "THRONE ROOM", "TREASURE TROVE", "MECHANARIUM", "LOST & FOUND", "CLOSED EXHIBIT", "CLOCK TOWER", "THE KENNEL", "VESTIBULE", "DOVECOTE", "SOLARIUM", "DORMITORY", "CASINO", "SAUNA", "LOCKER ROOM", "MORNING ROOM", "CLASSROOM"];
        public static List<ModRoom> OuterDraftRooms = new();

        public static Dictionary<string, string> UpgradeIDs = new Dictionary<string, string>()
        {
            {"BOUDOIR", "Upgrade Boudoir"},
            {"CLOSET", "Upgrade Closet"},
            {"COURTYARD", "Upgrade Courtyard"},
            {"HALLWAY", "Upgrade Hallway"},
            {"SPARE ROOM", "Upgrade Spare Room"},
            {"STOREROOM", "Upgrade Storeroom"},
            {"BILLIARD ROOM", "Upgrade Billiard"},
            {"MAIL ROOM", "Upgrade Mail Room"},
            {"BUNK ROOM", "Upgrade Bunk Room"},
            {"AQUARIUM", "Upgrade Aquarium"},
            {"GUEST BEDROOM", "Upgrade Guest Bedroom"},
            {"CLOISTER", "Upgrade Cloister"},
            {"NURSERY", "Upgrade Nursery"},
            {"PARLOR", "Upgrade Parlor"},
            {"NOOK", "Upgrade Nool"}
        };

        public static List<string> CurrentPickerArrays = [];
        public static List<string> CurrentPickerLists = [];

        public static List<ModRoom> ForceRoomQueue = new(); // Not actually a queue, but is handled like that by the functions that interact with it.

        public ModRoomManager() {
        }

        /// <summary>
        /// Clears all room state so InitializeRooms can be safely called again (e.g. on scene reload).
        /// </summary>
        public void Reset()
        {
            _Rooms.Clear();
            VanillaRooms.Clear();
            ForceRoomQueue.Clear();
            ForcedRoom = null;
            IsForcingDraft = false;
            Logging.Log("ModRoomManager reset.");
        }

        /// <summary>
        ///     Handles any room settings that need to be set on Day start
        /// </summary>
        public void StartOfDay() {
            Transform RoomSpawnPools = GameObject.Find("__SYSTEM/Room Spawn Pools/").transform;
            for (int i = 0; i < RoomSpawnPools.childCount; i++) {
                Transform child = RoomSpawnPools.GetChild(i);
                if (child != null)
                {
                    if (child.name.Contains("Foundation")) {
                        // Remove a copy of the Foundation to prevent extra foundations from being in the pool.
                        GetRoomByName("The Foundation").RoomPoolAdjustment = -1;
                    }
                }
            }
        }

        /// <summary>
        ///     Adds a room to the tracked mod pool.
        /// </summary>
        /// <param name="room">The ModRoom of the room to add.</param>
        /// <returns>The Modroom provided.</returns>
        public ModRoom AddRoom(ModRoom room) {
            bool found = false;
            int counter = -1;
            // check if room already exists in the room pool
            while (!found && counter < _Rooms.Count - 1) {
                counter++;
                if (_Rooms[counter].Name == room.Name) {
                    found = true;
                }
            }
            if (found) {
                Logging.LogWarning($"{_Rooms[counter].Name} already in Pool, adding more to the pool");
                _Rooms[counter].RoomPoolCount++;
                room.Initialize();
            }
            else
            {
                _Rooms.Add(room);
                room.Initialize();
                if (room.UseVanilla) {
                    VanillaRooms.Add(room.Name);
                }
            }
            return room;
        }

        /// <summary>
        ///     Forces a Draft for a given room
        /// </summary>
        /// <param name="roomname">The name of the room to force.</param>
        public void ForceDraft(string roomname) {
            ModRoom room = GetRoomByName(roomname);
            if (room != null)
            {
                ForceDraft(room);
                return;
            }
            Logging.LogWarning($"Error forcing room unable to find the room: {roomname}");
        }

        /// <summary>
        ///     Adds a room to the Queue or rooms to be forced.
        /// </summary>
        /// <param name="room"></param>
        public void ForceDraft(ModRoom room) {
            if (room != null) { 
                ForceRoomQueue.Add(room);
                return;
            }
            Logging.LogWarning("Error forcing room, room can't be null");
        }

        /// <summary>
        ///     Checks if there is a valid room to force. If it can be forced force it and return true.
        /// </summary>
        /// <returns>True if a room was forced, False if not</returns>
        public bool CheckForceRoomDraft() {
            // Check if any rooms have been queued for forcing.
            //Pre-reset this.
            if (ForceRoomQueue.Count > 0)
            {
                Logging.Log("Checking if rooms can be forced.");
                // Check if those rooms can be drafted at that location (ignoring usual rules).
                bool draftable = false;
                int i = -1;
                int j = 0;
                ModRoom room = null;
                if (ForcedRoom == null)
                {
                    UpdateCurrentPickerArrays();
                    Logging.LogWarning($"[{CurrentPickerArrays.Join(", ")}]");
                    while (!draftable && i < ForceRoomQueue.Count - 1)
                    {
                        i++;
                        room = ForceRoomQueue[i];
                        if (room != null)
                        {
                            while (!draftable && j < room.PickerArrays.Count)
                            {
                                if (CurrentPickerArrays.Contains(room.PickerArrays[j]))
                                {
                                    draftable = true;

                                }
                                j++;
                            }
                        }
                        j = 0;
                    }
                    // If one of the forced rooms is draftable, force it. If not return false and continue as normal.
                    if (draftable)
                    {
                        Logging.Log($"Forcing Room Draft for: {room.Name}");
                        ModInstance.MasterPicker.GetBoolVariable("ForceDraft").Value = true;
                        ModInstance.MasterPicker.GetGameObjectVariable("ForcedRoom").Value = room.GameObj;
                        ModInstance.MasterPicker.GetGameObjectVariable("RoomEngine").Value = room.GameObj;
                        ForcedRoom = room;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///     Sets all rooms to the correct unlock state. (for in case the game changed it or for if the state changed since it's last check).
        /// </summary>
        public void RecheckRoomUnlockStatus() {
            // Forcibly set certain rooms as removed from the pool so they are not actually draftable.
            foreach (ModRoom room in Rooms)
            {
                // If the room is unlocked.
                if (room.IsUnlocked)
                {
                    // If there are still copies in today's pool (or a safety for if extra copies are added without the mod tracking it.)
                    if (room.IsUnlocked && (room.RoomInHouseCount > 0 || room.RoomsLeftInPool > 0))
                    {
                        // Confirm all dependencies of the room have been met. If one is not met, turn the room off until the next draft. (very important for Foundation)
                        foreach (Func<ModRoom, bool> dependency in room.Dependencies)
                        {
                            if (!dependency.Invoke(room))
                            {
                                SetPoolRemovalVar(room.GameObjectName, true);
                                return;
                            }
                        }
                        SetPoolRemovalVar(room.GameObjectName);
                        return;
                    }
                }
                SetPoolRemovalVar(room.GameObjectName, true);
                return;
            }
        }

        /// <summary>
        ///     Removes a copy of a room from the mod's counted pool
        /// </summary>
        /// <param name="room">The room to remove</param>
        public void RemoveRoom(ModRoom room)
        {
            if (room.RoomPoolCount > 0) {
                room.RoomPoolCount -= 1;
            }
            room.IsUnlocked = false;
        }
        /// <summary>
        ///     A fix for the HLC being deactivated for days 8+ on veteran mode.
        /// </summary>
        public void HLCFix() {
            if (ModInstance.GlobalPersistentManager.GetBoolVariable("_Veteran Player").Value) {
                ModRoom HLC = GetRoomByName("HER LADYSHIP\'S CHAMBER");
                if (HLC.IsUnlocked) {
                    SetPoolRemovalVar("HER LADYSHIP\'S CHAMBER");
                }
            }
        }

        /// <summary>
        /// Resets the room in house count for all rooms. Call this at the start of a new day.
        /// </summary>
        public void ResetRoomInHouseCounts()
        {
            foreach (ModRoom room in _Rooms)
            {
                room.RoomInHouseCount = 0;
            }
            Logging.Log("Reset all room in-house counts for new day.");
        }

        /// <summary>
        ///     Gets a ModRoom by its name.
        /// </summary>
        /// <param name="name">The Name of the room</param>
        /// <returns>The ModRoom matching the room.</returns>
        public ModRoom GetRoomByName(string name)
        {
            foreach (ModRoom room in _Rooms) {
                if (room.Name.ToUpper().Trim() == name.ToUpper().Trim() || room.GameObjectName.ToUpper().Trim() == name.ToUpper().Trim()) {
                    return room; 
                }
                foreach (GameObject gameObject in room.UpgradeObjects) {
                    if (gameObject.name.ToUpper().Trim() == name.ToUpper().Trim())
                    {
                        return room;
                    }
                }
            }
                return null;
        }

        /// <summary>
        ///     Adds a room with the same name for both the room and its game object path.
        /// </summary>
        public ModRoom AddRoom(string name, List<string> pickerArrays, bool isUnlocked, bool useVanilla = false, bool hasBeenDrafted = false) {
            return AddRoom(name, name, pickerArrays, isUnlocked, useVanilla, hasBeenDrafted);
        }

        /// <summary>
        ///     Adds a room with a separate game object name (for special cases like classroom variants).
        /// </summary>
        /// <param name="name">The name used internally by the mod (e.g., "CLASSROOM (1)")</param>
        /// <param name="gameObjectName">The actual name of the game object in Room Engines (e.g., "CLASSROOM")</param>
        /// <param name="pickerArrays">The Picker Arrays the room uses.</param>
        /// <param name="isUnlocked">If the room is unlocked.</param>
        /// <param name="useVanilla">Whether to Use Vanilla handling</param>
        /// <param name="hasBeenDrafted">If the room has been drafted at least once.</param>
        public ModRoom AddRoom(string name, string gameObjectName, List<string> pickerArrays, bool isUnlocked, bool useVanilla = false, bool hasBeenDrafted = false) {
            string roomPath = "__SYSTEM/The Room Engines/" + gameObjectName;
            GameObject roomObj = GameObject.Find(roomPath);
            if (roomObj == null)
            {
                Logging.LogWarning($"Could not find room GameObject at '{roomPath}' for room '{name}'", "ModRoomManager");
            }

            if (name == "CLASSROOM")
            {
                return AddRoom(new ClassRoom(name, gameObjectName, roomObj, pickerArrays, isUnlocked, useVanilla, hasBeenDrafted));
            }
            // rooms only have children if they have upgrades.
            if (roomObj.transform.childCount > 0)
            {
                List<GameObject> UpgradeObjs = new List<GameObject>();
                for (int i = 0; i < roomObj.transform.childCount; i++) {
                    GameObject child = roomObj?.transform?.GetChild(i)?.gameObject;
                    if (child != null) { 
                        UpgradeObjs.Add(child);
                    }
                }
                // Get the current UpgradeID of the room
                int UpgradeID = 0;
                if (UpgradeIDs.ContainsKey(name))
                {
                    UpgradeID = ModInstance.GlobalPersistentManager.GetIntVariable(UpgradeIDs[name]).Value;
                }
                else {
                    Logging.LogWarning($"UpgradeID variable could not be found for {name}.");
                }
                return AddRoom(new ModRoom(name, gameObjectName, roomObj, pickerArrays, isUnlocked, useVanilla, hasBeenDrafted, UpgradeObjs, UpgradeID));
            }
            return AddRoom(new ModRoom(name, gameObjectName, roomObj, pickerArrays, isUnlocked, useVanilla, hasBeenDrafted));
        }

        /// <summary>
        ///     Attempts to update all the picker arrays to match what the mod expects them to be.
        /// </summary>
        public void UpdateRoomPools()
        {
            Logging.Log("Updating Room Pools");
            foreach (string key in ModInstance.PickerDict.Keys)
            {
                PlayMakerArrayListProxy untouchedArray = ModInstance.UntouchedPickers[key];
                PlayMakerArrayListProxy array = ModInstance.PickerDict[key];
                int length = array.arrayList.Count;
                GameObject room = null;
                ModRoom modRoom = null;
                Dictionary<string, int> RoomCounts = new Dictionary<string, int>();
                for (int i = 0; i < length; i++) {
                    room = array.arrayList[i].TryCast<GameObject>();
                    if (room != null)
                    {
                        modRoom = GetRoomByName(room.name);
                        if (modRoom != null)
                        {
                            if (RoomCounts.ContainsKey(room.name))
                            {
                                RoomCounts[room.name]++;
                            }
                            else
                            {
                                RoomCounts[room.name] = 1;
                            }
                        }
                        else {
                            Logging.Log($"Unable to find room: {room.name}");
                        }
                    }
                }
                int untouchedLength = untouchedArray.arrayList.Count;
                List<string> updated = [];
                for (int j = 0; j < untouchedLength; j++)
                {
                    if (untouchedArray.arrayList[j] != null)
                    {
                        room = untouchedArray.arrayList[j].TryCast<GameObject>();
                        if (room != null)
                        {
                            modRoom = GetRoomByName(room.name);
                            if (modRoom != null)
                            {
                                if (RoomCounts.ContainsKey(room.name))
                                {
                                    modRoom.UpdateArray(array, RoomCounts[room.name]);
                                    updated.Add(room.name);
                                }
                                else
                                {
                                    modRoom.UpdateArray(array, 0);
                                    updated.Add(room.name);
                                }
                            }
                            else
                            {
                                Logging.Log($"Unable to find room: {room.name}");
                            }
                        }
                    }
                }
                foreach (string roomName in FoundFloorplans) {
                    if (!RoomCounts.ContainsKey(roomName) && !updated.Contains(roomName)) {
                        modRoom = GetRoomByName(roomName);
                        if (modRoom != null)
                        {
                            if (modRoom.PickerArrays.Contains(key))
                            {
                                modRoom.UpdateArray(array, 0);
                            }
                        }
                        else
                        {
                            Logging.Log($"Unable to find room: {room.name}");
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Empties the draft pool.
        /// </summary>
        public void EmptyDraftPool()
        {
            foreach (ModRoom room in _Rooms) {
                room.IsUnlocked = false || room.UseVanilla; //Set the room to not unlocked, unless the room is set to use Vanilla Handling.
            }
        }

        /// <summary>
        ///     Clears the entire draft pool for Archipelago mode - locks ALL rooms regardless of vanilla status.
        ///     Use this when syncing with Archipelago to start fresh.
        /// </summary>
        public void ClearAllRoomsForArchipelago()
        {
            Logging.Log("Clearing all rooms for Archipelago sync...");
            ResetRoomInHouseCounts(); // Reset counts so pools calculate correctly
            foreach (ModRoom room in _Rooms) {
                room.IsUnlocked = false; // Lock ALL rooms, including vanilla ones
                room.UseVanilla = false; // Disable vanilla handling for Archipelago mode
            }
        }

        /// <summary>
        ///     Checks if an item name corresponds to a room, including special mappings.
        ///     Returns true if the item is a room, false otherwise.
        /// </summary>
        /// <param name="itemName">The AP item name to check</param>
        /// <returns>If the AP item is a room</returns>
        public bool IsRoomItem(string itemName)
        {
            // Try exact match first
            if (GetRoomByName(itemName) != null)
            {
                return true;
            }

            // Try mapped name (for classrooms and other special cases)
            string mappedName = MapArchipelagoRoomName(itemName);
            if (mappedName != null && GetRoomByName(mappedName) != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Unlocks a specific room by name for Archipelago mode.
        ///     Handles special cases like classroom variants (e.g., "Classroom 2" → "CLASSROOM (2)").
        /// </summary>
        /// <param name="roomName">The Name of the room</param>
        /// <returns>True if successfully unlocked, false if it failed.</returns>
        public bool UnlockRoomForArchipelago(string roomName)
        {
            // Try to find room with exact name first
            ModRoom room = GetRoomByName(roomName);

            // If not found, try special mappings for classroom variants
            if (room == null)
            {
                string mappedName = MapArchipelagoRoomName(roomName);
                if (mappedName != null)
                {
                    room = GetRoomByName(mappedName);
                }
            }

            if (room != null)
            {
                room.IsUnlocked = true;

                // Update the RoomRecords to simulate the room as having been drafted once. This makes it so the directory properly displays the unlocked room pool.
                if (!room.AddedToDirectory)
                {
                    Il2CppSystem.Collections.Hashtable RoomRecords = GameObject.Find("Global Persitent Manager").GetHashTableProxy("RoomRecords").hashTable;
                    if (RoomRecords.ContainsKey(room.Name))
                    {
                        int value = RoomRecords[room.Name].Unbox<int>();
                        if (value == 0)
                        {
                            RoomRecords[room.Name] = 1;
                        }
                        room.AddedToDirectory = true;
                    }
                }
                Logging.Log($"Archipelago: Unlocked room '{room.Name}'");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the mapped room name for an Archipelago item name.
        /// Returns the mapped name if a mapping exists, null otherwise.
        /// Public method for use by other classes that need the mapping.
        /// </summary>
        public string GetMappedRoomName(string apRoomName)
        {
            return MapArchipelagoRoomName(apRoomName);
        }

        /// <summary>
        /// Maps Archipelago room names to actual game room names.
        /// Handles special cases for rooms with non-standard naming.
        /// </summary>
        private string MapArchipelagoRoomName(string apRoomName)
        {
            // Add other special mappings here as needed in the future

            return apRoomName switch
            {
                "Progressive Classroom" => "CLASSROOM", // Map all classroom variants to the base classroom name
                _ => null
            };
        }

        /// <summary>
        /// Gets the base room name for upgraded rooms
        /// </summary>
        /// <param name="roomName">The name of the upgraded room to check</param>
        /// <returns>The base room name if the input is an upgraded room, otherwise returns the original name</returns>
        public static string GetBaseRoomName(string roomName)
        {
            roomName = roomName.ToUpper().Trim();
            return roomName switch
            {
                _ when roomName.Contains("SPARE") => "SPARE ROOM", // Handle all spare room variants
                _ when roomName.StartsWith("CLOISTER") => "CLOISTER", // Handle all cloister variants
                "FUNERAL PARLOR" => "PARLOR",
                "POOL HALL" => "BILLIARD ROOM",
                "BREAK ROOM" => "BILLIARD ROOM",
                "SPEAKEASY" => "BILLIARD ROOM",
                "EMPTY CLOSET" => "CLOSET",
                "BEDROOM CLOSET" => "CLOSET",
                "HALLWAY CLOSET" => "CLOSET",
                "READING NOOK" => "NOOK",
                "BREAKFAST NOOK" => "NOOK",
                "ELECTRIC EEL AQUARIUM" => "AQUARIUM",
                "STARFISH AQUARIUM" => "AQUARIUM",
                "GOLDFISH AQUARIUM" => "AQUARIUM",
                "GUESS BEDROOM" => "GUEST BEDROOM",
                "QUEST BEDROOM" => "GUEST BEDROOM",
                "GEIST BEDROOM" => "GUEST BEDROOM",
                "NURSE'S STATION" => "NURSERY",
                "INDOOR NURSERY" => "NURSERY",
                "CORRIYARD" => "COURTYARD",
                _ => roomName  
            };
        }

        /// <summary>
        ///     Sets the variable the game uses to remove rooms from the pool with repellant. Useful for our purposes.
        /// </summary>
        /// <param name="name">The name of the room</param>
        /// <param name="value">The value to set it to.</param>
        public void SetPoolRemovalVar(string name, bool value = false) {
            string roomPath = "__SYSTEM/The Room Engines/" + name;
            GameObject roomEngine = GameObject.Find(roomPath);
            if (roomEngine != null)
            {
                PlayMakerFSM fsm = roomEngine.GetComponent<PlayMakerFSM>();
                if (fsm != null)
                {
                    FsmBool poolRemovalVar = fsm.GetBoolVariable("POOL REMOVAL");
                    if (poolRemovalVar != null)
                    {
                        // POOL REMOVAL = true means room is NOT available (removed from pool)
                        // POOL REMOVAL = false means room IS available (in pool)
                        poolRemovalVar.Value = value;
                        Logging.LogDebug($"Room '{name.ToTitleCase()}' (GO: {name}) POOL REMOVAL set to {!value} (IsUnlocked={value})");
                    }
                    else
                    {
                        Logging.LogWarning($"Room '{name.ToTitleCase()}' (GO: {name}): Could not find 'POOL REMOVAL' variable in FSM");
                    }
                }
                else
                {
                    Logging.LogWarning($"Room 'Room '{name.ToTitleCase()}' (GO: {name}): Could not find FSM named '{name}'");
                }
            }
        }

        /// <summary>
        ///     A substitute preshuffle of the Outer Rooms that works on smaller pool sizes.
        /// </summary>
        /// <returns>The shuffled outer room list 3-8 long.</returns>
        public List<ModRoom> OuterDraftPrePickShuffling() {
            List<string> OuterRooms = ["TOOLSHED", "BOMB SHELTER", "SCHOOLHOUSE", "SHRINE", "ROOT CELLAR", "HOVEL", "TRADING POST", "TOMB"];
            List<string> NewList = new List<string>();
            List<ModRoom> Output = new List<ModRoom>();

            // Keep on the rooms that are unlocked.
            foreach (string room in OuterRooms) {
                ModRoom modRoom = GetRoomByName(room);
                if (modRoom.IsUnlocked) {
                    NewList.Add(room);
                }
            }

            // Replace with Closets if none.
            if (NewList.Count == 0) {
                ModRoom Closet = GetRoomByName("CLOSET");
                return [Closet, Closet, Closet];
            }

            // Shuffle All the Rooms;
            System.Random rng = new System.Random();
            NewList.Shuffle(rng);

            // Handles all the cases for less than 3 rooms in pool.
            if (NewList.Count < 3)
            {
                foreach (string roomname in NewList)
                {
                    ModRoom modRoom = GetRoomByName(roomname);
                    Output.Add(modRoom);
                }
                if (NewList.Count == 1) {
                    Output.Add(Output[0]);
                    Output.Add(Output[0]);
                }
                else if (NewList.Count == 2) {
                    Output.Add(Output[0]);
                }
                return Output;
            }
            // Get all needed values.
            int TombRNG = rng.Next(100) + 1;
            int SchoolHouseRNG = rng.Next(100) + 1;
            int ShrineRNG = rng.Next(100) + 1;
            bool Reached46 = ModInstance.GlobalPersistentManager.GetBoolVariable("Room 46 Reached").Value;
            bool VetMode = ModInstance.GlobalPersistentManager.GetBoolVariable("_Veteran Player").Value;
            bool FoundationLowered = ModInstance.GlobalPersistentManager.GetBoolVariable("Foundation Elevator Down").Value;
            int OuterDrafts = ModInstance.GlobalPersistentManager.GetIntVariable("Outer Drafts").Value;
            int Day = ModInstance.GlobalPersistentManager.GetIntVariable("DAY").Value;
            bool smallChance = Reached46 || (VetMode && Day == 1);
            int StandaloneDraftedID = ModInstance.GlobalPersistentManager.GetIntVariable("TheStandaloneRoomDraftedToday").Value;
            bool mediumChance = Day > 7 || FoundationLowered || OuterDrafts > 2;
            List<bool> colorBools = ConfirmColors();

            // Tomb RNG
            if (!(TombRNG > 10 && smallChance))
            {
                if (!(TombRNG > 45 && mediumChance))    
                {
                    if (TombRNG != 100)
                    {
                        NewList.FindAndInsert("TOMB", NewList.Count - 1);
                    }
                }
            }

            // Schoolhouse RNG
            if (!(SchoolHouseRNG > 10 && smallChance))
            {
                if (!(SchoolHouseRNG > 45 && mediumChance))
                {
                    if (SchoolHouseRNG < 95)
                    {
                        NewList.FindAndInsert("SCHOOLHOUSE", NewList.Count - 2);
                    }
                }
            }

            // Shrine RNG
            if (!(ShrineRNG > 10 && smallChance))
            {
                if (!(ShrineRNG > 45 && mediumChance))
                {
                    if (ShrineRNG < 60)
                    {
                        NewList.FindAndInsert("SCHOOLHOUSE", NewList.Count - 2);
                    }
                }
            }

            // First Outer Draft
            if (OuterDrafts == 0 && !(VetMode && Day == 1))
            {
                NewList.FindAndInsert("ROOT CELLAR");
                NewList.FindAndInsert("TOOLSHED", 1);
                NewList.FindAndInsert("HOVEL", 2);
            }

            // If 
            if (StandaloneDraftedID != 100 && NewList.Count > 3) {
                List<string> FirstThree = [NewList[0], NewList[1], NewList[2]];
                if (StandaloneDraftedID == 0 && FirstThree.Contains("TOMB"))
                {
                    NewList.FindAndInsert("TOMB", 4);
                }
                else if (StandaloneDraftedID == 1 && FirstThree.Contains("TOMB"))
                {
                    NewList.FindAndInsert("TOOL SHED", 4);
                }
                else if (StandaloneDraftedID == 2 && FirstThree.Contains("TOOL SHED"))
                {
                    NewList.FindAndInsert("TRADING POST", 4);
                }
                else if (StandaloneDraftedID == 3 && FirstThree.Contains("HOVEL"))
                {
                    NewList.FindAndInsert("HOVEL", 4);
                }
                else if (StandaloneDraftedID == 4 && FirstThree.Contains("BOMB SHELTER"))
                {
                    NewList.FindAndInsert("BOMB SHELTER", 4);
                }
                else if (StandaloneDraftedID == 5 && FirstThree.Contains("ROOT CELLAR"))
                {
                    NewList.FindAndInsert("ROOT CELLAR", 4);
                }
                else if (StandaloneDraftedID == 6 && FirstThree.Contains("SHRINE"))
                {
                    NewList.FindAndInsert("SHRINE", 4);
                }
                else if (StandaloneDraftedID == 6 && FirstThree.Contains("SCHOOLHOUSE"))
                {
                    NewList.FindAndInsert("SCHOOLHOUSE", 4);
                }
            }

            // Bedrooms more common today.
            if (colorBools[0]) {
                NewList.FindAndInsert("HOVEL", 0);
            }

            // Green Rooms more common today.
            if (colorBools[4]) 
            {
                NewList.FindAndInsert("ROOT CELLAR", 0);
            }

            // Shop Rooms more common today.
            if (colorBools[3])
            {
                NewList.FindAndInsert("TRADING POST", 0);
            }

            // Handles upgrade color rarities.
            if (colorBools[5]) {
                int BlueRNG = rng.Next(4);
                switch (BlueRNG)
                {
                    case 0:
                        NewList.FindAndInsertInOrder(["TOOLSHED", "BOMB SHELTER", "SHRINE"]);
                        break;
                    case 1:
                        NewList.FindAndInsertInOrder(["SCHOOLHOUSE", "TOOLSHED", "BOMB SHELTER", "SHRINE"]);
                        break;
                    case 2:
                        NewList.FindAndInsertInOrder(["SHRINE", "BOMB SHELTER", "SCHOOLHOUSE"]);
                        break;
                    case 3:
                        NewList.FindAndInsertInOrder(["TOOLSHED", "SHRINE", "BOMB SHELTER", "SCHOOLHOUSE"]);
                        break;
                    default:
                        break;
                }
            }

            // Handles Draxus
            if (ModInstance.RDHelper.EnableDraxus || colorBools[6]) {
                NewList.FindAndInsert("TOMB");
            }
            return GenerateOuterList(NewList);
        }

        /// <summary>
        ///     Generates the Outer Draft Room List Based on the current list of Draftable Rooms.
        /// </summary>
        /// <param name="rooms">A list containing room names.</param>
        /// <returns>A list of GameObjects representing rooms.</returns>
        private List<ModRoom> GenerateOuterList(List<string> rooms) {
            List<ModRoom> NewList = new();
            foreach (string room in rooms) {
                NewList.Add(GetRoomByName(room));
            }
            return NewList;
        }

        /// <summary>
        ///     Confirms all colors are set correctly based on the activated color based rarity effects such as King and Scepter.
        /// </summary>
        /// <returns>A List of bools with which colors are active.</returns>
        private List<bool> ConfirmColors() {
            //                       0: Violet                           1: Orange                            2: Red                              3: Yellow                         4: Green                               5: Blue                               6: Black                                  
            List<bool> colorBools = [ModInstance.RDHelper.EnableBedroom, ModInstance.RDHelper.EnableHallways, ModInstance.RDHelper.EnableFurnace, ModInstance.RDHelper.EnableShops, ModInstance.RDHelper.EnableGreenhouse, ModInstance.RDHelper.EnableBlueRooms, ModInstance.RDHelper.EnableBlackRooms];
            PlayMakerFSM ColorFSM = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /TEXT/INVENTORY INSPECT/Inventory Descriptions/ROYAL SCEPTER/You Found Royal Scepter/Text/GameObject/buttons/CONFIRM BUTTON").GetComponent<PlayMakerFSM>();
            string sceptorColor = ColorFSM.GetStringVariable("Scepter Color").Value;
            string kingColor = ModInstance.GlobalPersistentManager.GetIntVariable("Chess Power").Value == 6 ? ModInstance.ChessKing.GetStringVariable("King Color").Value : "";
            if (sceptorColor == "BEDROOMS" || kingColor == "BEDROOMS") {
                colorBools[0] = true;
            }
            else if (sceptorColor == "HALLWAYS" || kingColor == "HALLWAYS")
            {
                colorBools[1] = true;
            }
            else if (sceptorColor == "RED ROOMS" || kingColor == "RED ROOMS")
            {
                colorBools[2] = true;
            }
            else if (sceptorColor == "SHOP ROOMS" || kingColor == "SHOP ROOMS")
            {
                colorBools[3] = true;
            }
            else if (sceptorColor == "GREEN ROOMS" || kingColor == "GREEN ROOMS")
            {
                colorBools[4] = true;
            }
            else if (sceptorColor == "BLUEPRINTS" || kingColor == "BLUEPRINTS")
            {
                colorBools[5] = true;
            }
            else if (sceptorColor == "BLACKPRINTS" || kingColor == "BLACKPRINTS")
            {
                colorBools[6] = true;
            }
            return colorBools;
        }

        /// <summary>
        ///     Sets Variables in the MasterPicker for use in the Outer Draft Override.
        /// </summary>
        /// <param name="RoomList"></param>
        /// <param name="rerolls"></param>
        public void SetOuterDraftRooms(List<ModRoom> RoomList, int rerolls) {
            int index = (0 + 3 * rerolls);

            ModRoom Room1 = RoomList[index % (RoomList.Count)];
            ModRoom Room2 = RoomList[(index + 1) % (RoomList.Count)];
            ModRoom Room3 = RoomList[(index + 2) % (RoomList.Count)];
            ModInstance.MasterPicker.GetGameObjectVariable("OuterRoom1").Value = Room1.GameObj;
            ModInstance.MasterPicker.GetGameObjectVariable("OuterRoom2").Value = Room2.GameObj;
            ModInstance.MasterPicker.GetGameObjectVariable("OuterRoom3").Value = Room3.GameObj;
        }

        /// <summary>
        ///     Updates only the picker arrays being used in the current draft.
        /// </summary>
        public void UpdateCurrentPickerArrays() {
            PlayMakerFSM grid = ModInstance.TheGrid;
            PlayMakerFSM planPicker = grid.GetGameObjectVariable("theplanpick").value?.GetComponent<PlayMakerFSM>();
            CurrentPickerArrays.Clear();
            CurrentPickerLists.Clear();
            //Check all the states for SetFsmGameObject actions. If that action is setting one of the picker arrays, add it to the current picker list.
            if (planPicker != null)
            {
                foreach (FsmState state in planPicker.FsmStates)
                {
                    foreach (SetFsmGameObject action in state.GetActionsOfType<SetFsmGameObject>())
                    {
                        //Add the array to the list if it's getting set as a picker array, and it's not already on the list (some pickers use dupe lists).
                        if (action.variableName.value.Contains("Array") && ! CurrentPickerArrays.Contains(action.setValue.Value.name))
                        {
                            if (action.setValue.Value.name.Contains("CENTER"))
                            {
                                CurrentPickerArrays.Add("CENTER - Tier 1");
                                CurrentPickerArrays.Add("CENTER - Tier 2");
                                CurrentPickerArrays.Add("CENTER - Tier 3");
                                CurrentPickerArrays.Add("CENTER - Tier 1 G");
                                CurrentPickerArrays.Add("CENTER - Tier 2 G");
                                CurrentPickerArrays.Add("CENTER - Tier 3 G");
                            }
                            else
                            {
                                CurrentPickerArrays.Add(action.setValue.Value.name);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Represents a classroom in the draft pool with all its state.
    /// </summary>
    /// <param name="name">Internal name for the mod (e.g., "CLASSROOM (1)")</param>
    /// <param name="gameObjectName">Actual game object name in Room Engines (e.g., "CLASSROOM")</param>
    /// <param name="gameObject">The Unity GameObject for this room</param>
    /// <param name="pickerArrays">List of picker arrays this room can appear in</param>
    /// <param name="isUnlocked">Whether the room is initially unlocked</param>
    /// <param name="useVanilla">Whether to use vanilla handling for this room</param>
    /// <param name="hasBeenDrafted">Whether this room has been drafted this run</param>
    public class ClassRoom(string name, string gameObjectName, GameObject gameObject, List<string> pickerArrays, bool isUnlocked, bool useVanilla = false, bool hasBeenDrafted = false) : ModRoom(name, gameObjectName, gameObject, pickerArrays, isUnlocked, useVanilla, hasBeenDrafted)
    {
        private int _HighestDrafted = 0;
        public override bool HasBeenDrafted { 
            get { return RoomPoolCount <= _HighestDrafted; } 
            set {
                var beforeDraftCount = Plugin.ModRoomManager.GetRoomByName("CLASSROOM").RoomInHouseCount;
                if (beforeDraftCount > _HighestDrafted)
                {
                    Logging.LogWarning($"More classrooms in house that highest classroom entered, one of the classrooms was probably missed", "ClassRoom");
                }

                if (value && RoomPoolCount > _HighestDrafted)
                {
                    int classroomNumber = _HighestDrafted + 1;
                    string classroomNumberStr = classroomNumber < 9 ? classroomNumber.ToString() : "Exam";
                    ModInstance.ModEventHandler.OnClassroomFirstDrafted(classroomNumberStr);
                    _HighestDrafted += 1;
                }
            }     
        }
    }

    /// <summary>
    ///     Represents a room in the draft pool with all its state.
    /// </summary>
    /// <param name="name">Internal name for the mod (e.g., "CLASSROOM (1)")</param>
    /// <param name="gameObjectName">Actual game object name in Room Engines (e.g., "CLASSROOM")</param>
    /// <param name="gameObject">The Unity GameObject for this room</param>
    /// <param name="pickerArrays">List of picker arrays this room can appear in</param>
    /// <param name="isUnlocked">Whether the room is initially unlocked</param>
    /// <param name="useVanilla">Whether to use vanilla handling for this room</param>
    /// <param name="hasBeenDrafted">Whether this room has been drafted this run</param>
    /// <param name="upgradeObjs">The GameObjects for the Upgraded versions of the Room.</param>
    /// <param name="upgradeID">The Upgrade ID of this instance of the Room</param>
    public class ModRoom(string name, string gameObjectName, GameObject gameObject, List<string> pickerArrays, bool isUnlocked, bool useVanilla = false, bool hasBeenDrafted = false, List<GameObject> upgradeObjs = null, int upgradeID = 0)
    {
#pragma warning disable CS9124 // Parameter is captured into the state of the enclosing type and its value is also used to initialize a field, property, or event.
        private string _Name = name;
#pragma warning restore CS9124 // Parameter is captured into the state of the enclosing type and its value is also used to initialize a field, property, or event.
        public string Name { get { return _Name; } set { _Name = value; } }
        
        // The actual game object name used in "__SYSTEM/The Room Engines/"
        private string _GameObjectName = gameObjectName;
        public string GameObjectName { get { return _GameObjectName; } set { _GameObjectName = value; } }

        private GameObject _GameObj = gameObject;
        public GameObject GameObj { get { return _GameObj; } set { _GameObj = value; } }

        public List<GameObject> UpgradeObjects { get; set; } = upgradeObjs ?? new List<GameObject>();

        public int UpgradeID = upgradeID;

        private List<string> _PickerArrays = pickerArrays;
        public List<string> PickerArrays { get { return _PickerArrays; } set { _PickerArrays = value; } }

        private RoomHandler _Handler = RoomHandler.CreateRoomHandler(name);
        public RoomHandler Handler { get { return _Handler; } set { _Handler = value; } }

        private bool _IsUnlocked = isUnlocked;

        public List<Func<ModRoom,bool>> Dependencies = new List<Func<ModRoom, bool>>();

        public bool AddedToDirectory = false;

        public bool IsUnlocked {
            get { return _IsUnlocked; }
            set {
                // Update the FSM bool variable to control pool removal
                string roomPath = "__SYSTEM/The Room Engines/" + _GameObjectName;
                GameObject roomEngine = GameObject.Find(roomPath);
                if (roomEngine != null)
                {
                    PlayMakerFSM fsm = roomEngine.GetComponent<PlayMakerFSM>();
                    if (fsm != null)
                    {
                        FsmBool poolRemovalVar = fsm.GetBoolVariable("POOL REMOVAL");
                        if (poolRemovalVar != null)
                        {
                            // POOL REMOVAL = true means room is NOT available (removed from pool)
                            // POOL REMOVAL = false means room IS available (in pool)
                            poolRemovalVar.Value = !value;
                            Logging.LogDebug($"Room '{Name}' (GO: {_GameObjectName}) POOL REMOVAL set to {!value} (IsUnlocked={value})");
                        }
                        else
                        {
                            Logging.LogWarning($"Room '{Name}' (GO: {_GameObjectName}): Could not find 'POOL REMOVAL' variable in FSM");
                        }
                    }
                    else
                    {
                        Logging.LogWarning($"Room '{Name}' (GO: {_GameObjectName}): Could not find FSM named '{_GameObjectName}'");
                    }
                }
                else
                {
                    // This is expected if scene isn't loaded yet
                    Logging.LogDebug($"Room '{Name}': Room engine not found at '{roomPath}' (scene may not be loaded)");
                }
                _IsUnlocked = value;
                Handler?.OnRoomUnlocked(this);
            }
        }

        // Stores if the room has been drafted for tracking checks.
        private bool _HasBeenDrafted = hasBeenDrafted;
        public virtual bool HasBeenDrafted { 
            get { return _HasBeenDrafted; } 
            set {
                //Send the room drafted event on the first time this room is drafted only.
                if (!_HasBeenDrafted && value)
                {
                        ModInstance.ModEventHandler.OnFirstDrafted(this);
                        _HasBeenDrafted = value;

                }
                // No changes to value once the room has been drafted once, or if someone is not trying to set this to true for some stupid reason.
            }     
        }


        // For handling special rooms. Defaults to things that are not in the randomizable pool.
        private bool _UseVanilla = !useVanilla;

        public bool UseVanilla { get {return _UseVanilla;} set { _UseVanilla = value; } }

        // The number of this room that can be in the pool
        private int _RoomPoolCount = 1;
        public int RoomPoolCount 
        {
            get { return _RoomPoolCount; }
            set {
                if (value < 0)
                {
                    _RoomPoolCount = 0;
                    Logging.LogWarning("Cannot set roomcount to below 0");
                }
                else if (value > 1 && (ModRoomManager.CantCopy.Contains(Name)))
                {
                    Logging.LogWarning($"Cannot have more than 1 copy of the {Name}, it will break your save file/run.");
                }
                else {
                    _RoomPoolCount = value;
                }
            }
        }

       private int _RoomPoolAdjustment = 0;
       public int RoomPoolAdjustment
       { 
            get { return _RoomPoolAdjustment; }
            set { _RoomPoolAdjustment = value; }
       }

       private int _RoomMaxAdjustment = 0;
       public int RoomMaxAdjustment
       {
            get { return _RoomMaxAdjustment; }
            set { _RoomMaxAdjustment = value; }
       }

        // tracks how many copies of the room are in the house.
        private int _RoomInHouseCount = 0;

       public int RoomInHouseCount {
            get { return _RoomInHouseCount;} 
            set { _RoomInHouseCount = value + _RoomMaxAdjustment; }
       }

        public int RoomsLeftInPool {
            get { 
                int left = _RoomPoolCount - RoomInHouseCount + _RoomPoolAdjustment;
                return left > 0 ? left : 0; // Ensure we never return negative
            }
        }

        /// <summary>
        ///     Adds copy(s) of this room to the pool array
        /// </summary>
        /// <param name="array">The Picker Array to add it to</param>
        /// <param name="count">The number to add to the pool</param>
        private void AddToPool(PlayMakerArrayListProxy array, int count = 1) {
            // Ensure we have a valid GameObject to add
            if (_GameObj == null)
            {
                Logging.LogWarning($"Adding {count} {name}(s) to pool.");
                // Try to get the GameObject from the Room Engines using the game object name
                _GameObj = GameObject.Find("__SYSTEM/The Room Engines/" + _GameObjectName);
                if (_GameObj == null)
                {
                    Logging.LogWarning($"Cannot add {Name} to pool: GameObject is null (looked for '{_GameObjectName}')");
                    return;
                }
            }
            //Checks dependencies of the room before adding it to the draft pool.
            foreach (Func<ModRoom, bool> dependency in Dependencies) {
                Logging.LogWarning($"Checking Dependency of {Name}");
                if (!dependency(this)) {
                    Logging.LogWarning($"Cannot add {Name} to pool: Dependency not met");
                    return;
                }
            }

            for (int i = 0; i < count; i++)
            {
                array.Add(_GameObj, "GameObject");
                Logging.Log($"Added {Name} (GO: {_GameObjectName}) to {array.name}");
            }
        }
        /// <summary>
        ///     Removes copy(s) of the room from the picker array.
        /// </summary>
        /// <param name="array">The array to remove from</param>
        /// <param name="count">The number to remove from the pool.</param>
        private void RemoveFromPool(PlayMakerArrayListProxy array, int count = 1) {
            if (_GameObj == null)
            {
                Logging.LogWarning($"Cannot remove {Name} from pool: GameObject is null");
                return;
            }
            bool removed = false;
            for (int i = 0; i < count; i++)
            {
                if (array.Contains(_GameObj))
                {
                    array.Remove(_GameObj, "GameObject");
                    Logging.Log($"Removed {Name} from {array.name}");
                    removed = true;
                }
                // Handle the Upgraded objects.
                foreach (GameObject upgrade in UpgradeObjects) {
                    if (array.Contains(upgrade))
                    {
                        Logging.LogWarning("Removed Upgraded Room From Pool");
                        array.Remove(upgrade, "GameObject");
                        Logging.Log($"Removed {Name} from {array.name}");
                        removed = true;
                    }
                }
                if (!removed)
                {
                    Logging.Log($"{Name} doesn't exist in the pool {array.name}");
                }
            }
        }

        /// <summary>
        ///     Set the FSMBools in the appropriate room to ensure that the correct rooms show up in draft.
        /// </summary>
        public void Initialize()
        {
            var pool = GameObject.Find("__SYSTEM/The Room Engines/" + _GameObjectName)?.GetFsm(_GameObjectName)?.GetBoolVariable("POOL REMOVAL");
            if (pool == null) return;
            if (!IsUnlocked)
            {
                pool.Value = false;
            }
            else
            {
                pool.Value = false;
            }
        }

        /// <summary>
        ///     Helper function that updates 1 array at a time.
        /// </summary>
        /// <param name="array">The array to update</param>
        /// <param name="count">The number of copies that should be in the pool.</param>
        public void UpdateArray(PlayMakerArrayListProxy array, int count) {
            if (RoomsLeftInPool > 0)
            {
                // If the room has at least one copy currently in the pool
                if ((count > 0 && _IsUnlocked && !_UseVanilla))
                {
                    // check if there are more copies than there should be
                    if (count > RoomsLeftInPool)
                    {
                        RemoveFromPool(array, count - RoomsLeftInPool);
                        count -= (count - RoomsLeftInPool);
                    }
                    // check if there less copies than there should be
                    else if (RoomsLeftInPool > count)
                    {
                        AddToPool(array, RoomsLeftInPool - count);
                        count += RoomsLeftInPool - count;
                    }
                }
                // check if there are still rooms that should be in the pool but aren't
                else if (RoomsLeftInPool > 0 && _IsUnlocked && !_UseVanilla)
                {
                    AddToPool(array, RoomsLeftInPool);
                }
                // Handle extra copies of rooms that use vanilla logic. Assume always 1 is default (no extra copies), and that the rest is extra.
                else if (_RoomPoolCount > 1 && _RoomPoolCount -1 != count && _UseVanilla && ! ModRoomManager.CantCopy.Contains(Name)) {
                    AddToPool(array, _RoomPoolCount -1);
                }
                // If copies in pool and not set to use vanilla logic, remove from pool.
                else if (count > 0 && !_UseVanilla)
                {
                    RemoveFromPool(array, count);
                }
            }
        }

        /// <summary>
        ///     Adds a Dependency to the room
        /// </summary>
        /// <param name="dependency">A Function that checks the dependency.</param>
        public void AddDependency(Func<ModRoom, bool> dependency)
        {
            Dependencies.Add(dependency);
        }
        /// <summary>
        ///     Adds a collection of dependencies to a room
        /// </summary>
        /// <param name="dependencies">A collection of functions that check if dependencies for those rooms are met.</param>
        public void AddDependencies(params Func<ModRoom, bool>[] dependencies) {
            Dependencies.AddRange(dependencies);
        }
    }
}

using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms
{
    /// <summary>
    ///     A Manager that handles operations related to rooms or drafting.
    /// </summary>
    public static class ModRoomManager
    {
        private static List<ModRoom> _Rooms = [];
        public static List<ModRoom> Rooms
        {
            get { return _Rooms; }
            set { _Rooms = value; }
        }
        public static ModRoom ForcedRoom = null;
        public static bool IsForcingDraft = false;

        public static List<string> VanillaRooms = [];
        public static List<string> CantCopy = ["ANTECHAMBER", "ENTRANCE HALL", "ROOM 46", "FOUNDATION", ""];
        public static List<string> FoundFloorplans = ["PLANETARIUM", "CONSERVATORY", "TUNNEL", "THRONE ROOM", "TREASURE TROVE", "MECHANARIUM", "LOST & FOUND", "CLOSED EXHIBIT", "CLOCK TOWER", "THE KENNEL", "VESTIBULE", "DOVECOTE", "SOLARIUM", "DORMITORY", "CASINO", "SAUNA", "LOCKER ROOM", "MORNING ROOM", "CLASSROOM"];
        public static List<ModRoom> OuterDraftRooms = new();
        public static Dictionary<string, PlayMakerArrayListProxy> PickerDict { set; get; } = [];
        public static Dictionary<string, PlayMakerArrayListProxy> UntouchedPickers { set; get; } = [];

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

        /// <summary>
        /// Clears all room state so InitializeRooms can be safely called again (e.g. on scene reload).
        /// </summary>
        public static void Reset()
        {
            _Rooms.Clear();
            VanillaRooms.Clear();
            ForceRoomQueue.Clear();
            ForcedRoom = null;
            IsForcingDraft = false;
            Logging.Log("ModRoomManager reset.");
        }

        /// <summary>
        /// Re-loads the picker arrays. Call this when arrays may have been reset by the game.
        /// </summary>
        public static void ReloadArrays()
        {
            Logging.Log("Reloading picker arrays...");
            PickerDict.Clear();
            UntouchedPickers.Clear();
            LoadArrays();
            Logging.Log($"Reloaded {PickerDict.Count} picker arrays.");
        }

        /// <summary>
        ///     Syncs room pools with Archipelago received items. 
        ///     Should be called at the start of each day when connected to Archipelago.
        ///     Only operates if RoomDraftSanity option is enabled.
        ///     Even with no items received, this will lock all rooms for Archipelago mode.
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
            ClearAllRoomsForArchipelago();

            // Unlock rooms we've received from Archipelago (if any)
            var receivedItems = ArchipelagoClient.ServerData.ReceivedItems;
            int unlockedCount = 0;
            if (receivedItems != null && receivedItems.Count > 0)
            {
                foreach (string itemName in receivedItems)
                {
                    if (UnlockRoomForArchipelago(itemName))
                    {
                        unlockedCount++;
                    }
                }
            }

            // Update the actual picker arrays
            UpdateRoomPools();

            Logging.Log($"Auto-sync complete: {unlockedCount} rooms unlocked from Archipelago.", "Rooms");
        }

        /// <summary>
        ///     Lightweight method to ensure room unlock states match Archipelago received items.
        ///     Unlike full sync, this doesn't reset counts or clear rooms — just ensures unlock states are correct.
        ///     Call this before UpdateRoomPools() when a draft is about to start.
        ///     Only operates if RoomDraftSanity option is enabled.
        /// </summary>
        public static void EnsureRoomUnlockStates()
        {
            if (!ArchipelagoClient.Authenticated) return;

            // Skip if RoomDraftSanity is disabled
            if (!ArchipelagoOptions.RoomDraftSanity) return;

            var receivedItems = ArchipelagoClient.ServerData.ReceivedItems;
            if (receivedItems == null || receivedItems.Count == 0) return;

            // Lock all rooms that aren't using vanilla handling
            foreach (var room in Rooms)
            {
                if (!room.UseVanilla)
                {
                    room.IsUnlocked = false;
                }
            }

            // Unlock rooms we've received from Archipelago
            foreach (string itemName in receivedItems)
            {
                UnlockRoomForArchipelago(itemName);
            }
        }

        //TODO update this to be less hacky.
        /// <summary>
        ///     loads the list of picker arrays the rooms can be added to. 
        ///     May rewrite to use names instead of the id of the child for better forward compatibility.
        /// </summary>
        public static void LoadArrays()
        {
            // Core picker arrays (indexes 2-32, 55-56, 58-61)
            PlayMakerArrayListProxy array = null;
            List<int> coreChildIDs = [2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32];
            for (int i = 0; i < coreChildIDs.Count; i++)
            {
                array = ModInstance.PlanPicker.transform.GetChild(coreChildIDs[i]).gameObject.GetComponent<PlayMakerArrayListProxy>();
                if (array != null)
                {
                    PickerDict[array.name.Trim()] = array;
                }
            }

            for (int i = 1; i < 31; i++)
            {
                array = GameObject.Find("__SYSTEM/Room Lists/UntouchedPickers").transform.GetChild(i).gameObject.GetComponent<PlayMakerArrayListProxy>();
                if (array != null)
                {
                    UntouchedPickers[array.name.Trim()] = array;
                }
            }

            // Standalone Array Full
            array = ModInstance.PlanPicker.transform.GetChild(56).gameObject.GetComponent<PlayMakerArrayListProxy>();
            if (array != null)
            {
                UntouchedPickers["STANDALONE ARRAY"] = array;
            }

            //// Additional arrays that may be needed for special drafts (like Entrance Hall, first draft, etc.)
            //List<int> additionalChildIDs = [0, 33, 34, 35, 36, 37, 38, 39, 40, 44, 45, 57];
            //for (int i = 0; i < additionalChildIDs.Count; i++) {
            //    PlayMakerArrayListProxy array = PlanPicker.transform.GetChild(additionalChildIDs[i]).gameObject?.GetComponent<PlayMakerArrayListProxy>();
            //    if (array != null) {
            //        PickerDict[array.name.Trim()] = array;
            //        Logging.Log($"Loaded additional array: {array.name} with {array.GetCount()} rooms");
            //    }
            //}
        }

        /// <summary>
        ///     Handles any room settings that need to be set on Day start
        /// </summary>
        public static void StartOfDay()
        {
            Transform RoomSpawnPools = GameObject.Find("__SYSTEM/Room Spawn Pools/").transform;
            for (int i = 0; i < RoomSpawnPools.childCount; i++)
            {
                Transform child = RoomSpawnPools.GetChild(i);
                if (child != null)
                {
                    if (child.name.Contains("Foundation"))
                    {
                        // Remove a copy of the Foundation to prevent extra foundations from being in the pool.
                        GetRoomByName("The Foundation").RoomPoolAdjustment = -1;
                    }
                }
            }
            // Despawn Foundation Upgrade Disk
            if (ModItemManager.UpgradeDisks.FoundLocations.Contains("Foundation"))
            {
                Transform FoundationSpawn = GameObject.Find("UNDERGROUND").transform.Find("Below Foundation (Cullable)").Find("Below Foundation - Prefab").Find("_GAMEPLAY").Find("5");
                if (FoundationSpawn.childCount > 0)
                {
                    Logging.LogWarning("Despawning Foundation Upgrade Disk.");
                    GameObject.Destroy(FoundationSpawn.GetChild(0).gameObject);
                }
            }
        }

        /// <summary>
        ///     Adds a room to the tracked mod pool.
        /// </summary>
        /// <param name="room">The ModRoom of the room to add.</param>
        /// <returns>The Modroom provided.</returns>
        public static ModRoom AddRoom(ModRoom room)
        {
            bool found = false;
            int counter = -1;
            // check if room already exists in the room pool
            while (!found && counter < _Rooms.Count - 1)
            {
                counter++;
                if (_Rooms[counter].Name == room.Name)
                {
                    found = true;
                }
            }
            if (found)
            {
                Logging.LogWarning($"{_Rooms[counter].Name} already in Pool, adding more to the pool");
                _Rooms[counter].RoomPoolCount++;
                room.Initialize();
            }
            else
            {
                _Rooms.Add(room);
                room.Initialize();
                if (room.UseVanilla)
                {
                    VanillaRooms.Add(room.Name);
                }
            }
            return room;
        }

        /// <summary>
        ///     Forces a Draft for a given room
        /// </summary>
        /// <param name="roomname">The name of the room to force.</param>
        public static void ForceDraft(string roomname)
        {
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
        public static void ForceDraft(ModRoom room)
        {
            if (room != null)
            {
                ForceRoomQueue.Add(room);
                return;
            }
            Logging.LogWarning("Error forcing room, room can't be null");
        }

        /// <summary>
        ///     Checks if there is a valid room to force. If it can be forced force it and return true.
        /// </summary>
        /// <returns>True if a room was forced, False if not</returns>
        public static bool CheckForceRoomDraft()
        {
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
        public static void RecheckRoomUnlockStatus()
        {
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
        public static void RemoveRoom(ModRoom room)
        {
            if (room.RoomPoolCount > 0)
            {
                room.RoomPoolCount -= 1;
            }
            room.IsUnlocked = false;
        }
        /// <summary>
        ///     A fix for the HLC being deactivated for days 8+ on veteran mode.
        /// </summary>
        public static void HLCFix()
        {
            if (ModInstance.GlobalPersistentManager.GetBoolVariable("_Veteran Player").Value)
            {
                ModRoom HLC = GetRoomByName("HER LADYSHIP\'S CHAMBER");
                if (HLC.IsUnlocked)
                {
                    SetPoolRemovalVar("HER LADYSHIP\'S CHAMBER");
                }
            }
        }

        /// <summary>
        /// Resets the room in house count for all rooms. Call this at the start of a new day.
        /// </summary>
        public static void ResetRoomInHouseCounts()
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
        public static ModRoom GetRoomByName(string name)
        {
            foreach (ModRoom room in _Rooms)
            {
                if (room.Name.ToUpper().Trim() == name.ToUpper().Trim() || room.GameObjectName.ToUpper().Trim() == name.ToUpper().Trim())
                {
                    return room;
                }
                foreach (GameObject gameObject in room.UpgradeObjects)
                {
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
        public static ModRoom AddRoom(string name, List<string> pickerArrays, bool isUnlocked, bool useVanilla = false, bool hasBeenDrafted = false)
        {
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
        public static ModRoom AddRoom(string name, string gameObjectName, List<string> pickerArrays, bool isUnlocked, bool useVanilla = false, bool hasBeenDrafted = false)
        {
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
                for (int i = 0; i < roomObj.transform.childCount; i++)
                {
                    GameObject child = roomObj?.transform?.GetChild(i)?.gameObject;
                    if (child != null)
                    {
                        UpgradeObjs.Add(child);
                    }
                }
                // Get the current UpgradeID of the room
                int UpgradeID = 0;
                if (UpgradeIDs.ContainsKey(name))
                {
                    UpgradeID = ModInstance.GlobalPersistentManager.GetIntVariable(UpgradeIDs[name]).Value;
                }
                else
                {
                    Logging.LogWarning($"UpgradeID variable could not be found for {name}.");
                }
                return AddRoom(new ModRoom(name, gameObjectName, roomObj, pickerArrays, isUnlocked, useVanilla, hasBeenDrafted, UpgradeObjs, UpgradeID));
            }
            return AddRoom(new ModRoom(name, gameObjectName, roomObj, pickerArrays, isUnlocked, useVanilla, hasBeenDrafted));
        }

        /// <summary>
        ///     Attempts to update all the picker arrays to match what the mod expects them to be.
        /// </summary>
        public static void UpdateRoomPools()
        {
            Logging.Log("Updating Room Pools");
            foreach (string key in ModRoomManager.PickerDict.Keys)
            {
                PlayMakerArrayListProxy untouchedArray = ModRoomManager.UntouchedPickers[key];
                PlayMakerArrayListProxy array = ModRoomManager.PickerDict[key];
                int length = array.arrayList.Count;
                GameObject room = null;
                ModRoom modRoom = null;
                Dictionary<string, int> RoomCounts = new Dictionary<string, int>();
                for (int i = 0; i < length; i++)
                {
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
                        else
                        {
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
                foreach (string roomName in FoundFloorplans)
                {
                    if (!RoomCounts.ContainsKey(roomName) && !updated.Contains(roomName))
                    {
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
        public static void EmptyDraftPool()
        {
            foreach (ModRoom room in _Rooms)
            {
                room.IsUnlocked = false || room.UseVanilla; //Set the room to not unlocked, unless the room is set to use Vanilla Handling.
            }
        }

        /// <summary>
        ///     Clears the entire draft pool for Archipelago mode - locks ALL rooms regardless of vanilla status.
        ///     Use this when syncing with Archipelago to start fresh.
        /// </summary>
        public static void ClearAllRoomsForArchipelago()
        {
            Logging.Log("Clearing all rooms for Archipelago sync...");
            ResetRoomInHouseCounts(); // Reset counts so pools calculate correctly
            foreach (ModRoom room in _Rooms)
            {
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
        public static bool IsRoomItem(string itemName)
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
        public static bool UnlockRoomForArchipelago(string roomName)
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
        public static string GetMappedRoomName(string apRoomName)
        {
            return MapArchipelagoRoomName(apRoomName);
        }

        /// <summary>
        /// Maps Archipelago room names to actual game room names.
        /// Handles special cases for rooms with non-standard naming.
        /// </summary>
        private static string MapArchipelagoRoomName(string apRoomName)
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
        public static void SetPoolRemovalVar(string name, bool value = false)
        {
            string roomPath = "__SYSTEM/The Room Engines/" + name;
            GameObject roomEngine = GameObject.Find(roomPath);
            Logging.LogWarning(name);
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
        public static List<ModRoom> OuterDraftPrePickShuffling()
        {
            List<string> OuterRooms = ["TOOLSHED", "BOMB SHELTER", "SCHOOLHOUSE", "SHRINE", "ROOT CELLAR", "HOVEL", "TRADING POST", "TOMB"];
            List<string> NewList = new List<string>();
            List<ModRoom> Output = new List<ModRoom>();

            // Keep on the rooms that are unlocked.
            foreach (string room in OuterRooms)
            {
                ModRoom modRoom = GetRoomByName(room);
                if (modRoom.IsUnlocked)
                {
                    NewList.Add(room);
                }
            }

            // Replace with Closets if none.
            if (NewList.Count == 0)
            {
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
                if (NewList.Count == 1)
                {
                    Output.Add(Output[0]);
                    Output.Add(Output[0]);
                }
                else if (NewList.Count == 2)
                {
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
            if (StandaloneDraftedID != 100 && NewList.Count > 3)
            {
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
            if (colorBools[0])
            {
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
            if (colorBools[5])
            {
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
            if (ModInstance.RDHelper.EnableDraxus || colorBools[6])
            {
                NewList.FindAndInsert("TOMB");
            }
            return GenerateOuterList(NewList);
        }

        /// <summary>
        ///     Generates the Outer Draft Room List Based on the current list of Draftable Rooms.
        /// </summary>
        /// <param name="rooms">A list containing room names.</param>
        /// <returns>A list of GameObjects representing rooms.</returns>
        private static List<ModRoom> GenerateOuterList(List<string> rooms)
        {
            List<ModRoom> NewList = new();
            foreach (string room in rooms)
            {
                NewList.Add(GetRoomByName(room));
            }
            return NewList;
        }

        /// <summary>
        ///     Confirms all colors are set correctly based on the activated color based rarity effects such as King and Scepter.
        /// </summary>
        /// <returns>A List of bools with which colors are active.</returns>
        private static List<bool> ConfirmColors()
        {
            //                       0: Violet                           1: Orange                            2: Red                              3: Yellow                         4: Green                               5: Blue                               6: Black                                  
            List<bool> colorBools = [ModInstance.RDHelper.EnableBedroom, ModInstance.RDHelper.EnableHallways, ModInstance.RDHelper.EnableFurnace, ModInstance.RDHelper.EnableShops, ModInstance.RDHelper.EnableGreenhouse, ModInstance.RDHelper.EnableBlueRooms, ModInstance.RDHelper.EnableBlackRooms];
            PlayMakerFSM ColorFSM = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /TEXT/INVENTORY INSPECT/Inventory Descriptions/ROYAL SCEPTER/You Found Royal Scepter/Text/GameObject/buttons/CONFIRM BUTTON").GetComponent<PlayMakerFSM>();
            string sceptorColor = ColorFSM.GetStringVariable("Scepter Color").Value;
            string kingColor = ModInstance.GlobalPersistentManager.GetIntVariable("Chess Power").Value == 6 ? ModInstance.ChessKing.GetStringVariable("King Color").Value : "";
            if (sceptorColor == "BEDROOMS" || kingColor == "BEDROOMS")
            {
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
        public static void SetOuterDraftRooms(List<ModRoom> RoomList, int rerolls)
        {
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
        public static void UpdateCurrentPickerArrays()
        {
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
                        if (action.variableName.value.Contains("Array") && !CurrentPickerArrays.Contains(action.setValue.Value.name))
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

        /// <summary>
        ///     Internal. Intializes all of the base game rooms as mod objects so the mod can track details about them.
        /// </summary>
        public static void InitializeRooms()
        {
            Logging.Log("Initializing Rooms");
            // Checks if the pool is in the house.
            Func<ModRoom, bool> poolCheck = (room) => { return (ModRoomManager.GetRoomByName("THE POOL").RoomInHouseCount > 0); };
            // Checks if the garage is already in the house and can currently be drafted.
            Func<ModRoom, bool> garageRankCheck = (room) => {
                int targetRank = ModInstance.TheGrid.GetIntVariable("Taret Rank").Value;
                int currentRank = ModInstance.TheGrid.GetIntVariable("Current Rank").Value;
                int targetTile = ModInstance.TheGrid.GetIntVariable("Target Tile").Value;
                return room.RoomInHouseCount == 0 && targetRank > 3 && targetRank < 9 && currentRank <= targetRank && targetTile % 5 != 0; // Rank 4-8, not drafted south, and only on the west side of the house.
            };
            // Checks if the player is drafting north or south (not east/west).
            Func<ModRoom, bool> verticalDraftCheck = (room) => {
                float direction = ModInstance.TheGrid.GetFloatVariable("Cardinal Direction").Value;
                // Use the raw direction since the North South Variable seems to be wrong sometimes.
                return (direction > 150f && direction < 210f) || direction > 330f || direction < 60f;
            };
            // Checks if the foundation can be drafted here.
            Func<ModRoom, bool> foundationCheck = (room) => {

                // Check if the Foundation already exists in the house.
                if (ModInstance.GlobalPersistentManager.GetBoolVariable("Foundation").Value)
                {
                    return false;
                }
                //If the game has set the foundation to be removed.
                if (ModInstance.PlanPicker.GetComponent<PlayMakerFSM>().GetBoolVariable("FoundationRemoval").Value)
                {
                    return false;
                }
                int targetTile = ModInstance.TheGrid.GetIntVariable("Target Tile").Value;
                // If the foundation would be drafted in a location that is not allowed (rank 2 and in south of antechamber).
                if (targetTile == 7 || targetTile == 8 || targetTile == 9 || targetTile == 38)
                {
                    return false;
                }
                // 10% chance to allow Foundation on rank 3
                int lowerRank = UnityEngine.Random.Range(0, 10);
                if (lowerRank != 0)
                {
                    if (targetTile == 12 || targetTile == 13 || targetTile == 14)
                    {
                        return false;
                    }
                }
                return true;
            };
            // Checks if the Secret Passage can be drafted and if so prevents default drafting behaviour.
            Func<ModRoom, bool> secretPassageCheck = (room) => {
                int targetRank = ModInstance.TheGrid.GetIntVariable("Taret Rank").Value;
                return targetRank != 1 && targetRank != 9;
            };
            // Checks if the current chess power is the rook for the armory unlock.
            Func<ModRoom, bool> chessPowerRook = (room) =>
            {
                if (ModInstance.GlobalPersistentManager?.GetIntVariable("Chess Power")?.Value == 2)
                {
                    return true;
                }
                return false;
            };
            // Checks if room 46 has been reached.
            Func<ModRoom, bool> room46Reached = (room) =>
            {
                return ModInstance.GlobalPersistentManager?.GetBoolVariable("Room 46 Reached")?.Value ?? false;
            };

            AddRoom("AQUARIUM", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
            AddRoom("ARCHIVES", ["CENTER - Tier 2"], true);
            AddRoom("ATTIC", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
            AddRoom("BALLROOM", ["FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGECREEP - RARE G"], true);
            AddRoom("BEDROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("BILLIARD ROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("BOILER ROOM", ["CENTER - Tier 2 G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G"], true);
            AddRoom("BOOKSHOP", [""], true, true);
            AddRoom("BOUDOIR", ["SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("BUNK ROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("CASINO", ["FRONTBACK G - RARE", "EDGEPIERCE G", "EDGE ADVANCE EASTWING - G", "EDGE ADVANCE WESTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "NORTH PIERCE G", "CENTER - Tier 1 G", "CORNER - Tier 1 G"], false);
            AddRoom("CHAMBER OF MIRRORS", ["CENTER - Tier 2"], true);
            AddRoom("CHAPEL", ["FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            // CLASSROOM is a single room that can appear as different "grades" (1-9) when drafted
            // All "Classroom X" items from Archipelago map to this single CLASSROOM entry
            AddRoom("CLASSROOM", ["CENTER - Tier 1 G", "FRONT - Tier 1 G", "CORNER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true, false);
            AddRoom("CLOCK TOWER", ["CENTER - Tier 2 G", "FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], false);
            AddRoom("CLOISTER", ["CENTER - Tier 2 G"], true);
            AddRoom("CLOSED EXHIBIT", ["FRONTBACK - RARE", "NORTH PIERCE", "EDGEPIERCE - RARE", "EDGECREEP - RARE", "CENTER - Tier 2"], false);
            AddRoom("CLOSET", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("COAT CHECK", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("COMMISSARY", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
            AddRoom("CONFERENCE ROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
            AddRoom("CONSERVATORY", ["CORNER - Tier 1 G"], false);
            AddRoom("CORRIDOR", ["FRONT - Tier 1", "FRONTBACK - RARE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST"], true);
            AddRoom("COURTYARD", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
            AddRoom("DARKROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("DEN", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("DINING ROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1", "EDGECREEP - RARE", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("DORMITORY", ["CORNER - Tier 1", "FRONTBACK - RARE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], false);
            AddRoom("DOVECOTE", ["EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE", "CENTER - Tier 2"], false);
            AddRoom("DRAFTING STUDIO", ["FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGECREEP - RARE G"], true);
            AddRoom("DRAWING ROOM", ["FRONT - Tier 1 G", "FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("EAST WING HALL", ["EDGECREEP EAST", "EDGEPIERCE EAST"], true);
            AddRoom("FOYER", ["FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGECREEP - RARE G"], true);
            AddRoom("FURNACE", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 3", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
            AddRoom("FREEZER", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true)
                .AddDependency(room46Reached);
            AddRoom("GALLERY", ["FRONT - Tier 1", "FRONTBACK - RARE", "CENTER - Tier 3", "EDGECREEP - RARE"], false);
            AddRoom("GARAGE", ["EDGE ADVANCE WESTWING - G", "EDGEPIERCE G"], true)
                .AddDependency(garageRankCheck);
            AddRoom("GIFT SHOP", ["CENTER - Tier 2", "FRONT - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], false)
                .AddDependency(room46Reached);
            AddRoom("GREAT HALL", ["CENTER - Tier 3"], true);
            AddRoom("GREENHOUSE", ["EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G"], true);
            AddRoom("GUEST BEDROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("GYMNASIUM", ["FRONTBACK - RARE", "NORTH PIERCE", "CENTER - Tier 1", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
            AddRoom("HALLWAY", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CENTER - Tier 1"], true);
            AddRoom("HER LADYSHIP\'S CHAMBER", ["EDGE RETREAT WESTWING -  G"], true);
            AddRoom("HOVEL", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
            AddRoom("KITCHEN", ["FRONT - Tier 1 G", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
            AddRoom("LABORATORY", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
            AddRoom("LAUNDRY ROOM", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
            AddRoom("LAVATORY", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("LIBRARY", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("LOCKER ROOM", ["FRONT - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "CENTER - Tier 2 G"], false)
                .AddDependency(poolCheck);
            AddRoom("LOCKSMITH", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
            AddRoom("LOST & FOUND", ["FRONTBACK - RARE", "CORNER - Tier 1", "EDGECREEP WEST", "EDGECREEP EAST", "EDGEPIERCE WEST", "EDGEPIERCE EAST", "SOUTH PIERCE", "CENTER - Tier 2"], false);
            AddRoom("MAID\'S CHAMBER", ["FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
            AddRoom("MAIL ROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 3", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
            AddRoom("MASTER BEDROOM", ["EDGE ADVANCE EASTWING - G", "EDGE RETREAT EASTTWING -  G"], true);
            AddRoom("MECHANARIUM", ["CENTER - Tier 2"], false);
            AddRoom("MORNING ROOM", ["EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], false, false);
            AddRoom("MUSIC ROOM", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
            AddRoom("NOOK", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("NURSERY", ["FRONT - Tier 1 G", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
            AddRoom("OBSERVATORY", ["FRONT - Tier 1 G", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
            AddRoom("OFFICE", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G", "Center Rare G"], true);
            AddRoom("PANTRY", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("PARLOR", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("PASSAGEWAY", ["CENTER - Tier 1 G"], true);
            AddRoom("PATIO", ["EDGE ADVANCE WESTWING - G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G"], true);
            AddRoom("PLANETARIUM", ["CENTER - Tier 2", "FRONT - Tier 1", "CORNER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE"], false);
            AddRoom("PUMP ROOM", ["FRONTBACK - RARE", "CORNER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE", "CENTER - Tier 2"], true, false)
                .AddDependency(poolCheck);
            AddRoom("ROOM 8", [], false, false);
            AddRoom("ROOT CELLAR", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
            AddRoom("ROTUNDA", ["CENTER - Tier 2 G"], true);
            AddRoom("RUMPUS ROOM", ["FRONTBACK G - RARE", "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "Center Rare G"], true);
            AddRoom("SAUNA", ["CENTER - Tier 1", "FRONT - Tier 1", "CORNER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST", "NORTH PIERCE"], true, false)
                .AddDependency(poolCheck);
            AddRoom("SCHOOLHOUSE", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
            AddRoom("SECRET GARDEN", [""], true, false);
            AddRoom("SECRET PASSAGE", ["CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "Center Rare G"], true)
                .AddDependency(secretPassageCheck);
            AddRoom("SECURITY", ["NORTH PIERCE G", "CENTER - Tier 1 G", "EDGEPIERCE G"], true);
            AddRoom("SERVANT\'S QUARTERS", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 2 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G"], true);
            AddRoom("BOMB SHELTER", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
            AddRoom("SHOWROOM", ["FRONTBACK G - RARE", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "Center Rare G"], true);
            AddRoom("SHRINE", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
            AddRoom("SOLARIUM", ["CORNER - RARE G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G", "NORTH PIERCE G", "CENTER - Tier 2 G"], false);
            AddRoom("SPARE ROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST"], true);
            AddRoom("STOREROOM", ["FRONT - Tier 1", "FRONTBACK - RARE", "SOUTH PIERCE", "CORNER - Tier 1", "CENTER - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("STUDY", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "EDGEPIERCE - RARE", "Center Rare"], true);
            AddRoom("TERRACE", ["EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("THE ARMORY", ["CENTER - Tier 1 G", "CORNER - Tier 1 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G", "NORTH PIERCE G"], false)
                .AddDependency(chessPowerRook);
            AddRoom("THE FOUNDATION", ["CENTER - Tier 1", "CENTER - Tier 2", "CENTER - Tier 3"], true)
                .AddDependency(foundationCheck);
            AddRoom("THE KENNEL", ["FRONT - Tier 1", "EDGECREEP EAST", "EDGECREEP WEST", "CENTER - Tier 1"], false);
            AddRoom("THE POOL", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CENTER - Tier 2 G", "EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G", "EDGEPIERCE G", "Center Rare G"], true);
            AddRoom("THRONE ROOM", ["EDGEPIERCE - RARE G", "CENTER - Tier 2 G"], false);
            AddRoom("TOMB", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
            AddRoom("TOOLSHED", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
            AddRoom("TRADING POST", ["STANDALONE ARRAY", "STANDALONE ARRAY FULL"], true);
            AddRoom("TREASURE TROVE", ["FRONTBACK G - RARE", "CORNER - RARE G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G", "NORTH PIERCE G", "CENTER - Tier 3 G"], false);
            AddRoom("TROPHY ROOM", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 3 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G", "Center Rare G"], true);
            AddRoom("TUNNEL", ["CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST"], false)
                .AddDependency(verticalDraftCheck);
            AddRoom("UTILITY CLOSET", ["FRONT - Tier 1", "FRONTBACK - RARE", "CORNER - Tier 1", "CENTER - Tier 2", "EDGECREEP EAST", "EDGECREEP WEST", "EDGEPIERCE EAST", "EDGEPIERCE WEST"], true);
            AddRoom("VAULT", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - RARE G", "CENTER - Tier 2 G", "EDGECREEP - RARE G", "EDGEPIERCE - RARE G", "Center Rare G"], true);
            AddRoom("VERANDA", ["EDGE ADVANCE WESTWING - G", "EDGE ADVANCE EASTWING - G", "EDGE RETREAT WESTWING -  G", "EDGE RETREAT EASTTWING -  G"], true);
            AddRoom("VESTIBULE", ["CENTER - Tier 1 G"], false);
            AddRoom("WALK-IN CLOSET", ["FRONTBACK G - RARE", "NORTH PIERCE G", "CORNER - Tier 1 G", "CENTER - Tier 2 G", "EDGECREEP - RARE G", "EDGEPIERCE G", "Center Rare G"], true);
            AddRoom("WEIGHT ROOM", ["CENTER - Tier 3", "Center Rare"], true);
            AddRoom("WEST WING HALL", ["EDGECREEP WEST", "EDGEPIERCE WEST"], true);
            AddRoom("WINE CELLAR", ["FRONT - Tier 1", "FRONTBACK - RARE", "NORTH PIERCE", "CORNER - RARE", "CENTER - Tier 1", "EDGECREEP - RARE", "EDGEPIERCE - RARE"], true);
            AddRoom("WORKSHOP", ["FRONT - Tier 1", "FRONTBACK - RARE", "CENTER - Tier 2", "EDGECREEP - RARE", "Center Rare"], true);
            AddRoom("ANTECHAMBER", [], true, false);
            AddRoom("ROOM 46", [], true, false);
            AddRoom("ENTRANCE HALL", [], true, false);
        }

    }
}

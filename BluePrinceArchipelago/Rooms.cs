using BluePrinceArchipelago.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using BluePrinceArchipelago.RoomHandlers;

namespace BluePrinceArchipelago.Core
{
    public class ModRoomManager {
        private List<ModRoom> _Rooms = [];
        public List<ModRoom> Rooms {
            get { return _Rooms; }
            set { _Rooms = value; }
        }
        public ModRoom ForcedRoom = null;
        public bool IsForcingDraft = false;

        public static List<string> VanillaRooms = [];
        public static List<string> CantCopy = ["ANTECHAMBER", "ENTERANCE HALL", "ROOM 46", "FOUNDATION", ""];

        public static List<string> CurrentPickerArrays = [];

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
                Logging.Log("Room already in Pool, adding more to the pool");
                _Rooms[counter].RoomPoolCount++;
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
        public void ForceDraft(string roomname) {
            ModRoom room = GetRoomByName(roomname);
            if (room != null)
            {
                ForceDraft(room);
                return;
            }
            Logging.LogWarning($"Error forcing room unable to find the room: {roomname}");
        }
        public void ForceDraft(ModRoom room) {
            if (room != null) { 
                ForceRoomQueue.Add(room);
                return;
            }
            Logging.LogWarning("Error forcing room, room can't be null");
        }
        public void SetAllVanilla() {
            foreach (ModRoom room in _Rooms) {
                room.UseVanilla = true;
                VanillaRooms.Add(room.Name);
            }
        }

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

        public void RemoveRoom(ModRoom room)
        {
            if (room.RoomPoolCount > 0) {
                room.RoomPoolCount -= 1;
            }
            room.IsUnlocked = false;
        }
        // Updates the count of how many of each room is in the house.
        public void UpdateRoomsInHouse()
        {
            PlayMakerArrayListProxy rooms = ModInstance.RoomsInHouse?.GetComponent<PlayMakerArrayListProxy>();
            if (rooms != null && rooms.arrayList.Count > 0)
            {
                foreach (GameObject room in rooms.arrayList)
                {
                    ModRoom modRoom = GetRoomByName(room.name);
                    if (modRoom != null)
                    {
                        modRoom.RoomInHouseCount++;
                    }
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

        // Returns the ModRoom object by it's name.
        public ModRoom GetRoomByName(string name)
        {
            foreach (ModRoom room in _Rooms) {
                if (room.Name.ToUpper().Trim() == name.ToUpper().Trim()) {
                    return room; 
                }
            }
                    return null;
            }

            /// <summary>
            /// Adds a room with the same name for both the room and its game object path.
            /// </summary>
            public ModRoom AddRoom(string name, List<string> pickerArrays, bool isUnlocked, bool useVanilla = false, bool hasBeenDrafted = false) {
                return AddRoom(name, name, pickerArrays, isUnlocked, useVanilla, hasBeenDrafted);
            }

            /// <summary>
            /// Adds a room with a separate game object name (for special cases like classroom variants).
            /// </summary>
            /// <param name="name">The name used internally by the mod (e.g., "CLASSROOM (1)")</param>
            /// <param name="gameObjectName">The actual name of the game object in Room Engines (e.g., "CLASSROOM")</param>
            public ModRoom AddRoom(string name, string gameObjectName, List<string> pickerArrays, bool isUnlocked, bool useVanilla = false, bool hasBeenDrafted = false) {
                string roomPath = "__SYSTEM/The Room Engines/" + gameObjectName;
                GameObject roomObj = GameObject.Find(roomPath);
                if (roomObj == null)
                {
                    Logging.LogWarning($"Could not find room GameObject at '{roomPath}' for room '{name}'");
                }
                return AddRoom(new ModRoom(name, gameObjectName, roomObj, pickerArrays, isUnlocked, useVanilla, hasBeenDrafted));
            }

            public void UpdateRoomPools() {
            UpdateRoomsInHouse();
            Logging.Log("Updating Room Pools");
            foreach (ModRoom room in _Rooms) {
                room.UpdatePools();
            }
        }
        public void EmptyDraftPool()
        {
            foreach (ModRoom room in _Rooms) {
                room.IsUnlocked = false || room.UseVanilla; //Set the room to not unlocked, unless the room is set to use Vanilla Handling.
            }
        }

        /// <summary>
        /// Clears the entire draft pool for Archipelago mode - locks ALL rooms regardless of vanilla status.
        /// Use this when syncing with Archipelago to start fresh.
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
        /// Checks if an item name corresponds to a room, including special mappings.
        /// Returns true if the item is a room, false otherwise.
        /// </summary>
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
        /// Unlocks a specific room by name for Archipelago mode.
        /// Handles special cases like classroom variants (e.g., "Classroom 2" → "CLASSROOM (2)").
        /// </summary>
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
            // Currently no special mappings needed
            // Classroom items now come as just "Classroom" which matches "CLASSROOM" directly

            // Add other special mappings here as needed in the future

            return null;
        }

        public void UpdateCurrentPickerArrays() {
            PlayMakerFSM grid = ModInstance.TheGrid;
            PlayMakerFSM planPicker = grid.GetGameObjectVariable("theplanpick").value?.GetComponent<PlayMakerFSM>();
            CurrentPickerArrays.Clear();
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
                            CurrentPickerArrays.Add(action.setValue.Value.name);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents a room in the draft pool with all its state.
    /// </summary>
    /// <param name="name">Internal name for the mod (e.g., "CLASSROOM (1)")</param>
    /// <param name="gameObjectName">Actual game object name in Room Engines (e.g., "CLASSROOM")</param>
    /// <param name="gameObject">The Unity GameObject for this room</param>
    /// <param name="pickerArrays">List of picker arrays this room can appear in</param>
    /// <param name="isUnlocked">Whether the room is initially unlocked</param>
    /// <param name="useVanilla">Whether to use vanilla handling for this room</param>
    /// <param name="hasBeenDrafted">Whether this room has been drafted this run</param>
    public class ModRoom(String name, String gameObjectName, GameObject gameObject, List<string> pickerArrays, bool isUnlocked, bool useVanilla = false, bool hasBeenDrafted = false)
    {
        private string _Name = name;
        public string Name { get { return _Name; } set { _Name = value; } }

        // The actual game object name used in "__SYSTEM/The Room Engines/"
        private string _GameObjectName = gameObjectName;
        public string GameObjectName { get { return _GameObjectName; } set { _GameObjectName = value; } }

        private GameObject _GameObj = gameObject;
        public GameObject GameObj { get { return _GameObj; } set { _GameObj = value; } }

        private List<string> _PickerArrays = pickerArrays;
        public List<string> PickerArrays { get { return _PickerArrays; } set { _PickerArrays = value; } }

        private RoomHandler _Handler = RoomHandler.CreateRoomHandler(name);
        public RoomHandler Handler { get { return _Handler; } set { _Handler = value; } }

        private bool _IsUnlocked = isUnlocked;

        public List<Func<ModRoom,bool>> Dependencies = new List<Func<ModRoom, bool>>();

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
                            Logging.LogDebug($"Room '{_Name}' (GO: {_GameObjectName}) POOL REMOVAL set to {!value} (IsUnlocked={value})");
                        }
                        else
                        {
                            Logging.LogWarning($"Room '{_Name}' (GO: {_GameObjectName}): Could not find 'POOL REMOVAL' variable in FSM");
                        }
                    }
                    else
                    {
                        Logging.LogWarning($"Room '{_Name}' (GO: {_GameObjectName}): Could not find FSM named '{_GameObjectName}'");
                    }
                }
                else
                {
                    // This is expected if scene isn't loaded yet
                    Logging.LogDebug($"Room '{_Name}': Room engine not found at '{roomPath}' (scene may not be loaded)");
                }
                _IsUnlocked = value;
            }
        }

        // Stores if the room has been drafted for tracking checks.
        private bool _HasBeenDrafted = hasBeenDrafted;
        public bool HasBeenDrafted { 
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
                else if (value > 1 && (ModRoomManager.CantCopy.Contains(_Name)))
                {
                    Logging.LogWarning($"Cannot have more than 1 copy of the {_Name}, it will break your save file/run.");
                }
                else {
                    _RoomPoolCount = value;
                }
            }
        }

        // tracks how many copies of the room are in the house.
        private int _RoomInHouseCount = 0;

        public int RoomInHouseCount {
            get { return _RoomInHouseCount;} 
            set { _RoomInHouseCount = value; }
        }

        public int RoomsLeftInPool {
            get { 
                int left = _RoomPoolCount - RoomInHouseCount;
                return left > 0 ? left : 0; // Ensure we never return negative
            }
        }

        //Adds a copy(s) of this room to the pool array
        private void AddToPool(PlayMakerArrayListProxy array, int count = 1) {
            // Ensure we have a valid GameObject to add
            if (_GameObj == null)
            {
                // Try to get the GameObject from the Room Engines using the game object name
                _GameObj = GameObject.Find("__SYSTEM/The Room Engines/" + _GameObjectName);
                if (_GameObj == null)
                {
                    Logging.LogWarning($"Cannot add {_Name} to pool: GameObject is null (looked for '{_GameObjectName}')");
                    return;
                }
            }
            //Checks dependencies of the room before adding it to the draft pool.
            foreach (Func<ModRoom, bool> dependency in Dependencies) {
                if (!dependency(this)) {
                    Logging.LogWarning($"Cannot add {_Name} to pool: Dependency not met");
                    return;
                }
            }

            for (int i = 0; i < count; i++)
            {
                array.Add(_GameObj, "GameObject");
                Logging.Log($"Added {_Name} (GO: {_GameObjectName}) to {array.name}");
            }
        }
        //Removes copy(s) of this room from the pool array
        private void RemoveFromPool(PlayMakerArrayListProxy array, int count = 1) {
            if (_GameObj == null)
            {
                Logging.LogWarning($"Cannot remove {_Name} from pool: GameObject is null");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                if (array.Contains(_GameObj))
                {
                    array.Remove(_GameObj, "GameObject");
                    Logging.Log($"Removed {_Name} from {array.name}");
                }
                else {
                    Logging.Log($"{_Name} doesn't exist in the pool {array.name}");
                }
            }
        }
        //Set the FSMBools in the appropriate room to ensure that the correct rooms show up in draft.
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
        // Helper function that updates 1 array at a time.
        private void UpdateArray(PlayMakerArrayListProxy array) {
            if (RoomsLeftInPool > 0)
            {
                int count = 0;
                List<int> indexes = [];
                // Find all copies of the room currently in the list
                // Use GameObjectName for comparison since that's the actual Unity object name
                for (int i = 0; i < array.GetCount(); i++)
                {
                    GameObject room = array.arrayList[i].TryCast<GameObject>();
                    if (room != null)
                    {
                        if (room.name == _GameObjectName)
                        {
                            indexes.Insert(0, i); //add to front of list so it's in descending order.
                            count++;
                        }
                    }
                }
                // If the room has at least one copy currently in the pool
                if (count > 0 && _IsUnlocked && !_UseVanilla)
                {
                    // check if there are more copies than there should be
                    if (count > RoomsLeftInPool)
                    {
                        RemoveFromPool(array, count - RoomsLeftInPool);

                    }
                    // check if there less copies than there should be
                    else if (RoomsLeftInPool > count)
                    {
                        AddToPool(array, RoomsLeftInPool - count);
                    }
                }
                // check if there are still rooms that should be in the pool but aren't
                else if (RoomsLeftInPool > 0 && _IsUnlocked && !_UseVanilla)
                {
                    AddToPool(array, RoomsLeftInPool);
                }
                // Handle extra copies of rooms that use vanilla logic. Assume always 1 is default (no extra copies), and that the rest is extra.
                else if (_RoomPoolCount > 1 && _RoomPoolCount -1 != count && _UseVanilla && ! ModRoomManager.CantCopy.Contains(_Name)) {
                    AddToPool(array, _RoomPoolCount -1);
                }
                // If copies in pool and not set to use vanilla logic, remove from pool.
                else if (count > 0 && !_UseVanilla)
                {
                    RemoveFromPool(array, count);
                    FsmBool poolRemoval = GameObject.Find("__SYSTEM/The Room Engines/" + _GameObjectName)?.GetFsm(_GameObjectName)?.GetBoolVariable("POOL REMOVAL");
                    if (poolRemoval != null)
                    {
                        poolRemoval.Value = false;
                    } //Set the FSMBool to true so that it removes the room from the pool.
                }
            }
        }

        public bool CanDraft() {
            PlayMakerFSM grid = ModInstance.TheGrid;
            PlayMakerFSM planPicker = grid.GetGameObjectVariable("theplanpick").Value?.GetComponent<PlayMakerFSM>();
            //Check all the states for SetFsmGameObject actions. If that action is setting one of the picker Arrays, 
            if (planPicker != null) {
                foreach (FsmState state in planPicker.FsmStates) {
                    foreach (SetFsmGameObject action in state.GetActionsOfType<SetFsmGameObject>()) {
                        if (action.variableName.value.Contains("Array")) {
                            if (_PickerArrays.Contains(action.setValue.value.name)) {
                                return true;
                            }

                        }
                    }
                }


                return true;
            }
            return false;
        }

        public void UpdatePools()
        {
            foreach (string arrayName in _PickerArrays)
            {
                if (arrayName != "")
                {
                    if (ModInstance.PickerDict.ContainsKey(arrayName))
                    {
                        PlayMakerArrayListProxy array = ModInstance.PickerDict[arrayName];
                        if (array != null)
                        {
                            UpdateArray(array);
                        }
                        else
                        {
                            Logging.LogWarning($"Array '{arrayName}' is null in PickerDict for room '{_Name}'");
                        }
                    }
                    else
                    {
                        Logging.LogWarning($"Array '{arrayName}' not found in PickerDict for room '{_Name}'");
                    }
                }
            }
        }

        public void AddDependency(Func<ModRoom, bool> dependency)
        {
            Dependencies.Add(dependency);
        }
        public void AddDependencies(params Func<ModRoom, bool>[] dependencies) {
            Dependencies.AddRange(dependencies);
        }
    }
}

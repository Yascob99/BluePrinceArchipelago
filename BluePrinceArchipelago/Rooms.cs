using BluePrinceArchipelago.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace BluePrinceArchipelago.Core
{
    public class ModRoomManager {
        private List<ModRoom> _Rooms = [];
        public List<ModRoom> Rooms {
            get { return _Rooms; }
            set { _Rooms = value; }
        }

        public static List<string> VanillaRooms = [];
        public static List<string> CantCopy = ["ANTECHAMBER", "ENTERANCE HALL", "ROOM 46", "FOUNDATION"];

        public ModRoomManager() {
        }
        public void AddRoom(ModRoom room) {
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
                Plugin.BepinLogger.LogMessage("Room already in Pool, adding more to the pool");
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

        }
        // Updates the count of how many of each room is in the house.
        public void UpdateRoomsInHouse()
        {
            PlayMakerArrayListProxy rooms = ModInstance.RoomsInHouse?.GetComponent<PlayMakerArrayListProxy>();
            if (rooms.arrayList.Count > 0)
            {
                foreach (GameObject room in rooms.arrayList)
                {
                    GetRoomByName(room.name).RoomInHouseCount++;
                }
            }
        }
        // Returns the ModRoom object by it's name.
        public ModRoom GetRoomByName(string name)
        {
            foreach (ModRoom room in _Rooms) { 
                if (room.Name == name) { return room; }
            }
            return null;
        }
        public void AddRoom(string name, List<string> pickerArrays, bool isUnlocked, bool useVanilla = false, bool hasBeenDrafted = false) {
            AddRoom(new ModRoom(name, GameObject.Find("__SYSTEM/The Room Engines/" + name), pickerArrays, isUnlocked, useVanilla, hasBeenDrafted));
        }

        public void UpdateRoomPools() {
            UpdateRoomsInHouse();
            Plugin.BepinLogger.LogMessage("Updating Room Pools");
            foreach (ModRoom room in _Rooms) {
                room.UpdatePools();
            }
        }
    }
    public class ModRoom(String name, GameObject gameObject, List<string> pickerArrays, bool isUnlocked, bool useVanilla = false, bool hasBeenDrafted = false)
    {
        private string _Name = name;
        public string Name { get { return _Name; } set { _Name = value; } }

        private GameObject _GameObj = gameObject;
        public GameObject GameObj { get { return _GameObj; } set { _GameObj = value; } }

        private List<string> _PickerArrays = pickerArrays;
        public List<string> PickerArrays { get { return _PickerArrays; } set { _PickerArrays = value; } }

        private bool _IsUnlocked = isUnlocked;
        public bool IsUnlocked {
            get { return _IsUnlocked; }
            set {
                if (value)
                {
                    GameObject.Find("__SYSTEM/The Room Engines/" + _Name)?.GetFsm(_Name)?.GetBoolVariable("POOL REMOVAL")?.Value = false;
                }
                else {
                    GameObject.Find("__SYSTEM/The Room Engines/" + _Name)?.GetFsm(_Name)?.GetBoolVariable("POOL REMOVAL")?.Value = false; //make it unavailabe for draft if it's not unlocked.
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
                        ModInstance.Instance.ModEventHandler.OnFirstDrafted(this);
                        _HasBeenDrafted = value;
                }
                // No changes to value once the room has been drafted once, or if someone is not trying to set this to true for some stupid reason.
            }     
        }


        // For handling special rooms. Defaults to things that are not in the randomizable pool.
        private bool _UseVanilla = !useVanilla;

        public bool UseVanilla { get {return _UseVanilla;} set { _UseVanilla = value; } }


        // The number of this room that can bein the pool
        private int _RoomPoolCount = 1;
        public int RoomPoolCount 
        {
            get { return _RoomPoolCount; }
            set {
                if (value < 0)
                {
                    _RoomPoolCount = 0;
                    Plugin.BepinLogger.LogWarning("Cannot set roomcount to below 0");
                }
                else if (value == 1 && (ModRoomManager.CantCopy.Contains(_Name)) ) {
                    Plugin.BepinLogger.LogWarning($"Cannot have more than 1 copy of the {_Name}, it will break your save file/run.");
                }
            }
        }

        // tracks how many copies of the
        private int _RoomInHouseCount = 0;

        public int RoomInHouseCount {
            get { return _RoomInHouseCount;} 
            set { _RoomInHouseCount = value; }
        }

        public int RoomsLeftInPool {
            get { 
                return _RoomPoolCount - RoomInHouseCount; 
                }
        }

        //Adds a copy(s) of this room to the pool array
        private void AddToPool(PlayMakerArrayListProxy array, int count) {
            for (int i = 0; i < count; i++)
            {
                array.Add(_GameObj, "GameObject");
                Plugin.BepinLogger.LogMessage($"Added {_Name} to {array.name}");
            }
        }
        //Removes copy(s) of this room from the pool array
        private void RemoveFromPool(PlayMakerArrayListProxy array, int count) {
            for (int i = 0; i < count; i++)
            {
                if (array.Contains(_GameObj))
                {
                    array.Remove(_GameObj, "GameObject");
                    Plugin.BepinLogger.LogMessage($"Removed {_Name} from {array.name}");
                }
                else {
                    Plugin.BepinLogger.LogMessage($"{_Name} doesn't exist in the pool {array.name}");
                }
            }
        }
        //Adds copy(s) of this room to all of it's listed pool arrays
        private void AddToPools(int count = 1)
        {
            foreach (string arrayName in _PickerArrays)
            {
                PlayMakerArrayListProxy array = ModInstance.PickerDict[arrayName];
                AddToPool(array, count);
            }
        }
        //Removes copy(s) of this room from all of it's listed pool arrays 
        private void RemoveFromPools(int count = 1)
        {
            foreach (string arrayName in _PickerArrays)
            {
                PlayMakerArrayListProxy array = ModInstance.PickerDict[arrayName];
                RemoveFromPool(array, count);
            }
        }
        //Set the FSMBools in the appropriate room to ensure that the correct rooms show up in draft.
        public void Initialize()
        {
            if (!IsUnlocked)
            {
                GameObject.Find("__SYSTEM/The Room Engines/" + _Name)?.GetFsm(_Name)?.GetBoolVariable("POOL REMOVAL")?.Value = false;
            }
            else
            {
                GameObject.Find("__SYSTEM/The Room Engines/" + _Name)?.GetFsm(_Name)?.GetBoolVariable("POOL REMOVAL")?.Value = false;
            }
        }
        // Helper function that updates 1 array at a time.
        private void UpdateArray(PlayMakerArrayListProxy array) {
            if (RoomsLeftInPool > 0)
            {
                int count = 0;
                List<int> indexes = [];
                // Find all copies of the room currently in the list
                for (int i = 0; i < array.GetCount(); i++)
                {
                    GameObject room = array.arrayList[i].TryCast<GameObject>();
                    if (room != null)
                    {
                        if (room.name == _Name)
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
                // If the room is in the pool and shouldn't be remove it if it isn't using vanilla logic.
                else if (count > 0 && !_UseVanilla)
                {
                    RemoveFromPool(array, count);
                    GameObject.Find("__SYSTEM/The Room Engines/" + _Name)?.GetFsm(_Name)?.GetBoolVariable("POOL REMOVAL")?.Value = false; //Set the FSMBool to true so that it removes the room from the pool.
                }
            }
        }

        public void UpdatePools() {
            foreach (string arrayName in _PickerArrays) {
                if (arrayName != "")
                {
                    PlayMakerArrayListProxy array = ModInstance.PickerDict[arrayName];
                    UpdateArray(array);
                }
            }
        }
    }

}

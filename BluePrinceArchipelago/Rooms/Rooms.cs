using BluePrinceArchipelago.Archipelago;
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
                var beforeDraftCount = ModRoomManager.GetRoomByName("CLASSROOM").RoomInHouseCount;
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

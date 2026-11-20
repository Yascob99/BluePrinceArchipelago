using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Rooms;
using BluePrinceArchipelago.Utils;
using UnityEngine;
#if Bep
using PathologicalGames;
#endif
#if ML
using Il2CppPathologicalGames;
#endif
using System.Collections.Generic;


namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     The manager that tracks how many of which trunks have been opened.
    /// </summary>
    public class TrunkManager
    {
        private Dictionary<string, int> _TrunkCounts = [];
        private Dictionary<string, int> _MaxTrunkCounts = [];
        public Dictionary<string, int> TrunkCounts { 
            get { return _TrunkCounts; }
            set { _TrunkCounts = value; }
        }
        public Dictionary<string, int> MaxTrunkCounts
        {
            get { return _MaxTrunkCounts; }
            set { _MaxTrunkCounts = value; }
        }
        public Dictionary<string, bool> TrunkHooked { get; set; } = [];

        // Gets set to true if a trunk was spawned in the timing between the room being spawned and item code finishing it's execution.
        public bool TrunkSpawned = false;

        public bool CheckingForTrunkSpawn = false;

        public int TrunkChance { get; set; } = 0;

        public TrunkManager() 
        {
        }

        /// <summary>
        ///     Pulls from the saved state to initialize the current trunk counts.
        /// </summary>
        public void Initialize() {
            if (ArchipelagoClient.Authenticated) {
                State.InitializeTrunkCounts();
                return;
            }
        }

        /// <summary>
        ///     When a trunk is opened.
        /// </summary>
        public void OnTrunkOpen() {
            string currentRoom = ModInstance.TheGrid.GetStringVariable("CURRENT ROOM").Value;
            currentRoom = ModRoomManager.GetRoomByName(currentRoom).Name;
            if (_TrunkCounts.ContainsKey(currentRoom))
            {
                _TrunkCounts[currentRoom]++;
                ModInstance.ModEventHandler.OnTrunkOpened(currentRoom, _TrunkCounts[currentRoom]);
                State.UpdateTrunkCounts();
            }
        }

        public void TrySpawnTrunk(ModRoom room) {
            CheckingForTrunkSpawn = false; //Set it to false, just in case.
            Logging.Log($"Attempting to Spawn Trunk in {room.Name}.\n\tTrunk Spawned: {TrunkSpawned}\n\tTrunk Location Count: {_TrunkCounts.ContainsKey(room.Name)}\n\tTrunk Location Max: {_MaxTrunkCounts.ContainsKey(room.Name)}", "Trunks");
            // Check if a trunk was spawned by the item code. If not attempt a trunk spawn.
            if (_TrunkCounts.ContainsKey(room.Name) && _MaxTrunkCounts.ContainsKey(room.Name) && !TrunkSpawned) {
                Logging.Log($"Room has trunk locations.\n\tCurrent Trunks: {_TrunkCounts[room.Name]}\n\tMax Trunks: {_MaxTrunkCounts[room.Name]}", "Trunks");
                if (_TrunkCounts[room.Name] <= _MaxTrunkCounts[room.Name]) {

                    // Check if the player rolled a trunk on the additional trunk chance.
                    System.Random rng = new();
                   
                    int TrunkRNG = rng.Next(100) + 1;
                    Logging.Log($"Rolling to spawn trunk with {TrunkChance} and Rolled RNG of {TrunkRNG}", "Trunks");
                    if (TrunkChance >= TrunkRNG)
                    {
                        if (TryInstantiateTrunk(room))
                        {
                            Logging.Log($"Trunk Spawned Successfully in {room.Name}", "Trunks");
                        }
                    }
                }
            }
            TrunkSpawned = false;
        }

        private bool TryInstantiateTrunk(ModRoom room) {
            GameObject roomGameObject = ModRoomManager.GetRoomInstance(room.Name.ToTitleCase());
            if (roomGameObject != null) {
                List<GameObject> TrunkSpawns = roomGameObject.FindChildGameObjectsRecursive("TRUNK SPWN", true);
                // For now only attempt to spawn trunks in rooms that have 1 definitive spawn. Later will need special handling for rooms with more than one spawn.
                if (TrunkSpawns.Count == 1) {
                    
                    Transform TrunkPrefab = GetTrunkPrefab();
                    if (TrunkPrefab != null) {
                        // Use the PoolManager to spawn the Trunk.
                        PoolManager.Pools["Pickup"].Spawn(TrunkPrefab, TrunkSpawns[0].transform.position, TrunkSpawns[0].transform.rotation);
                    }
                    return true;
                }
            }
            return false;
        }

        public static Transform GetTrunkPrefab() { 
            SpawnPool ItemSpawnPool = GameObjectExtensions.FindInHierarchy("__SYSTEM/Pickup Spawn Pools").GetComponent<SpawnPool>();
            return ItemSpawnPool?.prefabs["TRUNK"] ?? null;
        }
    }
}

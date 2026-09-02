using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Utils;
using System;
using System.Collections.Generic;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     The manager that tracks how many of which trunks have been opened.
    /// </summary>
    public class TrunkManager
    {
        private Dictionary<string, int> _TrunkCounts = new Dictionary<string, int>();
        public Dictionary<string, int> TrunkCounts { 
            get { return _TrunkCounts; }
            set { _TrunkCounts = value; }
        }
        public TrunkManager() 
        {
        }

        /// <summary>
        ///     Pulls from the saved state to initialize the current trunk counts.
        /// </summary>
        public void Initialize() {
            if (ModInstance.IsArchipelagoMode) {
                State.InitializeTrunkCounts();
                return;
            }
        }

        /// <summary>
        ///     When a trunk is opened.
        /// </summary>
        public void OnTrunkOpen() {
            string currentRoom = ModInstance.TheGrid.GetStringVariable("CURRENT ROOM").ToString();
            if (!_TrunkCounts.ContainsKey(currentRoom))
            {
                _TrunkCounts.Add(currentRoom, 1);
            }
            else {
                _TrunkCounts[currentRoom]++;
            }
            ModInstance.ModEventHandler.OnTrunkOpened(currentRoom, _TrunkCounts[currentRoom]);
            State.UpdateTrunkCounts();
        }
    }
}

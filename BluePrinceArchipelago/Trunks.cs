using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Utils;
using System;
using System.Collections.Generic;

namespace BluePrinceArchipelago.Core
{
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

        public void Initialize() {
            if (ModInstance.IsArchipelagoMode) {
                State.InitializeTrunkCounts();
                return;
            }
        }
        public void OnTrunkOpen() {
            string currentRoom = ModInstance.TheGrid.GetStringVariable("CURRENT ROOM").ToString();
            if (!_TrunkCounts.ContainsKey(currentRoom))
            {
                _TrunkCounts.Add(currentRoom, 1);
            }
            else {
                _TrunkCounts[currentRoom]++;
            }
            ModInstance.Instance.ModEventHandler.OnTrunkOpened(currentRoom, _TrunkCounts[currentRoom]);
            State.UpdateTrunkCounts();
        }
    }
}

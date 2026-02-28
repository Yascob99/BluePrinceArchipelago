using BluePrinceArchipelago.Utils;

namespace BluePrinceArchipelago.COre
{
    public class TrunkTracker
    {
        private int _TrunkCount = 0;
        public int TrunkCount { 
            get { return _TrunkCount; }
        }
        public TrunkTracker() 
        {
        }
        public void Initialize() {
            if (State.ContainsKey("TrunkCount") && ModInstance.IsArchipelagoMode) {
                _TrunkCount = State.GetData<int>("TrunkCount");
                return;
            }
            _TrunkCount = 0;
        }
        public void OnTrunkOpen() { 
            _TrunkCount++;
        }
    }
}

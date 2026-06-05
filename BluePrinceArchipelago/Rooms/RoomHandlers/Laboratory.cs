using BluePrinceArchipelago.Items;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers
{
    public class Laboratory : RoomHandler
    {
        public Laboratory()
        {
            Logging.Log("Initializing Laboratory.");
        }

        public override void OnRoomDrafted(GameObject roomGameObject)
        {
            RoomGameObject = roomGameObject;
            
        }

        public override void OnAfterRoomDrafted()
        {
            // Runs the code to prevent Gemstone Cavern from being unlocked normally.
            Unlocks.BlackBridgeGrotto.PreventDefault();
        }
    }
}

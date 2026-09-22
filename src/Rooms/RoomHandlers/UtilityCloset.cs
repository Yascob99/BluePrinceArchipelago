using BluePrinceArchipelago.Items;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers
{
    public class UtilityCloset : RoomHandler
    {
        public UtilityCloset()
        {
        }

        public override void OnRoomDrafted(GameObject roomGameObject)
        {
            RoomGameObject = roomGameObject;
        }

        public override void OnAfterRoomDrafted(GameObject roomGameObject)
        {
            Unlocks.GemstoneCaverns.PreventDefault();
        }
    }
}

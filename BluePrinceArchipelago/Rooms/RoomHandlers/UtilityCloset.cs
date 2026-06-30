using BluePrinceArchipelago.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

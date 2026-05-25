using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BluePrinceArchipelago.RoomHandlers
{
    public class Study : RoomHandler
    {

        public Study()
        {
            Logging.Log("Initializing Study.");
        }

        public override void OnRoomDrafted(GameObject roomGameObject)
        {
            throw new System.NotImplementedException();
        }
    }
}

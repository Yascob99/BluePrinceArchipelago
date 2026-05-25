using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BluePrinceArchipelago.RoomHandlers
{
    public class Observatory : RoomHandler
    {

        public Observatory()
        {
            Logging.Log("Initializing Observatory.");
        }

        public override void OnRoomDrafted(GameObject roomGameObject)
        {
            throw new System.NotImplementedException();
        }
    }
}

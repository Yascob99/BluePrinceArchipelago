
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;

namespace BluePrinceArchipelago.RoomHandlers;

public class EntranceHall : RoomHandler
{
    // Vase 2 = West
    // Vase 1 = East
    public EntranceHall()
    {
        ObservedFSMStates.Add("Vase 1", ["BREAK!"]);
        ObservedFSMStates.Add("Vase 2", ["BREAK!"]);
    }

    public override void OnFSMStateChanged(Fsm fsm, string gameObjectName, string newState)
    {
        if (newState == "BREAK!")
        {
            if (gameObjectName == "Vase 1")
            {
                Logging.Log("Vase 1 broken in Entrance Hall.", "Entrance Hall");
                ModInstance.ModEventHandler.OnVaseBroken("Entrance Hall East");
            }
            else if (gameObjectName == "Vase 2")
            {
                Logging.Log("Vase 2 broken in Entrance Hall.", "Entrance Hall");
                ModInstance.ModEventHandler.OnVaseBroken("Entrance Hall West");
            }
        }
    }

    public override void OnRoomDrafted(GameObject roomGameObject) // This is still used when drafting this room in the outer room, which is needed for the allowance token check
    {
        RoomGameObject = roomGameObject;
    }
}
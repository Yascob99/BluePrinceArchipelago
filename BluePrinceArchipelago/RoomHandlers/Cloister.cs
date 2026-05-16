
using System.Linq;
using HutongGames.PlayMaker;
using UnityEngine;

namespace BluePrinceArchipelago.RoomHandlers;

class Cloister : RoomHandler
{
    private static bool _collected = false;
    public Cloister()
    {
        ObservedFSMStates.Add("ALLOWANCE TOKEN", ["State"]); // The FSM state name is empty for some reason
    }

    public override void OnFSMStateChanged(Fsm fsm, string gameObjectName, string newState)
    {
        if (_collected) return;
        var gameObject = fsm.GameObject;
        while (gameObject.name.ToUpper() == "ALLOWANCE TOKEN") // several objects called allowance token are nested
        {
            gameObject = gameObject.transform.parent.gameObject;
        }

        var parent = gameObject.transform.parent.parent.gameObject;

        if (parent.name.ToUpper().Contains("CLOISTER") && newState == "State")
        {
            Logging.Log("Allowance Token state changed in Cloister.", "Cloister");
            ModInstance.ModEventHandler.OnAllowanceCollected("Cloister Statue");
            _collected = true;
        }
    }

    public override void OnRoomDrafted(GameObject roomGameObject)
    {
        RoomGameObject = roomGameObject;
    }
}
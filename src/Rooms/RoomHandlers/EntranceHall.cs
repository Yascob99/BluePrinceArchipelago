
using System.Collections.Generic;
using BluePrinceArchipelago.FsmMethods;
using BluePrinceArchipelago.Utils;

#if Bep
using HutongGames.PlayMaker;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
#endif
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

public class EntranceHall : RoomHandler
{
    // Vase 2 = West
    // Vase 1 = East
    public EntranceHall()
    {
        AllowanceTokens.Add("Entrance Hall");
    }

    public override void OnRoomDrafted(GameObject roomGameObject) // This is still used when drafting this room in the outer room, which is needed for the allowance token check
    {
        roomGameObject = ModRoomManager.GetRoomInstance("Entrance Hall");
        PlayMakerFSM Vase1 = roomGameObject.transform.Find("VASES").Find("Vase 1").gameObject.GetComponent<PlayMakerFSM>();
        Vase1.GetState("BREAK!").AddAction(CustomFsmMethodManager.GetCallMethod("EntranceHallVaseBrokenEast"));
        PlayMakerFSM Vase2 = roomGameObject.transform.Find("VASES").Find("Vase 2").gameObject.GetComponent<PlayMakerFSM>();
        Vase2.GetState("BREAK!").AddAction(CustomFsmMethodManager.GetCallMethod("EntranceHallVaseBroken"));
    }

    public override void OnAllowanceTokenCollected(string token)
    {
         ModInstance.ModEventHandler.OnAllowanceCollected("Outer Entrance Hall Vase");
    }
}
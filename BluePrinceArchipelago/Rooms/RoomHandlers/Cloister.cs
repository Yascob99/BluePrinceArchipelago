
using System.Linq;
using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class Cloister : RoomHandler
{
    public Cloister()
    {
        AllowanceTokens.Add("Cloister");
    }
    public override void OnAllowanceTokenCollected()
    {
        ModInstance.ModEventHandler.OnAllowanceCollected("Cloister Statue");
    }
}
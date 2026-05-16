
using System.Linq;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace BluePrinceArchipelago.RoomHandlers;

public class Basement : RoomHandler
{
    public Basement()
    {
        ObservedFSMStates.Add("Basement Door 2", ["Open Door"]);
        ObservedFSMStates.Add("Basement Door 1", ["Open Door"]);
    }
    
    public override void OnFSMStateChanged(Fsm fsm, string gameObjectName, string newState)
    {
        if (newState == "Open Door")
        {
            var doorNum = fsm.GetState("Open Door").GetActionsOfType<SetFsmBool>().FirstOrDefault()?.variableName.value;

            if (doorNum != "Basement Door 1") return;

            ModInstance.ModEventHandler.OnUnlockBasementDoor("The Foundation");
        }
    }
}
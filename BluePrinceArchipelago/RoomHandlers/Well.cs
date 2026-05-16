
using System.Linq;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace BluePrinceArchipelago.RoomHandlers;

public class Well : RoomHandler
{
    public Well()
    {
        ObservedFSMStates.Add("Basement Door 2", ["Open Door"]);
        ObservedFSMStates.Add("Basement Door 1", ["Open Door"]);
    }

    public override void OnFSMStateChanged(Fsm fsm, string gameObjectName, string newState)
    {
        if (newState == "Open Door")
        {
            var doorNum = fsm.GetState("Open Door").GetActionsOfType<SetFsmBool>().FirstOrDefault()?.variableName.value;

            if (doorNum != "Basement Door 2") return;

            ModInstance.ModEventHandler.OnUnlockBasementDoor("The Foundation");
        }
    }
}
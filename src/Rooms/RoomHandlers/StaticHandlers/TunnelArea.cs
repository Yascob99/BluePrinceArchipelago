#if Bep
using HutongGames.PlayMaker;
#endif
#if ML
using Il2CppHutongGames.PlayMaker;
#endif

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

public class TunnelArea : RoomHandler
{
    public TunnelArea()
    {
        ObservedFSMStates.Add("Basement Door 3", ["Open Door"]);
    }

    public override void OnFSMStateChanged(Fsm fsm, string gameObjectName, string newState)
    {
        Logging.LogWarning("Tunnel Area FSM hooks are not implemented yet.", "Tunnel Area");
    }
}
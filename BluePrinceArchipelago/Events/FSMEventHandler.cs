using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;

namespace BluePrinceArchipelago.Events
{
    public static class FSMEventHandler
    {
        public static Dictionary<string, RegisteredFSMEvent> RegisteredEvents = new();

        public static void RegisterEvents() {
            RegisteredEvents["Apple Orchard Unlock"] = new AppleOrchardUnlock();
            RegisteredEvents["West Gate Path Unlock"] = new WestGatePathUnlock();
            RegisteredEvents["Blackbridge Grotto Unlock"] = new BlackBridgeGrotto();

            foreach (var REvent in RegisteredEvents){
                REvent.Value.OnRegister();
            }
        }
    }
    public abstract class RegisteredFSMEvent {

        public string Name { get; set; }
        public SendEvent Event {  get; set; }
        public abstract void OnTrigger();

        public abstract void OnRegister();
        public RegisteredFSMEvent() {
            ModInstance.APEventFSM.AddState(Name);
            ModInstance.APEventFSM.AddGlobalTransition(Name, Name);
            // Creates a new SendEvent instance that can be called by other FSMs to communicate important events to the mod (albeit a little jankily).
            Event = new SendEvent()
            {
                eventTarget = new FsmEventTarget()
                {
                    target = FsmEventTarget.EventTarget.GameObject,
                    gameObject = new FsmOwnerDefault()
                    {
                        gameObject = Plugin.ModObject,
                        ownerOption = OwnerDefaultOption.SpecifyGameObject
                    },
                    fsmName = "FSM",
                    sendToChildren = false,
                    excludeSelf = false
                },
                sendEvent = Plugin.ModObject.GetComponent<PlayMakerFSM>().GetGlobalTransition(Name).FsmEvent,
                everyFrame = false,
                delay = 0f
            };
        }
    }
    public class AppleOrchardUnlock : RegisteredFSMEvent {

        public new string Name = "Apple Orchard Unlock";

        public override void OnRegister()
        {
            
        }

        public override void OnTrigger()
        {
            PermanentUnlocks.Unlocks.AppleOrchard.FoundLocation();
        }
    }
    public class WestGatePathUnlock : RegisteredFSMEvent
    {

        public new string Name = "West Gate Path Unlock";

        public override void OnRegister()
        {
        }

        public override void OnTrigger()
        {
            PermanentUnlocks.Unlocks.WestGatePath.FoundLocation();
        }
    }
    public class BlackBridgeGrotto : RegisteredFSMEvent
    {

        public new string Name = "Blackbridge Grotto Unlock";

        public override void OnRegister()
        {
        }

        public override void OnTrigger()
        {
            PermanentUnlocks.Unlocks.WestGatePath.FoundLocation();
        }
    }
}

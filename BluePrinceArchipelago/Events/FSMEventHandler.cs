using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;
using System.Xml.Linq;

namespace BluePrinceArchipelago.Events
{
    public static class FSMEventHandler
    {
        public static Dictionary<string, RegisteredFSMEvent> RegisteredEvents = new()
        {
            { "Apple Orchard Unlock", new AppleOrchardUnlock() },
            { "Blackbridge Grotto Unlock", new BlackBridgeGrotto() },
            { "West Gate Path Unlock", new WestGatePathUnlock() },
            { "Gemstone Caverns Unlock", new AppleOrchardUnlock() },
            { "Outer Draft Start", new OuterDraftStart() },
            { "Satellite Raised", new SatelliteRaised() },
        };
        public static RegisteredFSMEvent AddFSMEvent(string name, UniqueItem item) {
            RegisteredFSMEvent Event  = new ItemPickup(name, item);
            RegisteredEvents[name] = Event;

            Event.OnRegister();
            return Event;
        }

        public static void RegisterEvents() {
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
            
        }
    }
    public class AppleOrchardUnlock : RegisteredFSMEvent {

        public new string Name { get; set; } = "Apple Orchard Unlock";

        public override void OnRegister()
        {
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

        public override void OnTrigger()
        {
            Unlocks.AppleOrchard.FoundLocation();
        }
    }
    public class WestGatePathUnlock : RegisteredFSMEvent
    {

        public new string Name { get; set; } = "West Gate Path Unlock";

        public override void OnRegister()
        {
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

        public override void OnTrigger()
        {
            Unlocks.WestGatePath.FoundLocation();
        }
    }
    public class BlackBridgeGrotto : RegisteredFSMEvent
    {

        public new string Name { get; set; } = "Blackbridge Grotto Unlock";

        public override void OnRegister()
        {
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

        public override void OnTrigger()
        {
            Unlocks.WestGatePath.FoundLocation();
        }
    }
    public class SatelliteRaised : RegisteredFSMEvent
    {

        public new string Name { get; set; } = "Satellite Raised";

        public override void OnRegister()
        {
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

        public override void OnTrigger()
        {
            Unlocks.SatelliteDish.FoundLocation();
        }
    }

    public class OuterDraftStart : RegisteredFSMEvent
    {
        public new string Name { get; set; } = "Outer Draft Start";
        public override void OnRegister()
        {
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

        public override void OnTrigger()
        {
            ModInstance.OnDraftInitialize();
        }
    }
    public class ItemPickup(string name, UniqueItem item) : RegisteredFSMEvent
    {
        public new string Name { get; set; } = name;

        public UniqueItem item { get; set; } = item;

        public override void OnRegister()
        {
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

        public override void OnTrigger()
        {
            item.HasBeenFound = true;
        }
    }
}

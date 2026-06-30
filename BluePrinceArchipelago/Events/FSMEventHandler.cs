using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;

namespace BluePrinceArchipelago.Events
{
    public static class FSMEventHandler
    {
        public static Dictionary<string, RegisteredFSMEvent> RegisteredEvents = new()
        {
            { "Apple Orchard Unlock", new AppleOrchardUnlock() },
            { "Blackbridge Grotto Unlock", new BlackBridgeGrotto() },
            { "West Gate Path Unlock", new WestGatePathUnlock() },
            { "Gemstone Caverns Unlock", new GemstoneCavernsUnlock() },
            { "Outer Draft Start", new OuterDraftStart() },
            { "Satellite Raised", new SatelliteRaised() },
        };
        public static RegisteredFSMEvent AddFSMEvent(string name, UniqueItem item) {
            RegisteredFSMEvent Event  = new ItemPickup(name, item);
            RegisteredEvents[name] = Event;

            Event.OnRegister();
            return Event;
        }
        public static RegisteredFSMEvent AddBuyFSMEvent(string name, UniqueItem item) {
            RegisteredFSMEvent Event = new ItemBought(name, item);
            RegisteredEvents[name] = Event;

            Event.OnRegister();
            return Event;
        }
        public static RegisteredFSMEvent AddDigFSMEvent(string name, UniqueItem item) {
            RegisteredFSMEvent Event = new ItemDugUp(name, item);
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
    public class GemstoneCavernsUnlock : RegisteredFSMEvent
    {

        public new string Name { get; set; } = "Gemstone Caverns Unlock";

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
            Unlocks.GemstoneCaverns.FoundLocation();
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

        public UniqueItem Item { get; set; } = item ?? null;

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
            if (!Item.HasBeenFound)
            {
                Item.HasBeenFound = true;
                Plugin.ModItemManager.RemoveUniqueItemAPSwirly(Item);
                if (Item.IsCommissary)
                {
                    FsmState state = Item.CommissaryState;
                    if (state != null)
                    {
                        // If the item is not unlocked, prevent it from being added to inventory.
                        if (item.IsUnlocked && item.ApplySanity())
                        {
                            //Disable the actions that add the item to inventory.
                            state.EnableActionsOfType<ArrayListAdd>();
                            // Check if the event we are trying to remove is the custom event we added.
                            SendEvent CustomEvent = state.GetLastActionOfType<SendEvent>();
                            if (CustomEvent.sendEvent.Name.Contains("Commissary"))
                            {
                                state.RemoveFirstActionOfType<SendEvent>();
                            }
                        }
                    }
                }
                if (Item.IsDig)
                {
                    FsmState state = Item.DigState;
                    if (state != null)
                    {
                        // If the item is not unlocked, prevent it from being added to inventory.
                        if (item.IsUnlocked && item.ApplySanity())
                        {
                            //Disable the actions that add the item to inventory.
                            state.EnableActionsOfType<ArrayListAdd>();
                            SendEvent CustomEvent = state.GetLastActionOfType<SendEvent>();
                            // Check if the event we are trying to remove is the custom event we added.
                            if (CustomEvent.sendEvent.Name.Contains("Dug Up"))
                            {
                                state.RemoveFirstActionOfType<SendEvent>();
                            }
                        }
                    }
                }
                if (Item.IsLocksmith)
                {
                    FsmState state = Item.LocksmithState;
                    if (state != null)
                    {
                        // If the item is not unlocked, prevent it from being added to inventory.
                        if (item.IsUnlocked && item.ApplySanity())
                        {
                            //Disable the actions that add the item to inventory.
                            state.EnableActionsOfType<ArrayListAdd>();
                            SendEvent CustomEvent = state.GetLastActionOfType<SendEvent>();
                            // Check if the event we are trying to remove is the custom event we added.
                            if (CustomEvent.sendEvent.Name.Contains("Locksmith"))
                            {
                                state.RemoveFirstActionOfType<SendEvent>();
                            }
                        }
                    }
                }
                ModInstance.QueueManager.AddLocationToQueue($"{item.Name.ToTitleCase()} First Pickup");
            }
        }
    }
    public class ItemDugUp(string name, UniqueItem item) : RegisteredFSMEvent
    {
        public new string Name { get; set; } = name;

        public UniqueItem Item { get; set; } = item ?? null;

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
            //Handle
            if (!Item.HasBeenFound)
            {
                if (Item.ApplySanity())
                {
                    Item.HasBeenFound = true;
                    Plugin.ModItemManager.RemoveUniqueItemAPSwirly(Item);
                    ModInstance.QueueManager.AddLocationToQueue($"{Item.Name.ToTitleCase()} First Pickup");
                }
            }
        }
    }

    public class AllowanceEnvelopePickedUp(string name) : RegisteredFSMEvent
    {
        public new string Name { get; set; } = name;

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
        }
    }

    public class ItemBought(string name, UniqueItem item) : RegisteredFSMEvent {
        public new string Name { get; set; } = name;

        public UniqueItem Item { get; set; } = item ?? null;

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
            if (!Item.HasBeenFound)
            {
                Item.HasBeenFound = true;
                Plugin.ModItemManager.RemoveUniqueItemAPSwirly(Item);
                ModInstance.QueueManager.AddLocationToQueue($"{Item.Name.ToTitleCase()} First Pickup");
            }
            Item.HasBeenFound = true;
        }
    }
}

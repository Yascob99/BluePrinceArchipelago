using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Triggers;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Events
{
    /// <summary>
    ///     An EventHandler that allows the mod to track custom made events sent via an FSM.
    /// </summary>
    public static class FSMEventHandler
    {
        public static Dictionary<string, RegisteredFSMEvent> RegisteredEvents { get; set; } = new()
        {
            { "Apple Orchard Unlock", new AppleOrchardUnlock() },
            { "Blackbridge Grotto Unlock", new BlackBridgeGrotto() },
            { "West Gate Path Unlock", new WestGatePathUnlock() },
            { "Gemstone Caverns Unlock", new GemstoneCavernsUnlock() },
            { "Satellite Raised", new SatelliteRaised() },
            { "Outer Draft Reroll", new OuterDraftReroll() },
            { "Item Traded", new ItemTraded()},
            { "Sundial Scorched", new SundialScorched()},
        };

        /// <summary>
        ///     Adds an FSM event related to a Unique Item pickup.
        /// </summary>
        /// <param name="name">The name of the event.</param>
        /// <param name="item">The item of the event.</param>
        /// <returns></returns>
        public static RegisteredFSMEvent AddItemFSMEvent(string name, UniqueItem item) {
            RegisteredFSMEvent Event  = new ItemPickup(name, item);
            RegisteredEvents[name] = Event;

            Event.OnRegister();
            return Event;
        }

        /// <summary>
        ///     Adds an FSM event related to a Unique Item Purchase.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static RegisteredFSMEvent AddBuyFSMEvent(string name, UniqueItem item) {
            RegisteredFSMEvent Event = new ItemBought(name, item);
            RegisteredEvents[name] = Event;

            Event.OnRegister();
            return Event;
        }

        /// <summary>
        ///     Adds an FSM event related to a Unique Item being dug up.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static RegisteredFSMEvent AddDigFSMEvent(string name, UniqueItem item) {
            RegisteredFSMEvent Event = new ItemDugUp(name, item);
            RegisteredEvents[name] = Event;

            Event.OnRegister();
            return Event;
        }

        /// <summary>
        ///     Registers all events and triggers any OnRegister code.
        /// </summary>
        public static void RegisterEvents() {
            foreach (var REvent in RegisteredEvents){
                REvent.Value.OnRegister();
            }
        }
    }

    /// <summary>
    ///     A template for a registered event.
    /// </summary>
    public abstract class RegisteredFSMEvent {

        public string Name { get; set; }
        public SendEvent Event {  get; set; }

        /// <summary>
        ///     The code for when an event occurs.
        /// </summary>
        public abstract void OnTrigger();


        /// <summary>
        ///     The code for when an event is registered.
        /// </summary>
        public abstract void OnRegister();
        public RegisteredFSMEvent() {
            
        }
    }

    /// <summary>
    ///     A unlock event for the Apple Orchard.
    /// </summary>
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
            PermanentUnlockTriggers.OnAppleOrchardUnlock();
        }
    }

    /// <summary>
    ///     An unlock event for the Gemstone Caverns.
    /// </summary>
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

    /// <summary>
    ///     An unlock event for the WestGatePath.
    /// </summary>
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

    /// <summary>
    ///     An Unlock event for the BlackBridgeGrotto.
    /// </summary>
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
            Unlocks.BlackBridgeGrotto.FoundLocation();
        }
    }

    /// <summary>
    ///     An Unlock event for when the Satelite is raised.
    /// </summary>
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
            Logging.LogWarning("Satellite Raised");
            Unlocks.SatelliteDish.FoundLocation();
        }
    }

    /// <summary>
    ///     An event for when a Unique Item is picked up.
    /// </summary>
    /// <param name="name">The name of the event.</param>
    /// <param name="item">The item that was picked up.</param>
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
            ItemTriggers.OnAfterItemPickup(Item);
        }  
    }

    /// <summary>
    ///     An event for when a Unique Item is Dug up.
    /// </summary>
    /// <param name="name">The name of the event.</param>
    /// <param name="item">The Unique Item that was dug up.</param>
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
            ItemTriggers.OnItemDugUp(Item);
        }
    }

    /// <summary>
    ///     On the allowance token being picked up.
    /// </summary>
    /// <param name="name">The name of the event.</param>
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
            EventTriggers.OnAllowanceEnvelopePickedUp();
        }
    }

    /// <summary>
    ///     An event for when a Unique Item is bought.
    /// </summary>
    /// <param name="name">The name of the event.</param>
    /// <param name="item">The Unique Item that was bought.</param>
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
            ItemTriggers.OnItemBought(Item);
        }
    }
    public class OuterDraftReroll() : RegisteredFSMEvent
    {
        public new string Name { get; set; } = "Outer Draft Reroll";

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
            DraftTriggers.OnOuterDraftReroll();
        }
    }
    public class SundialScorched() : RegisteredFSMEvent
    {
        public new string Name { get; set; } = "Sundial Scorched";

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
            EventTriggers.OnSundailScorched();
        }
    }
    public class ItemTraded() : RegisteredFSMEvent
    {
        public new string Name { get; set; } = "Item Traded";

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
            PlayMakerFSM TradingPostMenu = GameObject.Find("UI OVERLAY CAM").transform.Find("Trading Post Menu").Find("Items PM bridge").gameObject.GetComponent<PlayMakerFSM>();
            GameObject OfferItem = TradingPostMenu.GetGameObjectVariable("Offered_item").Value;
            GameObject Icon = TradingPostMenu.GetGameObjectVariable("Icon").Value;
            ItemTriggers.OnItemTraded(OfferItem, Icon);
        }
    }
}


    


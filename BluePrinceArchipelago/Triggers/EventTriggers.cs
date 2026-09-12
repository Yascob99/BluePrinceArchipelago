using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Rooms.RoomHandlers;
using BluePrinceArchipelago.Utils;
using UnityEngine;

namespace BluePrinceArchipelago.Triggers
{
    /// <summary>
    ///     Triggers related to various game Events
    /// </summary>
    public static class EventTriggers
    {
        /// <summary>
        ///     Triggers on a mod created event Triggering.
        /// </summary>
        /// <param name="eventName">The Name of the event.</param>
        public static void ModEventTrigger(string eventName) {
            Logging.Log($"Mod Event Triggered: {eventName}", "Events");
            if (FSMEventHandler.RegisteredEvents.ContainsKey(eventName))
            {
                FSMEventHandler.RegisteredEvents[eventName].OnTrigger();
            }
            else
            {
                Logging.LogWarning($"The custom Archipelago event {eventName} doesn't appear to be registered. It is likely mispelled or not fully implemented.", "Events");
            }
        }

        /// <summary>
        ///     Triggered whenever an event it recorded by the Game's StatLogger.
        /// </summary>
        /// <param name="id">The Enum EventID of the event being recorded.</param>
        public static void OnStatsLoggerRecordEvent(EventID id)
        {
            ModEventHandler ModEventHandler = ModInstance.ModEventHandler;
            Logging.Log($"Stats being recorded for {id}.", "StatEvents");
            if (!ArchipelagoClient.Authenticated) return;

            switch (id)
            {
                case EventID.Room_46_reached:
                    if (ArchipelagoOptions.GoalType == GoalType.option_room46)
                    {
                        GoalTriggers.OnRoom46Goal();
                    }
                    else
                    {
                        ModEventHandler.OnFirstDrafted("Room 46");
                    }
                    break;
                case EventID.Antechamber_entered:
                    if (ArchipelagoOptions.GoalType == GoalType.option_antechamber)
                    {
                        GoalTriggers.OnAntechamberGoal();
                    }
                    else
                    {
                        ModEventHandler.OnFirstDrafted("Antechamber");
                    }
                    break;
                case EventID.Throne_Room_Event:
                    if (ArchipelagoOptions.GoalType == GoalType.option_ascend)
                    {
                        GoalTriggers.OnAscendGoal();
                    }
                    break;

                case EventID.Boudoir_Safe_Opened:
                    ModEventHandler.OnSafeOpened("Boudoir Safe");
                    break;
                case EventID.Drawing_Room_Safe_Opened:
                    ModEventHandler.OnSafeOpened("Drawing Room Safe");
                    break;
                case EventID.Study_Safe_Opened:
                    ModEventHandler.OnSafeOpened("Study Safe");
                    break;
                case EventID.Office_Safe_Opened:
                    ModEventHandler.OnSafeOpened("Office Safe");
                    break;
                case EventID.Drafting_Studio_Safe_Opened:
                    ModEventHandler.OnSafeOpened("Drafting Studio Safe");
                    break;
                case EventID.Shelter_Safe_Opened:
                    ModEventHandler.OnSafeOpened("Shelter Safe");
                    break;
                case EventID.Mayait_Opened:
                    ModEventHandler.OnGateOpened("Underpass Gate");
                    break;
                case EventID.Foundation_Elevator_Lowered:
                    ModEventHandler.OnLowerFoundationElevator();
                    break;
                case EventID.Basement_Puzzle_Solved:
                    ModEventHandler.OnOtherLocation("Open Basement to Reservoir Door", "Solve Basement Puzzle");
                    break;
                case EventID.Gas_Orchard:
                    ModEventHandler.OnGasValveTurned("Orchard");
                    break;
                case EventID.Gas_Gemstone:
                    ModEventHandler.OnGasValveTurned("Gemstone Cavern");
                    break;
                case EventID.Gas_Hovel:
                    ModEventHandler.OnGasValveTurned("Hovel");
                    break;
                case EventID.Gas_Schoolhouse:
                    ModEventHandler.OnGasValveTurned("Schoolhouse");
                    break;
                case EventID.Basement_Wall_Knocked:
                    ModEventHandler.OnWallBreak("Basement to Sealed");
                    break;
                case EventID.Secret_Garden_Knocked:
                    ModEventHandler.OnWallBreak("Secret Garden");
                    break;
                case EventID.Greenhouse_Knocked:
                    ModEventHandler.OnWallBreak("Greenhouse");
                    break;
                case EventID.Weight_Room_Knocked:
                    ModEventHandler.OnWallBreak("Weight Room");
                    break;
                case EventID.Cliffside_Knocked:
                    ModEventHandler.OnWallBreak("Grounds to Sealed Entrance"); // This one is probably the wall in the chess room, but I want to check if its the grounds one, since that's the one we need a hook for.
                    break;
                case EventID.Conservatory_Floorplan_Found:
                    ModEventHandler.OnFloorplanFound("Conservatory");
                    break;
                case EventID.Planetarium_Floorplan_Found:
                    ModEventHandler.OnFloorplanFound("Planetarium");
                    break;
                case EventID.Lost_and_Found_Floorplan_Found:
                    ModEventHandler.OnFloorplanFound("Lost and Found");
                    break;
                case EventID.Treasure_Trove_Floorplan_Found: // TODO: This is doesn't work in vanilla, will need to re-hook it elsewhere
                    ModEventHandler.OnFloorplanFound("Treasure Trove");
                    break;
                case EventID.Throne_Room_Floorplan_Found:
                    ModEventHandler.OnFloorplanFound("Throne Room");
                    break;
                case EventID.Mechanarium_Floorplan_Found:
                    ModEventHandler.OnFloorplanFound("Mechanarium");
                    break;
                case EventID.Tunnel_Floorplan_Found:
                    ModEventHandler.OnFloorplanFound("Tunnel");
                    break;
                case EventID.Closed_Exhibit_Floorplan_Found:
                    ModEventHandler.OnFloorplanFound("Closed Exhibit");
                    break;
                case EventID.Dovecote_Added:
                    ModEventHandler.OnFloorplanFound("Dovecote");
                    break;
                case EventID.Kennel_Added:
                    ModEventHandler.OnFloorplanFound("Kennel");
                    break;
                case EventID.Casino_Added:
                    ModEventHandler.OnFloorplanFound("Casino");
                    break;
                case EventID.Clocktower_Added:
                    ModEventHandler.OnFloorplanFound("Clock Tower");
                    break;
                case EventID.Classroom_Added:
                    ModEventHandler.OnFloorplanFound("Classroom");
                    break;
                case EventID.Solarium_Added:
                    ModEventHandler.OnFloorplanFound("Solarium");
                    break;
                case EventID.Vestibule_Added:
                    ModEventHandler.OnFloorplanFound("Vestibule");
                    break;
                case EventID.Dormitory_Added:
                    ModEventHandler.OnFloorplanFound("Dormitory");
                    break;
                case EventID.Tomb_Solved:
                    ModEventHandler.OnTombPuzzleSolved("1");
                    break;
                case EventID.Natural_Order_Opened:
                    ModEventHandler.OnTombPuzzleSolved("2");
                    break;
                case EventID.Sigil_Solved_Arch_Aries:
                case EventID.Sigil_Solved_Corarica:
                case EventID.Sigil_Solved_Eraja:
                case EventID.Sigil_Solved_Fenn_Aries:
                case EventID.Sigil_Solved_Mora_Jai:
                case EventID.Sigil_Solved_Nuance:
                case EventID.Sigil_Solved_Orinda_Aries:
                case EventID.Sigil_Solved_Verra:
                    ModEventHandler.OnSanctumSolve(id.ToString().Replace("Sigil_Solved_", "").Replace("_", " "));
                    break;
                case EventID.Torch_Chamber_Lit:
                    ModEventHandler.OnOtherLocation("Open the Torch Chamber Shortcut", "Torch Chamber Lit");
                    break;
                case EventID.Moon_Pendant_Purchased:
                    ModEventHandler.OnFirstFound(ModItemManager.GetUniqueItem("MOON PENDANT"));
                    break;
                case EventID.Master_Key_Purchased:
                    ModEventHandler.OnFirstFound(ModItemManager.GetUniqueItem("MASTER KEY"));
                    break;
                case EventID.Chronograph_Purchased:
                    ModEventHandler.OnFirstFound(ModItemManager.GetUniqueItem("CHRONOGRAPH"));
                    break;
                case EventID.Silver_Spoon_Purchased:
                    ModEventHandler.OnFirstFound(ModItemManager.GetUniqueItem("SILVER SPOON"));
                    break;
                case EventID.Emerald_Bracelet_Purchased:
                    ModEventHandler.OnFirstFound(ModItemManager.GetUniqueItem("EMERALD BRACELET"));
                    break;
                case EventID.Ornate_Compass_Purchased:
                    ModEventHandler.OnFirstFound(ModItemManager.GetUniqueItem("ORNATE COMPASS"));
                    break;
            }
        }

        /// <summary>
        ///     Triggers on the AllowanceToken Being picked up.
        /// </summary>
        /// <param name="owner">The GameObject that sent the event.</param>
        public static void AllowanceTokenPickup(GameObject owner)
        {
            bool matched = false;
            var path = owner.gameObject.GetPath();
            Logging.LogWarning($"Allowance Token Pickup event sent from {owner.gameObject.name} with path {path}", "Events");
            foreach (var roomHandler in RoomHandler.RoomHandlers.Values)
            {
                foreach (var token in roomHandler.AllowanceTokens)
                {
                    if (path.Contains(token))
                    {
                        Logging.LogWarning($"Allowance Token matched for room handler {roomHandler.GetType().Name} with token {token}", "ArchipelagoEvents");
                        roomHandler.OnAllowanceTokenCollected(token);
                        matched = true;
                    }
                }
            }
            if (!matched)
            {
                Logging.LogWarning($"No matching room handler found for Allowance Token Pickup event with path {path}.", "ArchipelagoEvents");
            }
        }

        /// <summary>
        ///     Triggers when the Sundial is Scorched.
        /// </summary>
        public static void OnSundailScorched() {
            ModInstance.ModEventHandler.OnOtherLocation("Scorch Sundial");
        }

        /// <summary>
        ///     Triggers on the Allowance token being picked up.
        /// </summary>
        public static void OnAllowanceEnvelopePickedUp() { 
            
        }
    }
}

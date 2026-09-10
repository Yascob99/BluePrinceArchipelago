using System.Collections.Generic;

namespace BluePrinceArchipelago.Archipelago.Commands
{
    /// <summary>
    ///     A Command for simulating an in game event for testing permanent unlocks.
    /// </summary>
    /// <param name="name">The name of the Command.</param>
    public class RecordEventCommand(string name) : Command(name)
    {
        public override string Description => "Records an event to set some of the vanilla states (for testing purposes).";

        public override string Syntax => "Usage:\n\t/RecordEvent <EventName>\n\nExample:\n\t/RecordEvent Orchard_Unlocked";

        public override void Run(List<string> Args)
        {
            var eventName = string.Join(" ", Args);
            if (eventName.StartsWith("\"") && eventName.EndsWith("\""))
                eventName = eventName[1..^1];

            var eventID = EventID.Null;
            switch (eventName.ToLower())
            {
                case "west_path_gate_unlocked":
                case var _ when eventName.ToLower().Contains("west") && eventName.ToLower().Contains("gate") && eventName.ToLower().Contains("unlocked"):
                    eventID = EventID.West_Path_Gate_Unlocked;
                    break;

                case "gemstone_cavern_unlocked":
                case var _ when eventName.ToLower().Contains("gemstone") && eventName.ToLower().Contains("cavern") && eventName.ToLower().Contains("unlocked"):
                    eventID = EventID.Gemstone_Cavern_Unlocked;
                    break;

                case "orchard_unlocked":
                case var _ when eventName.ToLower().Contains("orchard") && eventName.ToLower().Contains("unlocked"):
                    eventID = EventID.Orchard_Unlocked;
                    break;

                case "satellite_raised":
                case var _ when eventName.ToLower().Contains("satellite") && eventName.ToLower().Contains("raised"):
                    eventID = EventID.Satellite_Raised;
                    break;

                case "blackbridge_powered":
                case var _ when eventName.ToLower().Contains("blackbridge") && eventName.ToLower().Contains("powered"):
                    eventID = EventID.Blackbridge_Powered;
                    break;

                default:
                    ArchipelagoConsole.LogMessage($"Unknown event name: {eventName}");
                    return;
            }

            ModInstance.StatsLogger.GetComponent<StatsLogger>().Record_Event(eventID);
        }
    }
}

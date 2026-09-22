using System.Collections.Generic;

namespace BluePrinceArchipelago.Archipelago.Commands
{
    /// <summary>
    ///     A command for collecting a location from the archipelago item pool (for testing purposes). 
    /// </summary>
    /// <param name="name"></param>
    public class CollectCommand(string name) : Command(name)
    {
        public override string Description => "Collects a location from the Archipelago item pool (for testing purposes).";

        public override string Syntax => "Usage:\n\t/collect <LocationName>\n\nExample:\n\t/collect Closet First Entering";

        public override void Run(List<string> Args)
        {
            var locationName = string.Join(" ", Args);
            if (locationName.StartsWith("\"") && locationName.EndsWith("\""))
                locationName = locationName[1..^1];

            if (locationName == "goal")
            {
                Plugin.ArchipelagoClient.GoalCompleted();
                return;
            }

            if (locationName == "death")
            {
                DeathLinkHandler.ForceKillPlayer("KillPlayer called from console.");
                return;
            }

            ModInstance.ModEventHandler.OnOtherLocation(locationName);
        }
    }
}

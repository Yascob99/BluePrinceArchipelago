using BluePrinceArchipelago.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BluePrinceArchipelago.Items;

#if ML
using Il2Cpp;
#endif

namespace BluePrinceArchipelago.Archipelago.Commands
{
    /// <summary>
    ///     Adjusts various resource totals.
    /// </summary>
    /// <param name="name"></param>
    public class AdjustCommand(string name) : Command(name)
    {
        private string _Description = "Allows you to Adjust the ammount of certain run resources";
        public override string Description
        {
            get { return _Description; }
        }
        private string _Syntax = "Usage:\n\t/Adjust Gems <Adjustment_Amount>\n\t/Adjust Keys <Adjustment_Amount>\n\t/Adjust Dice <Adjustment_Amount>\n\t/Adjust Stars <Adjustment_Amount>\n\t/Adjust Steps <Adjustment_Amount>\n\t/Adjust Gold <Adjustment_Amount>\n\t/Adjust Luck <Adjustment_Amount>";
        public override string Syntax
        {
            get { return _Syntax; }
        }

        public override void Run(List<string> Args)
        {
            ArchipelagoConsole.LogMessage(string.Join(" ", Args));
            if (!ModInstance.IsInRun)
            {
                ArchipelagoConsole.LogMessage("You are not currently in a run, you can only run this command during a run.");
                return;
            }
            if (Args.Count != 2)
            {
                ArchipelagoConsole.LogMessage($"Error Running Command {Name}: no parameters provided.");
                return;
            }
            string subcommand = Args[0];
            if (!int.TryParse(Args[1], out int count))
            {
                ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                return;
            }
            if (!ResourceManager.IsValidResourceType(subcommand))
            {
                ArchipelagoConsole.LogMessage($"Error Running Command {Name}: invalid subcommand {subcommand}");
            }

            //Make the adjustment
            if (ResourceManager.AdjustResource(subcommand, count))
            {
                ArchipelagoConsole.LogMessage($"Adjusted {subcommand} by {count}.");
            } 
            else
            {
                ArchipelagoConsole.LogMessage($"There was an unknown error adjusting {subcommand}.");
            }


        }
    }
}

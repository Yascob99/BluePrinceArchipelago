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
            if (!int.TryParse(Args[1], out int count){
                ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                return;
            }
            switch (subcommand.ToLower())
            {
                case "gems":
                    ResourceManager.AdjustGems(count);
                    ArchipelagoConsole.LogMessage($"Adjusted Gems by {count}.");
                    return;
                case "gold":
                    ResourceManager.AdjustGold(count);
                    ArchipelagoConsole.LogMessage($"Adjusted Gold by {count}.");
                    return;
                case "steps":
                    ResourceManager.AdjustSteps(count);
                    ArchipelagoConsole.LogMessage($"Adjusted Steps by {count}.");
                    return;
                case "keys":
                    ResourceManager.AdjustKeys(count);
                    ArchipelagoConsole.LogMessage($"Adjusted Keys by {count}.");
                    return;
                case "dice":
                    ResourceManager.AdjustDice(count);
                    ArchipelagoConsole.LogMessage($"Adjusted Dice by {count}.");
                    return;
                case "stars":
                    int totalStars = ModInstance.StarManager.FindIntVariable("TotalStars").Value;
                    if (totalStars + count > 0)
                    {
                        ModInstance.StarManager.FindIntVariable("TotalStars").Value = totalStars + count;

                    }
                    else
                    {
                        ModInstance.StarManager.FindIntVariable("TotalStars").Value = 0;
                    }
                    ArchipelagoConsole.LogMessage($"Adjusted Stars by {count}.");
                    return;
                case "luck":
                    int luck = ModInstance.LuckManager.FindIntVariable("LUCK").Value;
                    if (luck + count > 0)
                    {
                        ModInstance.LuckManager.FindIntVariable("LUCK").Value = luck + count;

                    }
                    else
                    {
                        ModInstance.LuckManager.FindIntVariable("LUCK").Value = 0;
                    }
                    ArchipelagoConsole.LogMessage($"Adjusted Luck by {count}.");
                    return;
                case "allowance":
                    try
                    {
                        GameObject.Find("DAY").GetComponent<PlayMakerFSM>().FindIntVariable("allowance").Value += count;
                    }
                    catch (Exception ex)
                    {
                        ArchipelagoConsole.LogMessage(ex.Message);
                        Logging.Log(ex, "Items");
                    }
                    return;
                default:
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name}: invalid subcommand {subcommand}");
                    return;

            }
        }
    }
}

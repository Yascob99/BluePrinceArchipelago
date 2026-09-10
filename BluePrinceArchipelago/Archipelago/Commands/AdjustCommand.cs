using BluePrinceArchipelago.Utils;
using StableNameDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            ArchipelagoConsole.LogMessage(Args.Join(" "));
            if (!ModInstance.IsInRun)
            {
                ArchipelagoConsole.LogMessage("You are not currently in a run, you can only run this command during a run.");
                return;
            }
            if (Args.Count == 2)
            {
                string subcommand = Args[0];
                if (subcommand.ToLower() == "gems")
                {
                    try
                    {
                        int count = int.Parse(Args[1]);
                        ModInstance.GemManager.FindIntVariable("Gem Adjustment Amount").Value = count;
                        ModInstance.GemManager.SendEvent("Update with Sound");
                        ArchipelagoConsole.LogMessage($"Adjusted Gems by {count}.");
                        return;
                    }
                    catch
                    {
                        ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                        return;
                    }

                }
                else if (subcommand.ToLower() == "gold")
                {
                    try
                    {
                        int count = int.Parse(Args[1]);
                        ModInstance.GoldManager.FindIntVariable("Adjustment Amount").Value = count;
                        ModInstance.GoldManager.SendEvent("Update");
                        ArchipelagoConsole.LogMessage($"Adjusted Gold by {count}.");
                        return;
                    }
                    catch
                    {
                        ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                        return;
                    }

                }
                else if (subcommand.ToLower() == "steps")
                {
                    try
                    {
                        int count = int.Parse(Args[1]);
                        ModInstance.StepManager.FindIntVariable("Adjustment Amount").Value = count;
                        ModInstance.StepManager.SendEvent("Update");
                        ArchipelagoConsole.LogMessage($"Adjusted Steps by {count}.");
                        return;
                    }
                    catch
                    {
                        ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                        return;
                    }

                }
                else if (subcommand.ToLower() == "dice")
                {
                    try
                    {
                        int count = int.Parse(Args[1]);
                        ModInstance.DiceManager.FindIntVariable("Adjustment Amount").Value = count;
                        ModInstance.DiceManager.SendEvent("Update");
                        ArchipelagoConsole.LogMessage($"Adjusted Dice by {count}.");
                        return;
                    }
                    catch
                    {
                        ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                        return;
                    }

                }
                else if (subcommand.ToLower() == "keys")
                {
                    try
                    {
                        int count = int.Parse(Args[1]);
                        ModInstance.KeyManager.FindIntVariable("Adjustment Amount").Value = count;
                        ModInstance.KeyManager.SendEvent("Update");
                        ArchipelagoConsole.LogMessage($"Adjusted Keys by {count}.");
                        return;
                    }
                    catch
                    {
                        ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                        return;
                    }

                }
                else if (subcommand.ToLower() == "stars")
                {
                    try
                    {
                        int count = int.Parse(Args[1]);
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
                    }
                    catch
                    {
                        ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                        return;
                    }

                }
                else if (subcommand.ToLower() == "luck")
                {
                    try
                    {
                        int count = int.Parse(Args[1]);
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
                    }
                    catch
                    {
                        ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                        return;
                    }

                }
                else if (subcommand.ToLower() == "allowance")
                {
                    try
                    {
                        int count = int.Parse(Args[1]);

                        GameObject.Find("DAY").GetComponent<PlayMakerFSM>().FindIntVariable("allowance").Value += count;
                        return;
                    }
                    catch (Exception ex)
                    {
                        ArchipelagoConsole.LogMessage(ex.Message);
                        Logging.Log(ex, "Items");
                        return;
                    }
                }
                ArchipelagoConsole.LogMessage($"Error Running Command {Name}: invalid subcommand {subcommand}");
                return;
            }
            ArchipelagoConsole.LogMessage($"Error Running Command {Name}: no parameters provided.");
        }
    }
}

using System.Collections.Generic;

namespace BluePrinceArchipelago.Archipelago.Commands
{
    public class DeathLinkCommand(string name) : Command(name)
    {
        private string _Description = "Allows you to change which mode of DeathLink you are using with the 'type' subcommand. Allows you to enable spoiler deathlinks being broadcast to the room via the 'spoilers' subcommand.";
        public override string Description
        {
            get { return _Description; }
        }
        private string _Syntax = "Usage:\n\t/DeathLink Type <None|EOD|Bedroom|Steps>\n\t/Deathlink Spoilers <Enable|True/Disable|False>";
        public override string Syntax
        {
            get { return _Syntax; }
        }

        public override void Run(List<string> Args)
        {
            if (Args.Count == 2)
            {
                string subcommand = Args[0];
                if (subcommand.ToLower() == "type")
                {
                    string value = Args[1].ToLower().Trim();

                    int type = -1;
                    if (value == "none")
                    {
                        type = 0;
                    }
                    else if (value == "eod")
                    {
                        type = 1;
                    }
                    else if (value == "bedroom")
                    {
                        type = 2;
                    }
                    else if (value == "steps")
                    {
                        type = 3;
                    }
                    if (type > -1)
                    {
                        if (Plugin.ArchipelagoClient.DeathLinkHandler.ChangeDeathLinkType((DeathLinkType)type))
                        {
                            ArchipelagoConsole.LogMessage($"Deathlink Changed to {Args[1]}.");
                            return;
                        }
                        ArchipelagoConsole.LogMessage($"Deathlink already set to {Args[1]}.");
                        return;
                    }
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name}: {value} is not a valid DeathLink type.");
                    return;
                }
                else if (subcommand.ToLower() == "spoilers") {
                    string value = Args[1].ToLower().Trim();
                    if (value == "true")
                    {
                        DeathLinkMessages.EnableSpoilers();
                    }
                    else if (value == "false")
                    {
                        DeathLinkMessages.DisableSpoilers();
                    }
                    else if (value == "enable")
                    {
                        DeathLinkMessages.EnableSpoilers();
                    }
                    else if (value == "disable")
                    {
                        DeathLinkMessages.DisableSpoilers();
                    }
                }
                ArchipelagoConsole.LogMessage($"Error Running Command {Name}: invalid subcommand {subcommand}");
                return;
            }
            ArchipelagoConsole.LogMessage($"Error Running Command {Name}: Incorrect number of arguements, DeathLink only accepts 2 Arguements.");
        }
    }
}

using System.Collections.Generic;

namespace BluePrinceArchipelago.Archipelago.Commands
{
    public class DeathLinkCommand(string name) : Command(name)
    {
        private string _Description = "Allows you to change which mode of DeathLink you are using.";
        public override string Description
        {
            get { return _Description; }
        }
        private string _Syntax = "Usage:\n\t/DeathLink Type <None|EOD|Bedroom|Steps>";
        public override string Syntax
        {
            get { return _Syntax; }
        }

        public override void Run(List<string> Args)
        {
            if (Args.Count == 2)
            {
                string subcommand = Args[0];
                if (subcommand.ToLower() == "type") { 
                    string value = Args[1];
                    int type = -1;
                    switch (value.ToLower()){
                        case "none":
                            type = 0;
                            break;
                        case "eod":
                            type = 1;
                            break;
                        case "bedroom":
                            type = 2;
                            break;
                        case "steps":
                            type = 3;
                            break;
                    }
                    if (type > 0) {
                        DeathLinkHandler.DeathLinkOverride = true;
                        DeathLinkHandler.DeathLinkTypeOverride = (DeathLinkType)type;
                        return;
                    }
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name}: {value} is not a valid DeathLink type.");
                    return;
                }
                ArchipelagoConsole.LogMessage($"Error Running Command {Name}: invalid subcommand {subcommand}");
                return;
            }
            ArchipelagoConsole.LogMessage($"Error Running Command {Name}: Incorrect number of arguements, DeathLink only accepts 2 Arguements.");
        }
    }
}

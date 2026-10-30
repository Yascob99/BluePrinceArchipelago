using BluePrinceArchipelago;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Archipelago.Commands;
using System.Collections.Generic;

public class TrunkCommand(string name) : Command(name)
{
    private readonly string _Description = "Adjusts settings related to trunks";
    public override string Description
    {
        get { return _Description; }
    }
    private readonly string _Syntax = "Usage:\n\t/trunk chance <0-100>";
    public override string Syntax
    {
        get { return _Syntax; }
    }

    public override void Run(List<string> Args)
    {
        if (Args.Count != 2)
        {
            ArchipelagoConsole.LogMessage($"Error: No subcommand provided.\n{_Syntax}");
            return;
        }
        else {
            if (Args[0].ToLower() == "sync") { 
                int chance = int.Parse(Args[1]);
                if (chance < 101 && chance > -1) { 
                    ModInstance.TrunkManager.TrunkChance = chance;
                    return;
                }
                ArchipelagoConsole.LogMessage($"Error: Chance was not an integer from 0-100\n{_Syntax}");
                return;
            }
        }
    }
}
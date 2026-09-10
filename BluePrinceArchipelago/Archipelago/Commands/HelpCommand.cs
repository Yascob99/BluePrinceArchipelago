using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrinceArchipelago.Archipelago.Commands
{
    /// <summary>
    ///     A command for displaying all of the commands and how to use them.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    public class HelpCommand(string name) : Command(name)
    {
        private string _Description = "Displays all Local Commands";
        public override string Description
        {
            get { return _Description; }
        }
        private readonly string _Syntax = "Usage\n\t/Help";
        public override string Syntax
        {
            get { return _Syntax; }
        }
        public override void Run(List<string> Args)
        {
            CommandManager.PrintHelpText();
        }
    }
}

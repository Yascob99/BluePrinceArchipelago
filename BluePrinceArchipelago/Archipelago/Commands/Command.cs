using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrinceArchipelago.Archipelago.Commands
{
    /// <summary>
    ///     The Command Framework.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    public abstract class Command(string name)
    {
        public string Name = name;

        public abstract string Description
        {
            get;
        }
        public abstract string Syntax
        {
            get;
        }

        /// <summary>
        ///     The core functionality of the command.
        /// </summary>
        /// <param name="Args">The arguements for running the command.</param>
        public abstract void Run(List<string> Args);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrinceArchipelago.Archipelago.Commands
{
    /// <summary>
    ///     A command for reseting the cached and stored data about the current run.
    /// </summary>
    /// <param name="name">The name of the Command.</param>
    public class ResetDataCommand(string name) : Command(name)
    {
        public override string Description => "Resets the stored data so a new run can be properly started.";

        public override string Syntax => "Usage:\n\t/ResetData";

        public override void Run(List<string> Args)
        {
            State.Reset();
            State.Initialize();
        }
    }
}

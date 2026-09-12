using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrinceArchipelago.Models
{
    public static class DeathLinkMessages
    {
        public static Dictionary<string, string[]> DeathLinkMsgDict = new()
        {
            {"Antechanber", ["{0} went post-mortem in the Antechamber.", "{0} thought they had reached Room 46."]},
            {"Apple Orchard", ["{0} discovered gravity in the Apple Orchard."]},
            {"Aquarium", ["{0} is swimming with the fishes in the Aquarium.", "{0} 's tank contains a dead herring."]},

        };
    }
}

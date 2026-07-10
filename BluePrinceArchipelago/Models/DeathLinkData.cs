using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrinceArchipelago.Models
{
    public class DeathLinkData
    {
        public int DeathLinkCount { get; set; }

        public int TotalDeathLinksSent { get; set; }

        public bool DeathLinkEnabled { get; set; }

        public int BlockedDeaths { get; set; }
    }
}

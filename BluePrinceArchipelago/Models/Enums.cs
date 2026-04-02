using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrinceArchipelago
{
    public enum GoalType
    {
        option_antechamber = 0,
        option_room46 = 1,
        option_sanctum = 2,
        option_ascend = 3,
        option_blueprints = 4
    }
    public enum DeathLinkType {
        option_none = 0,
        option_eod = 1,
        option_bedroom = 2,
        option_steps = 3
    }
    public enum ItemLogicMode {
        option_default = 0,
        option_rare = 1,
        option_complex = 2,
        option_rare_complex = 3,
        option_extreme = 4
    }
}

using BluePrinceArchipelago.Archipelago;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrinceArchipelago.Triggers
{
    /// <summary>
    ///     Triggers related to Archipelago Goals
    /// </summary>
    public static class GoalTriggers
    {
        /// <summary>
        ///     On the Room 46 goal being completed
        /// </summary>
        public static void OnRoom46Goal()
        {
            // prevent death link from triggering before the goal completion is sent
            DeathLinkHandler.OnRoom46FirstEntered();
            Plugin.ArchipelagoClient.GoalCompleted();
        }

        /// <summary>
        ///     Triggers on the Antechamber goal
        /// </summary>
        public static void OnAntechamberGoal()
        {
            Plugin.ArchipelagoClient.GoalCompleted();
        }

        /// <summary>
        ///     Triggers on the Ascend goal
        /// </summary>
        public static void OnAscendGoal() {
            Plugin.ArchipelagoClient.GoalCompleted();
        }
    }
}

using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Items;
using System.Diagnostics;
using System.Linq;

namespace BluePrinceArchipelago.Triggers
{
    public static class TrapTriggers
    {
        /// <summary>
        ///     Triggers when a trap is received.
        /// </summary>
        /// <param name="itemInfo">The ItemInfo of the trap.</param>
        public static void OnTrapReceived(ItemInfo itemInfo) {
            Trap trap = ModItemManager.TrapList.FirstOrDefault(trap => trap.Name.ToLower() == itemInfo.ItemName.ToLower());
            if (trap != null)
            {
                trap.ActivateTrap();
            }
            else
            {
                Logging.LogError($"Error receiving {itemInfo.ItemName}: No Trap with that name could be found.");
            }
        }
    }
}

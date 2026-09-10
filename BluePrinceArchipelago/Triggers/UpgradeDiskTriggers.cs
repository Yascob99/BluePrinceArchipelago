using BluePrinceArchipelago.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrinceArchipelago.Triggers
{
    public static class UpgradeDiskTriggers
    {
        public static void OnUpgradeDiskTraded() {
            ModItemManager.UpgradeDisks.OnTrade();
        }

        public static void OnUpgradeDiskUsed(int upgradeId)
        {
            ModItemManager.UpgradeDisks.OnUsed(upgradeId);
        }

        public static void OnUpgradeDiskPickedUp() {
            ModItemManager.UpgradeDisks.OnPickup();
        }
    }
}

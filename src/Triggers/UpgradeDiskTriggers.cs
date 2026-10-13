using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
#if Bep
using TMPro;
#endif
#if ML
using Il2CppTMPro;
#endif
namespace BluePrinceArchipelago.Triggers
{
    public static class UpgradeDiskTriggers
    {
        /// <summary>
        ///     Triggers on the the Trading Post Upgrade Disk being traded for.
        /// </summary>
        public static void OnUpgradeDiskTraded() {
            ModItemManager.UpgradeDisks.OnTrade();
        }

        /// <summary>
        ///     Triggers on the on an Upgrade Disk being used.
        /// </summary>
        /// <param name="upgradeId"></param>
        public static void OnUpgradeDiskUsed(int upgradeId)
        {
            ModItemManager.UpgradeDisks.OnUsed(upgradeId);
        }

        /// <summary>
        ///     Triggers on an Upgrade Disk being picked up.
        /// </summary>
        public static void OnUpgradeDiskPickedUp() {
            string roomname = ModInstance.RoomText.GetComponent<TextMeshPro>()?.text ?? ""; ;
            // TP Dynamite, and Abandoned Mine Name fix;
            roomname = roomname.ToUpper().Replace("POST", "POST DYNAMITE").Replace("UNDERGROUND", "ABANDONED MINE");
            // Below the Foundation sets the RoomText to "", set it to default to foundation.
            if (roomname == "")
            {
                roomname = "THE FOUNDATION";
            }
            ModItemManager.UpgradeDisks.OnFind(roomname);
        }
    }
}

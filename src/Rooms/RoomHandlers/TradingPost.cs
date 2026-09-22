
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
#endif
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers
{
    public class TradingPost : RoomHandler
    {
        public TradingPost()
        {
            AllowanceTokens.Add("Trading Post");
        }

        public override void OnAllowanceTokenCollected(string token)
        {
            ModInstance.ModEventHandler.OnMoraJaiSolved("Trading Post");
        }

        public override void OnAfterRoomDrafted(GameObject roomGameObject)
        {
            roomGameObject = ModRoomManager.GetRoomInstance("Trading Post");
            if (roomGameObject != null)
            {
                PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_CULLABLE/_Non Static/AFTER EXPLOSION/2")?.GetComponent<PlayMakerFSM>();
                if (ItemDropFSM != null)
                {
                    bool found = ModItemManager.UpgradeDisks.FoundLocations.Contains("TRADING POST DYNAMITE");
                    Logging.LogWarning(found);
                    FsmBool CanSpawnDisk = ItemDropFSM.AddBoolVariable("CanSpawnDisk");
                    CanSpawnDisk.Value = found;
                    ItemDropFSM.GetState("State 5").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
                    ArrayListContains CheckInInventory = ItemDropFSM.GetState("State 2").GetFirstActionOfType<ArrayListContains>();
                    CheckInInventory.isContainedEvent = CheckInInventory.isNotContainedEvent;
                }
                else
                {
                    Logging.LogWarning("Error changing Tomb Upgrade disk spawn logic.");
                }
            }
        }

        private void ReplaceTradeText()
        {
            
        }
    }
}   
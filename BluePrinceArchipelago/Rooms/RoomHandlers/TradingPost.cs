
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
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

        public override void OnRoomDrafted(GameObject roomGameObject)
        {
            RoomGameObject = roomGameObject;
            PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_CULLABLE/_Non Static/AFTER EXPLOSION/2")?.GetComponent<PlayMakerFSM>();
            if (ItemDropFSM != null)
            {
                bool found = !ModItemManager.UpgradeDisks.FoundLocations.Contains("TRADING POST DYNAMITE");
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

        public override void OnAfterRoomDrafted(GameObject roomGameObject)
        {
            // Logging.Log("Trading Post drafted. Setting up FSM hooks.");
        }

        private void ReplaceTradeText()
        {
            
        }
    }
}   
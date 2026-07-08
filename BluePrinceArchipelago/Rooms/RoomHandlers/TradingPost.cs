
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
            }
            else
            {
                Logging.LogWarning("Error changing Tomb Upgrade disk spawn logic.");
            }
            //_ClickTradingPostColliderFSM = RoomGameObject.transform.Find("_GAMEPLAY/ITEMS FOR TRADE/Click Trading Post Collider")?.GetComponent<PlayMakerFSM>(); ;
            //_MoreButtonFSM = UIOverlayCam.transform.Find("Trading Post Menu/Menu Buttons/more button")?.GetComponent<PlayMakerFSM>(); ;
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

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
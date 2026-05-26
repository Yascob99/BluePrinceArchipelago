using System;
using BluePrinceArchipelago.Utils;
using BluePrinceArchipelago.Utils.Actions;
using HarmonyLib;
using HutongGames.PlayMaker;
using Il2CppSystem.Linq;
using UnityEngine;

namespace BluePrinceArchipelago.RoomHandlers
{
    public class TradingPost : RoomHandler
    {
        public TradingPost()
        {
            Logging.Log("Initializing Trading Post.");
        }

        public override void OnRoomDrafted(GameObject roomGameObject)
        {
            RoomGameObject = roomGameObject;
            //_ClickTradingPostColliderFSM = RoomGameObject.transform.Find("_GAMEPLAY/ITEMS FOR TRADE/Click Trading Post Collider")?.GetComponent<PlayMakerFSM>(); ;
            //_MoreButtonFSM = UIOverlayCam.transform.Find("Trading Post Menu/Menu Buttons/more button")?.GetComponent<PlayMakerFSM>(); ;
        }

        public override void OnAfterRoomDrafted()
        {
            // Logging.Log("Trading Post drafted. Setting up FSM hooks.");
        }

        private void ReplaceTradeText()
        {
            
        }
    }
}   
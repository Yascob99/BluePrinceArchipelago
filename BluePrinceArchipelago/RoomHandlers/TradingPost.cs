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
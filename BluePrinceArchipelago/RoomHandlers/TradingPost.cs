
using HarmonyLib;
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
            
        }

        [HarmonyPatch(typeof(TradeManager), nameof(TradeManager.RefreshTradingPairs))]
        [HarmonyPostfix]
        public static void Postfix(TradeManager __instance) {
            Logging.Log("TradeManager RefreshTradingPairs Postfix called.");
        }

        [HarmonyPatch(typeof(TradeManager), nameof(TradeManager.SetTrade))]
        [HarmonyPostfix]
        public static void SetTradePostfix(TradeManager __instance)
        {
            Logging.Log("TradeManager SetTrade Postfix called.");
        }

        [HarmonyPatch(typeof(TradeManager), nameof(TradeManager.SetTradeOffer))]
        [HarmonyPostfix]
        public static void SetTradeOfferPostfix(TradeManager __instance)
        {
            Logging.Log("TradeManager SetTradeOffer Postfix called.");
        }

        [HarmonyPatch(typeof(TradeManager), nameof(TradeManager.GenerateNewOffers))]
        [HarmonyPostfix]
        public static void GenerateNewOffersPostfix(TradeManager __instance)
        {
            Logging.Log("TradeManager GenerateNewOffers Postfix called.");
        }
    }
}
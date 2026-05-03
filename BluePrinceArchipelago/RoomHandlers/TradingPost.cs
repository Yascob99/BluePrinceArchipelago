using System;
using BluePrinceArchipelago.Utils;
using BluePrinceArchipelago.Utils.Actions;
using HarmonyLib;
using Il2CppSystem.Linq;
using UnityEngine;

namespace BluePrinceArchipelago.RoomHandlers
{
    public class TradingPost : RoomHandler
    {
        private PlayMakerFSM _ClickTradingPostColliderFSM;
        private PlayMakerFSM _MoreButtonFSM;
        public TradingPost()
        {
            Logging.Log("Initializing Trading Post.");
        }

        public override void OnRoomDrafted(GameObject roomGameObject)
        {
            RoomGameObject = roomGameObject;
            _ClickTradingPostColliderFSM = RoomGameObject.transform.Find("_GAMEPLAY/ITEMS FOR TRADE/Click Trading Post Collider").gameObject.GetFsm("FSM");
            _MoreButtonFSM = UIOverlayCam.transform.Find("Trading Post Menu/Menu Buttons/more button").gameObject.GetFsm("FSM");
        }

        public override void OnAfterRoomDrafted()
        {
            Logging.Log("Trading Post drafted. Setting up FSM hooks.");
            SetupTradingPost();
        }

        private void SetupTradingPost()
        {
            if (_ClickTradingPostColliderFSM == null || _MoreButtonFSM == null)
            {
                Logging.LogError("Trading Post FSMs not found. Cannot set up Trading Post.");
                return;
            }

            var clickState = _ClickTradingPostColliderFSM.GetState("Click");
            if (clickState == null)
            {
                Logging.LogError("Click state not found in Click Trading Post Collider FSM.");
                return;
            }

            var moreClickState = _MoreButtonFSM.GetState("State 2");
            if (moreClickState == null)
            {
                Logging.LogError("State 2 not found in More Button FSM.");
                return;
            }

            clickState.AddLambdaMethod(OnTradingPostClicked);

            for (int i = 0; i < clickState.Actions.Length; i++)
            {
                var action = clickState.Actions[i];
                Logging.Log($"Click Trading Post Collider FSM Action {i}: {action.GetType().FullName}: {(action is MethodAction methodAction ? methodAction.Method?.Method.Name : "N/A")}");
            }

            moreClickState.AddLambdaMethod(OnMoreClicked);

            for (int i = 0; i < moreClickState.Actions.Length; i++)
            {
                var action = moreClickState.Actions[i];
                Logging.Log($"More Button FSM Action {i}: {action.GetType().FullName}: {(action is MethodAction methodAction ? methodAction.Method?.Method.Name : "N/A")}");
            }
        }

        public static void OnTradingPostClicked(Action finishAction)
        {
            Logging.Log("Trading Post clicked.");
            Logging.Log($"Finish action: {finishAction?.Method.Name ?? "null"}");
        }

        public static void OnMoreClicked(Action finishAction)
        {
            Logging.Log("Trading Post 'More' button clicked.");
            Logging.Log($"Finish action: {finishAction?.Method.Name ?? "null"}");
        }
    }
}   
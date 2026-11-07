using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using TMPro;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
using Il2CppTMPro;
#endif
using UnityEngine;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    public class LockContraption(string contraptionName, int currentInstance) : RegisteredCustomFsmMethod
    {
        public new readonly string Name = CustomFsmMethodManager.GetItemOnCraftMethodName(contraptionName, currentInstance);

        private readonly string WRENCH_OBJECT_PATH = $"__SYSTEM/Room Spawn Pools/Workshop(Clone)00{currentInstance}/_GAMEPLAY/WorkShop Table/ICONS TEXT/Wrench";
        private const string ASSEMBLE_TEXT_OBJECT_SUBPATH = "Assemble";
        private const string CLICK_STATE_NAME = "Click";
        private const string NORMAL_STATE_NAME = "State 3";
        private const string HOVERED_STATE_NAME = "State 4";
        private const string CLICK_EVENT_NAME = "click";
        private const string LOCKED_DISPLAY_TEXT = "LOCKED";
        private const string UNLOCKED_DISPLAY_TEXT = "ASSEMBLE";

        private GameObject workshopWrench = new();
        private PlayMakerFSM wrenchFSM = new();

        public override void OnRegister()
        {
            workshopWrench = GameObject.Find(WRENCH_OBJECT_PATH);
            wrenchFSM = GameObject.Find(WRENCH_OBJECT_PATH).GetComponent<PlayMakerFSM>();
        }

        public override void OnCalled()
        {
            UniqueItem contraption = ModItemManager.GetUniqueItem(contraptionName);
            if (contraption.HasBeenFound && !contraption.IsUnlocked)
            {
                LockCraftButton();
                ChangeCraftButtonText(LOCKED_DISPLAY_TEXT);
                return;
            }
            UnlockCraftButton();
            ChangeCraftButtonText(UNLOCKED_DISPLAY_TEXT);
        }

        private void LockCraftButton()
        {
            wrenchFSM.GetState(NORMAL_STATE_NAME).RemoveTransitionsTo(CLICK_STATE_NAME);
            wrenchFSM.GetState(HOVERED_STATE_NAME).RemoveTransitionsTo(CLICK_STATE_NAME);
        }

        private void UnlockCraftButton()
        {
            wrenchFSM.GetState(NORMAL_STATE_NAME).AddTransition(CLICK_EVENT_NAME, CLICK_STATE_NAME);
            wrenchFSM.GetState(HOVERED_STATE_NAME).AddTransition(CLICK_EVENT_NAME, CLICK_STATE_NAME);
        }

        private void ChangeCraftButtonText(string text)
        {
            workshopWrench.transform.Find(ASSEMBLE_TEXT_OBJECT_SUBPATH).GetComponent<TextMeshPro>().text = text;
        }
    }
}
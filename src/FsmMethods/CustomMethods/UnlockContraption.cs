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
    public class UnlockContraption() : RegisteredCustomFsmMethod
    {
        private const string WRENCH_OBJECT_PATH = "__SYSTEM/Room Spawn Pools/Workshop(Clone)001/_GAMEPLAY/WorkShop Table/ICONS TEXT/Wrench";
        private const string ASSEMBLE_TEXT_OBJECT_NAME = "Assemble";
        private const string CLICK_STATE_NAME = "Click";
        private const string NORMAL_STATE_NAME = "State 3";
        private const string HOVERED_STATE_NAME = "State 4";
        private const string LOCKED_DISPLAY_TEXT = "LOCKED";


        public new string Name { get; set; } = "LockContraption";

        public override void OnCalled()
        {
            //TODO : Handle multiple instances... 
            GameObject workshopWrench = GameObject.Find(WRENCH_OBJECT_PATH);
            LockCraftButton(workshopWrench);
            IndicateCraftButtonIsLocked(workshopWrench);
        }

        private static void LockCraftButton(GameObject workshopWrench)
        {
            PlayMakerFSM wrenchFSM = workshopWrench.GetComponent<PlayMakerFSM>();
            wrenchFSM.GetState(NORMAL_STATE_NAME).AddTransition("click", CLICK_STATE_NAME);
            wrenchFSM.GetState(HOVERED_STATE_NAME).AddTransition("click", CLICK_STATE_NAME);
        }

        private static void IndicateCraftButtonIsLocked(GameObject workshopWrench)
        {
            workshopWrench.transform.Find(ASSEMBLE_TEXT_OBJECT_NAME).GetComponent<TextMeshPro>().text = LOCKED_DISPLAY_TEXT;
        }
    }
}
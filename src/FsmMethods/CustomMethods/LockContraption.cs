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
    public class LockContraption() : RegisteredCustomFsmMethod
    {
        public new string Name { get; set; } = "LockContraption";

        public override void OnCalled()
        {
            //TODO : Handle multiple instances... 
            GameObject workshopWrench = GameObject.Find("__SYSTEM/Room Spawn Pools/Workshop(Clone)001/_GAMEPLAY/WorkShop Table/ICONS TEXT/Wrench");

            //PlayMakerFSM workshopFSM = workshopMenu.transform.Find("ICONS TEXT").GetComponent<PlayMakerFSM>();
            
            //INDICATE THAT WRENCH IS LOCKED
            workshopWrench.transform.Find("Assemble").GetComponent<TextMeshPro>().text = "LOCKED";
        }
    }
}
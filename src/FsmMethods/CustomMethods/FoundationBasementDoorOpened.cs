using BluePrinceArchipelago.Triggers;
using BluePrinceArchipelago.Utils;

#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
#endif

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    /// <summary>
    ///     An Open Event for the Basement Door
    /// </summary>
    public class FoundationBasementDoorOpened : RegisteredCustomFsmMethod
    {

        public new string Name { get; set; } = "FoundationBasementDoorOpened";

        public bool isUnlocked = false;

        public override void Update()
        {
            isUnlocked = ModInstance.GlobalPersistentManager.GetBoolVariable("Basement Door 1").Value;
        }
        public override void OnCalled()
        {
            if (isUnlocked){
                EventTriggers.OnBasementDoorOpened("The Foundation");
            }
        }

    }
}

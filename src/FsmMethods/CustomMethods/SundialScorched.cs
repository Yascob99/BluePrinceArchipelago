using BluePrinceArchipelago.Events;
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
    public class SundialScorched : RegisteredCustomFsmMethod
    {
        public new string Name { get; set; } = "SundialScorched";

        public override void OnCalled()
        {
            EventTriggers.OnSundailScorched();
        }
    }
}

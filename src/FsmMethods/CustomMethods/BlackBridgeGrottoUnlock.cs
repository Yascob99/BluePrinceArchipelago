using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Items;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    /// <summary>
    ///     An Unlock event for the BlackBridgeGrotto.
    /// </summary>
    public class BlackBridgeGrottoUnlock : RegisteredCustomFsmMethod
    {

        public new string Name { get; set; } = "BlackbridgeGrottoUnlock";

        public override void OnCalled()
        {
            Unlocks.BlackBridgeGrotto.FoundLocation();
        }
    }
}

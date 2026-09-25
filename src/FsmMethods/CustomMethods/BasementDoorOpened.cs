using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
#if ML
using Il2Cpp;
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
    ///     An Open Event for the Basement Door
    /// </summary>
    public class BasementDoorOpened : RegisteredCustomFsmMethod
    {

        public new string Name { get; set; } = "BasementDoorOpened";

        public override void OnCalled()
        {
            Unlocks.BlackBridgeGrotto.FoundLocation();
        }
    }
}

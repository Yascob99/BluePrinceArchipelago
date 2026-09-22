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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    /// <summary>
    ///     On the allowance token being picked up.
    /// </summary>
    /// <param name="name">The name of the event.</param>
    public class AllowanceEnvelopePickedUp(string name) : RegisteredCustomFsmMethod
    {
        public new string Name { get; set; } = name;

        public override void OnCalled()
        {
            EventTriggers.OnAllowanceEnvelopePickedUp();
        }
    }
}

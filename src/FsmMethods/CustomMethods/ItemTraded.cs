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
    public class ItemTraded : RegisteredCustomFsmMethod
    {
        public new string Name { get; set; } = "ItemTraded";

        public GameObject OfferItem { get; set; }

        public GameObject Icon { get; set; }

        public override void OnRegister()
        {
            OfferItem = null;
            Icon = null;
        }

        public override void Update() {
            PlayMakerFSM TradingPostMenu = GameObject.Find("UI OVERLAY CAM").transform.Find("Trading Post Menu").Find("Items PM bridge").gameObject.GetComponent<PlayMakerFSM>();
            OfferItem = TradingPostMenu.GetGameObjectVariable("Offered_item").Value;
            Icon = TradingPostMenu.GetGameObjectVariable("Icon").Value;
        }
        public override void OnCalled()
        {
            if (OfferItem != null && Icon != null)
            {
                ItemTriggers.OnItemTraded(OfferItem, Icon);
            }
            else {
                Logging.Log($"The {Name} Custom method could not be run, OfferItem and/or Icon was null", "CustomFsmMethods");
            }
        }
    }
}

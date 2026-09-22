using BluePrinceArchipelago.FsmMethods.CustomMethods;
using BluePrinceArchipelago.Utils;
using BluePrinceArchipelago.Items;
#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
#endif
#if ML
using Il2Cpp;
using HutongGames = Il2CppHutongGames;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
#endif
using Il2CppInterop.Runtime;
using System.Collections.Generic;


namespace BluePrinceArchipelago.FsmMethods
{
    public static class CustomFsmMethodManager
    {

        public static Dictionary<string, RegisteredCustomFsmMethod> RegisteredCustomFsmMethods = new Dictionary<string, RegisteredCustomFsmMethod>() {
            { "AppleOrchardUnlock", new AppleOrchardUnlock() },
            { "BlackbridgeGrottoUnlock", new BlackBridgeGrottoUnlock() },
            { "WestGatePathUnlock", new WestGatePathUnlock() },
            { "GemstoneCavernsUnlock", new GemstoneCavernsUnlock() },
            { "SatelliteRaised", new SatelliteRaised() },
            { "OuterDraftReroll", new OuterDraftReroll() },
            { "ItemTraded", new ItemTraded()},
            { "SundialScorched", new SundialScorched()},
            { "GarageOpened", new GarageOpened() },
            { "ShowroomMenuOpened", new ShowroomMenuOpened() },
        };

        public static void RegisterMethods() {
            foreach (RegisteredCustomFsmMethod Method in RegisteredCustomFsmMethods.Values) {
                Method.OnRegister();
            }
        }

        /// <summary>
        ///     Adds an FSM event related to a Unique Item pickup.
        /// </summary>
        /// <param name="item">The Unique Item that is picked up.</param>
        /// <returns></returns>
        public static RegisteredCustomFsmMethod AddItemPickedUpMethod(UniqueItem item)
        {
            string pickedUpName = GetItemPickedUpMethodName(item.Name);
            RegisteredCustomFsmMethod Method = new ItemPickedUp(item.Name);
            RegisteredCustomFsmMethods[pickedUpName] = Method;
            Logging.Log($"Registered New Method: {pickedUpName}", "CustomFsmMethods");
            Method.OnRegister();
            return Method;
        }
        public static string GetItemPickedUpMethodName(string itemName) {
            return itemName.ToTitleCase().Replace(" ", "") + "ItemPickedUp";
        }

        /// <summary>
        ///     Adds an Custom Fsm Method related to a Unique Item pickup.
        /// </summary>
        /// <param name="item">The Unique Item that is picked up.</param>
        /// <returns></returns>
        public static RegisteredCustomFsmMethod AddItemBoughtMethod(UniqueItem item)
        {
            string boughtName = GetItemBoughtMethodName(item.Name);
            RegisteredCustomFsmMethod Method = new ItemBought(item.Name);
            RegisteredCustomFsmMethods[boughtName] = Method;
            Logging.Log($"Registered New Method: {boughtName}", "CustomFsmMethods");
            Method.OnRegister();
            return Method;
        }
        public static string GetItemBoughtMethodName(string itemName)
        {
            return itemName.ToTitleCase().Replace(" ", "") + "ItemBought";
        }

        /// <summary>
        ///     Adds an FSM event related to a Unique Item being dug up.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static RegisteredCustomFsmMethod AddItemDugUpMethod(UniqueItem item)
        {
            string dugUpName = GetItemDugUpMethodName(item.Name);
            RegisteredCustomFsmMethod Method = new ItemDugUp(item.Name);
            RegisteredCustomFsmMethods[dugUpName] = Method;
            Logging.Log($"Registered New Method: {dugUpName}", "CustomFsmMethods");
            Method.OnRegister();
            return Method;
        }
        public static string GetItemDugUpMethodName(string itemName)
        {
            return itemName.ToTitleCase().Replace(" ", "") + "ItemDugUp";
        }

        public static RegisteredCustomFsmMethod RegisterCustomFsmMethod(string methodname, RegisteredCustomFsmMethod method)
        {
            RegisteredCustomFsmMethods[methodname] = method;
            Logging.Log($"Registered New Method: {methodname}", "CustomFsmMethods");
            method.OnRegister();
            return method;
        }
        public static CallMethod GetCallMethod(string methodname)
        {
            if (RegisteredCustomFsmMethods.ContainsKey(methodname))
            {
                FsmVar method = new FsmVar(Il2CppType.Of<string>());
                method.Init(new FsmString("methodname") { value = methodname });
                FsmObject modInstance = new FsmObject() { value = CustomFsmMethods.Instance };
                return new CallMethod() { behaviour = modInstance, methodName = "RunCustomMethod", parameters = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<FsmVar>([method]), everyFrame = false };
            }
            Logging.Log($"Failed to find {methodname}", "CustomFsmMethods");
            return null;
        }
    }
}

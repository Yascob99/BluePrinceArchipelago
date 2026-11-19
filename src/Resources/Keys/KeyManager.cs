#if Bep
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
#endif
#if ML
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
#endif


namespace BluePrinceArchipelago.Resources
{
    public static class KeyManager
    {

        private static KeyResourceManager _ResourceManagerInstance = null;


        /// <summary>
        ///     Gets the Dice Resource Manager Instance
        /// </summary>
        public static void Initialize()
        {
            _ResourceManagerInstance = Plugin.ModObject.GetComponent<KeyResourceManager>();
        }

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Update/Add Coins.
        /// </summary>
        public static CallMethod GetUpdateMethod => GetCallMethod("RunUpdate");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Update/Update2/UpdateSubtract
        /// </summary>
        public static CallMethod GetUpdate2Method => GetCallMethod("RunUpdate2");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Freeze/QuickFreeze
        /// </summary>
        public static CallMethod GetFreezeMethod => GetCallMethod("Freeze");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for UnFreeze/QuickUnfreeze
        /// </summary
        public static CallMethod GetUnFreezeMethod => GetCallMethod("UnFreeze");

        /// <summary>
        ///     Sets the steps to the amount provided
        /// </summary>
        /// <param name="amount">The int to set dice to</param>
        public static void SetKeys(int amount) => _ResourceManagerInstance.SetKeys(amount);


        /// <summary>
        ///     Adjusts the number of dice by the amount. This can a be positive or negative.
        /// </summary>
        /// <param name="amount">The int to adjust steps by</param>
        public static void AdjustKeys(int amount) => _ResourceManagerInstance.AdjustKeys(amount);

        /// <summary>
        ///     Gets a CallMethod with the given Method name for the MonoBehavior.
        /// </summary>
        /// <param name="methodname"></param>
        /// <returns></returns>
        private static CallMethod GetCallMethod(string methodname)
        {
            FsmObject StepResourceManagerInstance = new FsmObject() { value = _ResourceManagerInstance };
            return new CallMethod() { behaviour = StepResourceManagerInstance, methodName = methodname, parameters = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<FsmVar>([]), everyFrame = false };
        }
    }
}
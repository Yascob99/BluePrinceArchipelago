using BluePrinceArchipelago.FsmMethods;
using Il2CppInterop.Runtime;
#if Bep
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
#endif
#if ML
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using Il2CppSteamworks;
#endif


namespace BluePrinceArchipelago.Resources
{
    public static class GemManager
    {

        private static GemResourceManager _ResourceManagerInstance = null;


        /// <summary>
        ///     Gets the Dice Resource Manager Instance
        /// </summary>
        public static void Initialize()
        {
            _ResourceManagerInstance = Plugin.ModObject.GetComponent<GemResourceManager>();
        }

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Update/UpdateSubtract
        /// </summary>
        public static CallMethod GetUpdateMethod => GetCallMethod("RunUpdate");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Update2
        /// </summary>
        public static CallMethod GetUpdate2Method => GetCallMethod("RunUpdate2");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for UpdateWithSound
        /// </summary>
        public static CallMethod GetUpdateWithSoundMethod => GetCallMethod("RunUpdateWithSound");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Freeze/QuickFreeze
        /// </summary>
        public static CallMethod GetFreezeMethod => GetCallMethod("Freeze");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for UnFreeze/QuickUnfreeze
        /// </summary
        public static CallMethod GetUnFreezeMethod => GetCallMethod("UnFreeze");

        /// <summary>
        ///     Sets the gems to the amount provided
        /// </summary>
        /// <param name="amount">The int to set gems to</param>
        public static void SetGems(int amount) => _ResourceManagerInstance.SetGems(amount);


        /// <summary>
        ///     Adjusts the number of gems by the amount. This can a be positive or negative.
        /// </summary>
        /// <param name="amount">The int to adjust gems by</param>
        public static void AdjustGems(int amount) => _ResourceManagerInstance.AdjustGems(amount);

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
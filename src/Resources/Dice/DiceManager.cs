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
    public static class DiceManager
    {

        private static DiceResourceManager _ResourceManagerInstance = null;


        /// <summary>
        ///     Gets the Dice Resource Manager Instance
        /// </summary>
        public static void Initialize()
        {
            _ResourceManagerInstance = Plugin.ModObject.GetComponent<DiceResourceManager>();
        }

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Update/Update2/UpdateSubtract
        /// </summary>
        public static CallMethod GetUpdateMethod => GetCallMethod("RunUpdate");

        /// <summary>
        ///     Sets the steps to the amount provided
        /// </summary>
        /// <param name="amount">The int to set dice to</param>
        public static void SetDice(int amount) => _ResourceManagerInstance.SetDice(amount);


        /// <summary>
        ///     Adjusts the number of dice by the amount. This can a be positive or negative.
        /// </summary>
        /// <param name="amount">The int to adjust steps by</param>
        public static void AdjustDice(int amount) => _ResourceManagerInstance.AdjustDice(amount);

        public static void StartOfDay()
        {
            if (_ResourceManagerInstance != null)
            {
                _ResourceManagerInstance.StartOfDay();
            }
        }

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
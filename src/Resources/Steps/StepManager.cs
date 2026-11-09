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


namespace BluePrinceArchipelago.Resources {
    public static class StepManager
    {

        private static StepResourceManager _ResourceManagerInstance = null;


        /// <summary>
        ///     Gets the Step Resource Manager Instance for use in
        /// </summary>
        public static void Initialize() { 
            _ResourceManagerInstance = Plugin.ModObject.GetComponent<StepResourceManager>();
        }


        /// <summary>
        ///     Gets the CallMethod FsmStateAction for UpdateStep
        /// </summary>
        public static CallMethod GetUpdateStepMethod => GetCallMethod("RunUpdateStep");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Update
        /// </summary>
        public static CallMethod GetUpdateMethod => GetCallMethod("RunUpdate");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Event0
        /// </summary>
        public static CallMethod GetEvent0 => GetCallMethod("Event0");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Event10
        /// </summary>
        public static CallMethod GetEvent10 => GetCallMethod("Event10");

        /// <summary>
        ///     Sets the steps to the amount provided
        /// </summary>
        /// <param name="amount">The int to set steps to</param>
        public static void SetSteps(int amount) => _ResourceManagerInstance.SetSteps(amount);


        /// <summary>
        ///     Adjusts the number of steps by the amount. This can a be positive or negative.
        /// </summary>
        /// <param name="amount">The int to adjust steps by</param>
        public static void AdjustSteps(int amount) => _ResourceManagerInstance.AdjustSteps(amount);

        /// <summary>
        ///     Gets a CallMethod with the given Method name for the MonoBehavior.
        /// </summary>
        /// <param name="methodname"></param>
        /// <returns></returns>
        private static CallMethod GetCallMethod(string methodname) {
            FsmObject StepResourceManagerInstance = new FsmObject() { value = _ResourceManagerInstance };
            return new CallMethod() { behaviour = StepResourceManagerInstance, methodName = methodname, parameters = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<FsmVar>([]), everyFrame = false };
        }
    }
}
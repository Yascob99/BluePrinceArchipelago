using System;
using UnityEngine;
#if ML
using MelonLoader;
#endif


namespace BluePrinceArchipelago.FsmMethods
{
#if ML
    [RegisterTypeInIl2Cpp]
#endif

    internal class CustomFsmMethods : MonoBehaviour
    {
        public static CustomFsmMethods Instance;


        public CustomFsmMethods(IntPtr ptr) : base(ptr)
        {
            Instance = this; //Set the modInstance for easy access.
        }

        public void RunCustomMethod(string methodname)
        {
            Logging.Log($"Attempting to find method {methodname}", "CustomFsmMethods");
            foreach (string key in CustomFsmMethodManager.RegisteredCustomFsmMethods.Keys) {
                if (methodname == key) {
                    CustomFsmMethodManager.RegisteredCustomFsmMethods[key].Update();
                    CustomFsmMethodManager.RegisteredCustomFsmMethods[key].OnCalled();
                    return;
                }
            }
            Logging.Log($"{methodname} was not a valid Method Name", "CustomFsmMethods");
        }

        private void Start() {
            CustomFsmMethodManager.RegisterMethods();
        }
    }
}

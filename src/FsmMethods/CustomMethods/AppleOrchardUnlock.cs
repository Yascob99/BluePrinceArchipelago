using BluePrinceArchipelago.Triggers;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    /// <summary>
    ///     A unlock event for the Apple Orchard.
    /// </summary>
    public class AppleOrchardUnlock : RegisteredCustomFsmMethod
    {

        public new string Name { get; set; } = "AppleOrchardUnlock";

        public override void OnCalled()
        {
            PermanentUnlockTriggers.OnAppleOrchardUnlock();
        }
    }
}

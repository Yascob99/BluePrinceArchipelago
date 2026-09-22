using BluePrinceArchipelago.Items;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    /// <summary>
    ///     An unlock event for the WestGatePath.
    /// </summary>
    public class WestGatePathUnlock : RegisteredCustomFsmMethod
    {

        public new string Name { get; set; } = "WestGatePathUnlock";


        public override void OnCalled()
        {
            Unlocks.WestGatePath.FoundLocation();
        }
    }
}

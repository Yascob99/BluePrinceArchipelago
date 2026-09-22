using BluePrinceArchipelago.Items;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    /// <summary>
    ///     An unlock event for the Gemstone Caverns.
    /// </summary>
    public class GemstoneCavernsUnlock : RegisteredCustomFsmMethod
    {

        public new string Name { get; set; } = "GemstoneCavernsUnlock";

        public override void OnCalled()
        {
            Unlocks.GemstoneCaverns.FoundLocation();
        }
    }
}

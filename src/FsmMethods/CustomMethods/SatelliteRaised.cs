using BluePrinceArchipelago.Items;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    /// <summary>
    ///     An Unlock event for when the Satelite is raised.
    /// </summary>
    public class SatelliteRaised : RegisteredCustomFsmMethod
    {

        public new string Name { get; set; } = "SatelliteRaised";


        public override void OnCalled()
        {
            Unlocks.SatelliteDish.FoundLocation();
        }
    }
}

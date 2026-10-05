using BluePrinceArchipelago.Triggers;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    /// <summary>
    ///     On the allowance token being picked up.
    /// </summary>
    public class BeforeDraftStart : RegisteredCustomFsmMethod
    {
        public new string Name { get; set; } = "BeforeDraftStart";

        public override void OnCalled()
        {

            DraftTriggers.OnBeforeDraftStart();
        }
    }
}
using BluePrinceArchipelago.Triggers;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    public class OuterDraftReroll() : RegisteredCustomFsmMethod
    {
        public new string Name { get; set; } = "OuterDraftReroll";

        public override void OnCalled()
        {
            DraftTriggers.OnOuterDraftReroll();
        }
    }
}

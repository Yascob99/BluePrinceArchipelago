namespace BluePrinceArchipelago.FsmMethods.CustomMethods {
    public class ZeroStepEnding : RegisteredCustomFsmMethod
    {

        public new string Name { get; set; } = "ZeroStepEnding";

        public override void OnCalled()
        {
            Plugin.ArchipelagoClient.DeathLinkHandler.SendStepsDeathLink();
        }

    }
}
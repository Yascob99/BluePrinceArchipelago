using BluePrinceArchipelago.Archipelago;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods {
    public class ZeroStepEnding : RegisteredCustomFsmMethod
    {

        public new string Name { get; set; } = "ZeroStepEnding";


        public override void OnCalled()
        {
            if (!DeathLinkHandler.processingDeathLink)
            {
                Plugin.ArchipelagoClient.DeathLinkHandler.SendStepsDeathLink();
            }
        }

    }
}
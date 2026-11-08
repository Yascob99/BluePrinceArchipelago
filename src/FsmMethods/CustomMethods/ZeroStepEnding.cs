using BluePrinceArchipelago.Archipelago;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods {
    public class ZeroStepEnding : RegisteredCustomFsmMethod
    {

        public new string Name { get; set; } = "ZeroStepEnding";


        public override void OnCalled()
        {
            // Prevent the trigger if a DeathLink is being processed.
            if (!DeathLinkHandler.processingDeathLink)
            {
                Plugin.ArchipelagoClient.DeathLinkHandler.SendStepsDeathLink();
            }
        }

    }
}
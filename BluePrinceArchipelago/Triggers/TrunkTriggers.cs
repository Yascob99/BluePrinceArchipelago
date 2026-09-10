namespace BluePrinceArchipelago.Triggers
{
    public static class TrunkTriggers
    {
        public static void OnTrunkOpened() {
            ModInstance.TrunkManager.OnTrunkOpen();
        }
    }
}

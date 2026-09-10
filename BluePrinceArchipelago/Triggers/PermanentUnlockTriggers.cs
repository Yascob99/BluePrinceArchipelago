using BluePrinceArchipelago.Items;

namespace BluePrinceArchipelago.Triggers
{
    /// <summary>
    ///     Triggers Related to Permanent unlocks.
    /// </summary>
    public static class PermanentUnlockTriggers
    {
        /// <summary>
        ///     Triggers when the Apple Orchard is Unlocked.
        /// </summary>
        public static void OnAppleOrchardUnlock() {
            Unlocks.AppleOrchard.FoundLocation();
        }

        /// <summary>
        ///     Triggers when the Gemstone Caverns is Unlocked.
        /// </summary>
        public static void OnGemstoneCavernsUnlock()
        {
            Unlocks.GemstoneCaverns.FoundLocation();
        }

        /// <summary>
        ///     Triggers when the Blackbridge Grotoo is Unlocked.
        /// </summary>
        public static void OnBlackBridgeGrottoUnlock()
        {
            Unlocks.BlackBridgeGrotto.FoundLocation();
        }

        /// <summary>
        ///     Triggers when the Blackbridge Grotoo is Unlocked.
        /// </summary>
        public static void OnWestGatePathUnlock()
        {
            Unlocks.WestGatePath.FoundLocation();
        }

        /// <summary>
        ///     Triggers when the Blackbridge Grotoo is Unlocked.
        /// </summary>
        public static void OnSatelliteRaised()
        {
            Unlocks.SatelliteDish.FoundLocation();
        }
    }


}

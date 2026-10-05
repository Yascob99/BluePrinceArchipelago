using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Utils;
using System;

namespace BluePrinceArchipelago.Items
{
	/// <summary>
	/// Manage Simon's daily resources. This includes gems, keys, gold, steps, and dice. 
	/// </summary>
	public static class ResourceManager
	{
		/// <summary>
		/// Add or remove Gems from Simon's resources.
		/// </summary>
		/// <param name="count">The number to add (positive) or remove (negative).</param>
		public static void AdjustGems(int count)
		{
			try
			{
				ModInstance.GemManager.FindIntVariable("Gem Adjustment Amount").Value = count;
				ModInstance.GemManager.SendEvent("Update with Sound");
			}
			catch
			{
				Logging.Logger.LogWarning("Error adjusting Gems");
			}

        }
        /// <summary>
        /// Add or remove Gold from Simon's resources.
        /// </summary>
		/// <param name="count">The number to add (positive) or remove (negative).</param>
        public static void AdjustGold(int count)
        {
            try
            {
                ModInstance.GoldManager.FindIntVariable("Adjustment Amount").Value = count;
                ModInstance.GoldManager.SendEvent("Update");
            }
            catch
            {
                Logging.Logger.LogWarning("Error adjusting Gold");
            }

        }
        /// <summary>
        /// Add or remove Dice from Simon's resources.
        /// </summary>
		/// <param name="count">The number to add (positive) or remove (negative).</param>
        public static void AdjustDice(int count)
        {
            try
            {
                ModInstance.DiceManager.FindIntVariable("Adjustment Amount").Value = count;
                ModInstance.DiceManager.SendEvent("Update");
            }
            catch
            {
                Logging.Logger.LogWarning("Error adjusting Dice");
            }

        }
        /// <summary>
        /// Add or remove Steps from Simon's resources.
        /// </summary>
		/// <param name="count">The number to add (positive) or remove (negative).</param>
        public static void AdjustSteps(int count)
        {
            try
            {
                ModInstance.StepManager.FindIntVariable("Adjustment Amount").Value = count;
                ModInstance.StepManager.SendEvent("Update");
            }
            catch
            {
                Logging.Logger.LogWarning("Error adding Steps");
            }

        }
        /// <summary>
        /// Add or remove Keys from Simon's resources.
        /// </summary>
        /// <param name="count">The number to add (positive) or remove (negative).</param>
        public static void AdjustKeys(int count)
        {
            try
            {
                ModInstance.KeyManager.FindIntVariable("Adjustment Amount").Value = count;
                ModInstance.KeyManager.SendEvent("Update");
            }
            catch
            {
                Logging.Logger.LogWarning("Error adding Keys");
            }

        }

        /// <summary>
        /// Used in the DEBUG build configuration to give Simon many resources at start of day.
        /// Modify this if Simon also needs Items or anything else for your testing.
        /// </summary>
        public static void GodMode()
        {
            AdjustGems(100);
            AdjustGold(500);
            AdjustSteps(500);
            AdjustDice(200);
            AdjustKeys(100);
        }
    }
}
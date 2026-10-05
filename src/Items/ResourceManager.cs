using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Utils;
using System;
using System.Xml.Linq;
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
	/// <summary>
	/// Manage Simon's daily resources. 
    /// This includes gems, keys, gold, steps, dice, stars, allowance, and luck. 
	/// </summary>
	public static class ResourceManager
	{
		/// <summary>
		/// Add or remove Gems from Simon's resources.
		/// </summary>
		/// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
		public static bool AdjustGems(int count)
		{
			try
			{
				ModInstance.GemManager.FindIntVariable("Gem Adjustment Amount").Value += count;
				ModInstance.GemManager.SendEvent("Update with Sound");
                return true;
            }
			catch
			{
				Logging.Logger.LogWarning("Error adjusting Gems");
                return false;
            }

        }
        /// <summary>
        /// Add or remove Gold from Simon's resources.
        /// </summary>
		/// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public static bool AdjustGold(int count)
        {
            try
            {
                ModInstance.GoldManager.FindIntVariable("Adjustment Amount").Value += count;
                ModInstance.GoldManager.SendEvent("Update");
                return true;
            }
            catch
            {
                Logging.Logger.LogWarning("Error adjusting Gold");
                return false;
            }

        }
        /// <summary>
        /// Add or remove Dice from Simon's resources.
        /// </summary>
		/// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public static bool AdjustDice(int count)
        {
            try
            {
                ModInstance.DiceManager.FindIntVariable("Adjustment Amount").Value += count;
                ModInstance.DiceManager.SendEvent("Update");
                return true;
            }
            catch
            {
                Logging.Logger.LogWarning("Error adjusting Dice");
                return false;
            }

        }
        /// <summary>
        /// Add or remove Steps from Simon's resources.
        /// </summary>
		/// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public static bool AdjustSteps(int count)
        {
            try
            {
                ModInstance.StepManager.FindIntVariable("Adjustment Amount").Value += count;
                ModInstance.StepManager.SendEvent("Update");
                return true;
            }
            catch
            {
                Logging.Logger.LogWarning("Error adjusting Steps");
                return false;
            }

        }
        /// <summary>
        /// Add or remove Keys from Simon's resources.
        /// </summary>
        /// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public static bool AdjustKeys(int count)
        {
            try
            {
                ModInstance.KeyManager.FindIntVariable("Adjustment Amount").Value += count;
                ModInstance.KeyManager.SendEvent("Update");
                return true;
            }
            catch
            {
                Logging.Logger.LogWarning("Error adjusting Keys");
                return false;
            }

        }

        /// <summary>
        /// Add or remove Stars from Simon's resources.
        /// </summary>
        /// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public static bool AdjustStars(int count)
        {
            try
            {
                int totalStars = ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value;
                if (totalStars + count > 0)
                {
                    ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value += count;
                }
                else
                {
                    ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value = 0;
                }
                ModInstance.StarManager.SendEvent("Update");
                return true;
            }
            catch
            {
                Logging.Logger.LogWarning("Error adjusting Keys");
                return false;
            }

        }
        /// <summary>
        /// Add or remove Luck from Simon's resources.
        /// </summary>
        /// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public static bool AdjustLuck(int count)
        {
            try
            {
                int luck = ModInstance.LuckManager.FindIntVariable("LUCK").Value;
                if (luck + count > 0)
                {
                    ModInstance.LuckManager.FindIntVariable("LUCK").Value += count;
                }
                else
                {
                    ModInstance.LuckManager.FindIntVariable("Luck").Value = 0;
                }
                return true;
            }
            catch
            {
                Logging.Logger.LogWarning("Error adjusting Luck");
                return false;
            }

        }
        /// <summary>
        /// Add or remove Allowance from Simon's resources.
        /// </summary>
        /// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public static bool AdjustAllowance(int count)
        {
            try
            {
                int totalAllowance = ModInstance.GlobalPersistentManager.GetIntVariable("allowance").Value;
                if (totalAllowance + count > 0)
                {
                    ModInstance.GlobalPersistentManager.GetIntVariable("allowance").Value += count;
                }
                else
                {
                    ModInstance.GlobalPersistentManager.GetIntVariable("allowance").Value = 0;
                }
                return true;
            }
            catch
            {
                Logging.Logger.LogWarning("Error adjusting Allowance");
                return false;
            }
        }
        /// <summary>
        /// Validate if the entered resource type is valid. Useful for input validation, like the console. 
        /// </summary>
        /// <param name="type">The resource type</param>
        /// <returns>True if a valid type, otherwise false.</returns>
        public static bool IsValidResourceType(string type)
        {
            switch (type.ToLower())
            {
                case "gems":
                case "gold":
                case "steps":
                case "keys":
                case "dice":
                case "stars":
                case "luck":
                case "allowance":
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// Add or remove resources of a given type from Simon.
        /// </summary>
        /// <param name="type">The resource type to add or remove</param>
        /// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public static bool AdjustResource(string type, int count)
        {
            switch (type.ToLower())
            {
                case "gems":
                    return ResourceManager.AdjustGems(count);
                case "gold":
                    return ResourceManager.AdjustGold(count);
                case "steps":
                    return ResourceManager.AdjustSteps(count);
                case "keys":
                    return ResourceManager.AdjustKeys(count);
                case "dice":
                    return ResourceManager.AdjustDice(count);
                case "stars":
                    return ResourceManager.AdjustStars(count);
                case "luck":
                    return ResourceManager.AdjustLuck(count);
                case "allowance":
                    return ResourceManager.AdjustAllowance(count);
                default:
                    Logging.Logger.LogWarning($"Error adjusting unknown resource type: {type}");
                    return false;

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
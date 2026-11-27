using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;

namespace BluePrinceArchipelago.Resources
{
	/// <summary>
	/// Manage Simon's daily resources. 
    /// This includes gems, keys, gold, steps, dice, stars, allowance, and luck. 
	/// </summary>
	public static class ResourceManager
	{

        public static int StartingGems = 0;
        public static int StartingKeys = 0;
        public static int StartingDice = 0;
        public static int StartingSteps = 0;
        public static int StartingLuck = 0;

        /// <summary>
        ///     Recalculates the amount of starting items the player should have.
        /// </summary>
        public static void RecalculculateStartingItems() {
            StartingGems = 0;
            StartingKeys = 0;
            StartingDice = 0;
            StartingSteps = 0;
            StartingLuck = 0;
            if (ModItemManager.PermanentItemList.Count > 0)
            {
                foreach (PermanentItem item in ModItemManager.PermanentItemList) {
                    item.UnlockedCount = 0;
                }
                foreach (string itemname in ArchipelagoClient.ServerData.ReceivedItems)
                {
                    if (itemname.ToLower().Contains("permanent"))
                    {
                        Logging.LogWarning(itemname);
                        PermanentItem item = ModItemManager.GetPermanentItem(itemname);
                        item.UnlockedCount++;
                    } 
                }
                foreach (PermanentItem item in ModItemManager.PermanentItemList)
                {
                    switch (item.ItemType)
                    {
                        case "Gems":
                            StartingGems += item.UnlockedCount * item.Count;
                            break;
                        case "Keys":
                            StartingKeys += item.UnlockedCount * item.Count;
                            break;
                        case "Dice":
                            StartingDice += item.UnlockedCount * item.Count;
                            break;
                        case "Steps":
                            StartingSteps += item.UnlockedCount * item.Count;
                            break;
                        case "Luck":
                            StartingLuck += item.UnlockedCount * item.Count;
                            break;
                    }

                }

            }
        }

        public static void AddStartingItems() {
            Logging.LogWarning($"Gems: {StartingGems}");
            AdjustGems(StartingGems);
            Logging.LogWarning($"Keys: {StartingKeys}");
            AdjustKeys(StartingKeys);
            Logging.LogWarning($"Dice: {StartingDice}");
            AdjustDice(StartingDice);
            Logging.LogWarning($"Steps: {StartingSteps}");
            AdjustSteps(StartingSteps);
            Logging.LogWarning($"Luck: {StartingLuck}");
            AdjustLuck(StartingLuck);
        }

        /// <summary>
        /// Add or remove Gems from Simon's resources.
        /// </summary>
        /// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public static void AdjustGems(int count)
		{
            if (ModInstance.IsInRun) {
                GemResourceManager.AdjustGems(count);
            }

        }
        /// <summary>
        /// Add or remove Gold from Simon's resources.
        /// </summary>
		/// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public static void AdjustGold(int count)
        {
            if (ModInstance.IsInRun)
            {
                GoldResourceManager.AdjustGold(count);
            }
        }
        /// <summary>
        /// Add or remove Dice from Simon's resources.
        /// </summary>
		/// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public static void AdjustDice(int count)
        {
            if (ModInstance.IsInRun)
            {
                DiceResourceManager.AdjustDice(count);
            }

        }
        /// <summary>
        /// Add or remove Steps from Simon's resources.
        /// </summary>
		/// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public static void AdjustSteps(int count)
        {
            if (ModInstance.IsInRun)
            {
                StepResourceManager.AdjustSteps(count);
            }
        }
       
        /// <summary>
        /// Add or remove Keys from Simon's resources.
        /// </summary>
        /// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public static void AdjustKeys(int count)
        {
            if (ModInstance.IsInRun)
            {
                KeyResourceManager.AdjustKeys(count);
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
                Logging.LogWarning("Error adjusting Stars", "Resources");
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

                Logging.LogWarning("Error adjusting Luck", "Resources");
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
                Logging.LogWarning("Error adjusting Allowance");
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
        public static void AdjustResource(string type, int count)
        {
            switch (type.ToLower())
            {
                case "gems":
                    AdjustGems(count);
                    break;
                case "gold":
                    AdjustGold(count);
                    break;
                case "steps":
                    AdjustSteps(count);
                    break;
                case "keys":
                    AdjustKeys(count);
                    break;
                case "dice":
                    AdjustDice(count);
                    break;
                case "stars":
                    AdjustStars(count);
                    break;
                case "luck":
                    AdjustLuck(count);
                    break;
                case "allowance":
                    AdjustAllowance(count);
                    break;
                default:
                    Logging.Logger.LogWarning($"Error adjusting unknown resource type: {type}");
                    break;

            }
        }
        /// <summary>
        ///     Returns the current total of a resource, or -1 if it could not be found.
        /// </summary>
        /// <param name="type">The type of resource to get the total of.</param>
        /// <returns>The total of that resource. -1 if not found.</returns>
        public static int GetResourceCount(string type)
        {
            switch (type.ToLower())
            {
                case "gems":
                    return ModInstance.GemManager.GetIntVariable("GEMS").Value;
                case "gold":
                    return ModInstance.GoldManager.GetIntVariable("GOLD").Value;
                case "steps":
                    return ModInstance.StepManager.GetIntVariable("STEPS").Value;
                case "keys":
                    return ModInstance.KeyManager.GetIntVariable("KEYS").Value;
                case "dice":
                    return ModInstance.DiceManager.GetIntVariable("BONES").Value;
                case "stars":
                    return ModInstance.GlobalPersistentManager.GetIntVariable("Total Stars").Value;
                case "luck":
                    return ModInstance.LuckManager.GetIntVariable("LUCK").Value;
                case "allowance":
                    return ModInstance.GlobalPersistentManager.GetIntVariable("allowance").Value;
                default:
                    Logging.Logger.LogWarning($"Error getting count for unknown resource type: {type}");
                    return -1;

            }
        }


        public static void ReplaceResourceManagers() { 
            
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
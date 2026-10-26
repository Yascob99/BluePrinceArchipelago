using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Utils;
#if ML
using Il2CppTMPro;
#endif
#if Bep
using TMPro;
#endif
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    public abstract class ResourceHandler
    {
        private static int ComputeNewResourceCount(int baseResources, int addedResources, int upperLimit)
        {
            if (baseResources + addedResources < 0)
            {
                return 0;
            }
            if (baseResources + addedResources > upperLimit)
            {
                return upperLimit;
            }
            return baseResources + addedResources;
        }

        public bool AdjustResource(int addedResources)
        {
            if (IsResourceLocked())
            {
                return false;
            }
            UpdateResources(addedResources);
            UpdateUI();
            return true;
        }

        public void UpdateResources(int addedResources)
        {
            int currentResources = GetResourceCount();
            int newResourceTotal = ComputeNewResourceCount(currentResources, addedResources, GetUpperLimit());
            SetResourceCount(newResourceTotal);
        }

        public abstract int GetResourceCount();

        public abstract void SetResourceCount(int resourceCount);

        public virtual bool IsResourceLocked()
        {
            return false;
        }
        public virtual int GetUpperLimit()
        {
            return 1000000;
        }

        public virtual void UpdateUI() { }
    }

    public class GemHandler : ResourceHandler
    {
        private const int GEM_COUNT_UPPER_LIMIT = 1000000;

        public override bool IsResourceLocked()
        {
            if (ModInstance.GemManager.GetBoolVariable("GEM LOCK").Value)
            {
                Logging.LogWarning("Gems are locked, cannot adjust", "Resources");
                return true;
            }
            return false;
        }

        public override int GetResourceCount()
        {
            return ModInstance.GemManager.GetIntVariable("GEMS").Value;
        }

        public override void SetResourceCount(int resourceCount)
        {
            ModInstance.GemManager.GetIntVariable("GEMS").Value = resourceCount;
        }

        public override int GetUpperLimit()
        {
            return GEM_COUNT_UPPER_LIMIT;
        }

        public override void UpdateUI()
        {
            GameObject GemIcon = GameObject.Find("__SYSTEM/HUD/Gems/Gems Icon");
            GemIcon.transform.Find("Gem #").GetComponent<TextMeshPro>().text = GetResourceCount().ToString();
            if (GemIcon.active == false)
            {
                GemIcon.SetActive(true);
            }
        }
    }

    public class GoldHandler : ResourceHandler
    {
        public override int GetResourceCount()
        {
            return ModInstance.GoldManager.GetIntVariable("GOLD").Value;
        }

        public override int GetUpperLimit()
        {
            return 1000000;
        }

        public override bool IsResourceLocked()
        {
            return ModInstance.GoldManager.GetBoolVariable("GOLD LOCK").Value;
        }

        public override void SetResourceCount(int resourceCount)
        {
            ModInstance.GoldManager.GetIntVariable("GOLD").Value = resourceCount;
        }

        public override void UpdateUI()
        {
            GameObject GoldIcon = GameObject.Find("__SYSTEM/HUD/Gold/Gold Icon");
            GoldIcon.transform.Find("Gold #").GetComponent<TextMeshPro>().text = GetResourceCount().ToString();
            if (GoldIcon.active == false)
            {
                GoldIcon.SetActive(true);
            }
        }
    }

    public class DiceHandler : ResourceHandler
    {
        public override int GetResourceCount()
        {
            return ModInstance.DiceManager.GetIntVariable("BONES").Value;
        }

        public override int GetUpperLimit()
        {
            return 1000000;
        }

        public override void SetResourceCount(int resourceCount)
        {
            ModInstance.DiceManager.GetIntVariable("BONES").Value = resourceCount;
        }

        public override void UpdateUI()
        {
            GameObject BoneIcon = GameObject.Find("__SYSTEM/HUD/Bones/Bones Icon");
            GameObject BoneNum = BoneIcon.transform.Find("Bone #").gameObject;
            TextMeshPro BoneTMP = BoneNum.GetComponent<TextMeshPro>();
            // Set the Dice Icon's local scale
            BoneNum.transform.localScale = new Vector3(1.3652f, 1.3652f, 1.3652f);
            // Set the Dice text's color
            BoneTMP.color = new Color(1f, 1f, 1f, 1f);
            // Set the Dice Icon and text to active 

            // if the set the value to 0 then hide dice UI
            if (GetResourceCount() == 0)
            {
                BoneIcon.SetActive(false);
            }
            else
            {
                // Show the Dice UI
                BoneIcon.SetActive(true);
            }

            // Set text to the correct amount.
            BoneTMP.text = GetResourceCount().ToString();
        }
    }

    public class StepHandler : ResourceHandler
    {
        public override int GetResourceCount()
        {
            return ModInstance.StepManager.GetIntVariable("Steps").Value;
        }

        public override int GetUpperLimit()
        {
            return 10000;
        }

        public override bool IsResourceLocked()
        {
            if (ModInstance.StepManager.GetBoolVariable("STEP LOCK").Value)
            {
                Logging.LogWarning("Steps are locked, cannot adjust", "Resources");
                return true;
            }
            return false;
        }

        public override void SetResourceCount(int resourceCount)
        {
            ModInstance.StepManager.GetIntVariable("Steps").Value = resourceCount;
        }

        public override void UpdateUI()
        {
            GameObject.Find("__SYSTEM/HUD/Steps/Steps Icon/EatPopup/Steps #").GetComponent<TextMeshPro>().text = GetResourceCount().ToString();
        }

        public static void ForceAdjust(int resourceCount)
        {
            ModInstance.StepManager.FindIntVariable("Adjustment Amount").Value += resourceCount;
            ModInstance.StepManager.SendEvent("Update");
        }
    }

    public class KeyHandler : ResourceHandler
    {
        public override bool IsResourceLocked()
        {
            if (ModInstance.KeyManager.GetBoolVariable("KEY LOCK").Value)
            {
                Logging.LogWarning("Keys are locked, cannot adjust", "Resources");
                return true;
            }
            return false;
        }
        public override int GetResourceCount()
        {
            return ModInstance.KeyManager.GetIntVariable("KEYS").Value;
        }

        public override int GetUpperLimit()
        {
            return 1000000;
        }

        public override void SetResourceCount(int resourceCount)
        {
            ModInstance.KeyManager.GetIntVariable("KEYS").Value = resourceCount;
        }

        public override void UpdateUI()
        {
            GameObject KeyIcon = GameObject.Find("__SYSTEM/HUD/Keys/Keys Icon");
            KeyIcon.transform.Find("Key #").GetComponent<TextMeshPro>().text = GetResourceCount().ToString();
            if (!KeyIcon.active)
            {
                KeyIcon.SetActive(true);
            }
        }
    }

    public class StarHandler : ResourceHandler
    {
        public override int GetResourceCount()
        {
            return ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value;
        }

        public override void SetResourceCount(int resourceCount)
        {
            ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value = resourceCount;
        }

        public override void UpdateUI()
        {
            ModInstance.StarManager.SendEvent("Update");
        }
    }

    public class LuckHandler : ResourceHandler
    {
        public override int GetResourceCount()
        {
            return ModInstance.LuckManager.FindIntVariable("LUCK").Value;
        }

        public override void SetResourceCount(int resourceCount)
        {
            ModInstance.LuckManager.FindIntVariable("LUCK").Value = resourceCount;
        }
    }

    public class AllowanceHandler : ResourceHandler
    {
        public override int GetResourceCount()
        {
            return ModInstance.GlobalPersistentManager.GetIntVariable("allowance").Value;
        }

        public override void SetResourceCount(int resourceCount)
        {
            ModInstance.GlobalPersistentManager.GetIntVariable("allowance").Value = resourceCount;
        }
    }

    /// <summary>
    /// Manage Simon's daily resources. 
    /// This includes gems, keys, gold, steps, dice, stars, allowance, and luck. 
    /// </summary>
    public sealed class ResourceManager
    {

        public static int StartingGems = 0;
        public static int StartingKeys = 0;
        public static int StartingDice = 0;
        public static int StartingSteps = 0;
        public static int StartingLuck = 0;

        private readonly GemHandler gemHandler = new();
        private readonly GoldHandler goldHandler = new();
        private readonly DiceHandler diceHandler = new();
        private readonly StepHandler stepHandler = new();
        private readonly KeyHandler keyHandler = new();
        private readonly StarHandler starHandler = new();
        private readonly LuckHandler luckHandler = new();
        private readonly AllowanceHandler allowanceHandler = new();

        //Singleton
        private ResourceManager() { }

        private static ResourceManager _instance;

        public static ResourceManager GetInstance()
        {
            _instance ??= new();
            return _instance;
        }

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
                    if (itemname.ToLower().Contains("starting"))
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

        public void GiveStartingItems() {
            RecalculculateStartingItems();
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
        public bool AdjustGems(int count)
        {
            try
            {
                return gemHandler.AdjustResource(count);
            }
            catch
            {
                Logging.LogWarning("Error adjusting Gems", "Resources");
                return false;
            }
        }
        /// <summary>
        /// Add or remove Gold from Simon's resources.
        /// </summary>
		/// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public bool AdjustGold(int count)
        {
            try
            {
                return goldHandler.AdjustResource(count);
            }
            catch
            {
                Logging.LogWarning("Error adjusting Gold", "Resources");
                return false;
            }

        }
        /// <summary>
        /// Add or remove Dice from Simon's resources.
        /// </summary>
		/// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public bool AdjustDice(int count)
        {
            try
            { 
                return diceHandler.AdjustResource(count);
            }
            catch
            {
                Logging.LogWarning("Error adjusting Dice", "Resources");
                return false;
            }
        }
        /// <summary>
        /// Add or remove Steps from Simon's resources.
        /// </summary>
		/// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <param name="setdirectly">Whether to set the steps directly or use the slow updater.</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public bool AdjustSteps(int count, bool setdirectly = false)
        {
            try
            {
                if (setdirectly){
                    return stepHandler.AdjustResource(count);
                }
                else
                {
                    StepHandler.ForceAdjust(count);
                    return true;
                }
            }
            catch
            {
                Logging.LogWarning("Error adjusting Steps", "Resources");
                return false;
            }

        }
        /// <summary>
        /// Add or remove Keys from Simon's resources.
        /// </summary>
        /// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public bool AdjustKeys(int count)
        {
            try
            {
                return keyHandler.AdjustResource(count);
            }
            catch
            {
                Logging.LogWarning("Error adjusting Keys", "Resources");
                return false;
            }

        }

        /// <summary>
        /// Add or remove Stars from Simon's resources.
        /// </summary>
        /// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public bool AdjustStars(int count)
        {
            try
            {
                return starHandler.AdjustResource(count);
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
        public bool AdjustLuck(int count)
        {
            try
            {
                return luckHandler.AdjustResource(count);
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
        public bool AdjustAllowance(int count)
        {
            try
            {
                return allowanceHandler.AdjustResource(count);
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
        public bool AdjustResource(string type, int count)
        {
            switch (type.ToLower())
            {
                case "gems":
                    return AdjustGems(count);
                case "gold":
                    return AdjustGold(count);
                case "steps":
                    return AdjustSteps(count);
                case "keys":
                    return AdjustKeys(count);
                case "dice":
                    return AdjustDice(count);
                case "stars":
                    return AdjustStars(count);
                case "luck":
                    return AdjustLuck(count);
                case "allowance":
                    return AdjustAllowance(count);
                default:
                    Logging.Logger.LogWarning($"Error adjusting unknown resource type: {type}");
                    return false;

            }
        }
        /// <summary>
        ///     Returns the current total of a resource, or -1 if it could not be found.
        /// </summary>
        /// <param name="type">The type of resource to get the total of.</param>
        /// <returns>The total of that resource. -1 if not found.</returns>
        public int GetResourceCount(string type)
        {
            switch (type.ToLower())
            {
                case "gems":
                    return gemHandler.GetResourceCount();
                case "gold":
                    return goldHandler.GetResourceCount();
                case "steps":
                    return stepHandler.GetResourceCount();
                case "keys":
                    return keyHandler.GetResourceCount();
                case "dice":
                    return diceHandler.GetResourceCount();
                case "stars":
                    return starHandler.GetResourceCount();
                case "luck":
                    return luckHandler.GetResourceCount();
                case "allowance":
                    return allowanceHandler.GetResourceCount();
                default:
                    Logging.Logger.LogWarning($"Error getting count for unknown resource type: {type}");
                    return -1;

            }
        }

        /// <summary>
        /// Used in the DEBUG build configuration to give Simon many resources at start of day.
        /// Modify this if Simon also needs Items or anything else for your testing.
        /// </summary>
        public void GodMode()
        {
            AdjustGems(100);
            AdjustGold(500);
            AdjustSteps(500);
            AdjustDice(200);
            AdjustKeys(100);
        }
    }
}
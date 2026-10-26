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

        public abstract bool IsResourceLocked();

        public abstract int GetResourceCount();

        public abstract void SetResourceCount(int resourceCount);

        public abstract int GetUpperLimit();

        public abstract void UpdateUI();
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
            GemIcon.transform.Find("Gem #").GetComponent<TextMeshPro>().text = ModInstance.GemManager.GetIntVariable("GEMS").Value.ToString();
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
            GoldIcon.transform.Find("Gold #").GetComponent<TextMeshPro>().text = ModInstance.GoldManager.GetIntVariable("GOLD").Value.ToString();
            if (GoldIcon.active == false)
            {
                GoldIcon.SetActive(true);
            }
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

        private GemHandler gemManager = new();
        private GoldHandler goldManager = new();

        //Singleton
        private ResourceManager() { }

        private static ResourceManager _instance;

        public static ResourceManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new();
            }
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

        public void AddStartingItems() {
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

        public static int ComputeNewResourceCount(int baseResources, int addedResources, int upperLimit) {
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

        /// <summary>
        /// Add or remove Gems from Simon's resources.
        /// </summary>
        /// <param name="count">The number to add (positive) or remove (negative).</param>
        /// <returns>True if the adjstment succeeded, otherwise false.</returns>
        public bool AdjustGems(int count)
        {
            try
            {
                return gemManager.AdjustResource(count);
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
                return goldManager.AdjustResource(count);
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
        public static bool AdjustDice(int count)
        {
            try
            {

                int Total = ModInstance.DiceManager.GetIntVariable("BONES").Value;
                // Int Clamp
                if (Total + count <= 0)
                {
                    return true;
                }
                if (Total + count > 1000000)
                {
                    ModInstance.DiceManager.GetIntVariable("BONES").Value = 1000000;
                }
                else
                {
                    ModInstance.DiceManager.GetIntVariable("BONES").Value += count;
                }

                GameObject BoneIcon = GameObject.Find("__SYSTEM/HUD/Bones/Bones Icon");
                GameObject BoneNum = BoneIcon.transform.Find("Bone #").gameObject;
                TextMeshPro BoneTMP = BoneNum.GetComponent<TextMeshPro>();
                // Set the Dice Icon's local scale
                BoneNum.transform.localScale = new Vector3(1.3652f, 1.3652f, 1.3652f);
                // Set the Dice text's color
                BoneTMP.color = new Color(1f, 1f, 1f, 1f);
                // Set the Dice Icon and text to active 

                // if the set the value to 0 then hide dice UI
                if (ModInstance.DiceManager.GetIntVariable("BONES").Value == 0)
                {
                    BoneIcon.SetActive(false);
                }
                else
                {
                    // Show the Dice UI
                    BoneIcon.SetActive(true);
                }
                
                // Set text to the correct amount.
                BoneTMP.text = ModInstance.DiceManager.GetIntVariable("BONES").Value.ToString();
                return true;
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
        public static bool AdjustSteps(int count, bool setdirectly = false)
        {
            try
            {
                if (setdirectly){
                    // Don't set Steps if Steps are locked.
                    if(ModInstance.StepManager.GetBoolVariable("STEP LOCK").Value)
                    {
                        Logging.LogWarning("Steps are locked, cannot adjust", "Resources");
                        return false;
                    }
                    int Total = ModInstance.StepManager.GetIntVariable("Steps").Value;

                    // Int Clamp
                    if (Total + count < 0)
                    {
                        ModInstance.StepManager.GetIntVariable("Steps").Value = 0;
                    }
                    if (Total + count > 10000)
                    {
                        ModInstance.StepManager.GetIntVariable("Steps").Value = 10000;
                    }
                    else
                    {
                        ModInstance.StepManager.GetIntVariable("Steps").Value += count;
                    }
                    // Set the UI
                    GameObject.Find("__SYSTEM/HUD/Steps/Steps Icon/EatPopup/Steps #").GetComponent<TextMeshPro>().text = ModInstance.StepManager.GetIntVariable("Steps").Value.ToString();
                    return true;
                }
                else
                {
                    ModInstance.StepManager.FindIntVariable("Adjustment Amount").Value += count;
                    ModInstance.StepManager.SendEvent("Update");
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
        public static bool AdjustKeys(int count)
        {
            try
            {
                // Don't set Keys if Keys are locked.
                if(ModInstance.KeyManager.GetBoolVariable("KEY LOCK").Value)
                {
                    Logging.LogWarning("Keys are locked, cannot adjust", "Resources");
                    return false;
                }
                int Total = ModInstance.KeyManager.GetIntVariable("KEYS").Value;

                // Int Clamp
                if (Total + count < 0)
                {
                    ModInstance.KeyManager.GetIntVariable("KEYS").Value = 0;
                }
                if (Total + count > 1000000)
                {
                    ModInstance.KeyManager.GetIntVariable("KEYS").Value = 1000000;
                }
                else
                {
                    ModInstance.KeyManager.GetIntVariable("KEYS").Value += count;
                }
                // Set the UI
                GameObject KeyIcon = GameObject.Find("__SYSTEM/HUD/Keys/Keys Icon");
                KeyIcon.transform.Find("Key #").GetComponent<TextMeshPro>().text = ModInstance.KeyManager.GetIntVariable("KEYS").Value.ToString();
                if (!KeyIcon.active)
                {
                    KeyIcon.SetActive(true);
                }
                return true;
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
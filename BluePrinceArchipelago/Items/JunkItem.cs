using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     A template for creating junk items for the mod. 
    /// </summary>
    /// <param name="name">The name of the item</param>
    /// <param name="gameObject">The gameobject of the item. Usually Null.</param>
    /// <param name="isUnlocked">If the item is unlocked.</param>
    /// <param name="itemType">The type of the Junk Item.</param>
    /// <param name="count">The count of the junk item.</param>
    public class JunkItem(string name, GameObject gameObject, bool isUnlocked, string itemType, int count = 1) : ModItem(name, gameObject, isUnlocked)
    {

        private string _ItemType = itemType;
        public string Itemtype
        {
            get { return _ItemType; }
            set { _ItemType = value; }
        }

        private int _Count = count;
        public new int Count
        {
            get { return _Count; }
            set
            {
                if (value > 0)
                {
                    _IsTrap = true; //Sets IsTrap dynamically (not sure that it's needed, but it's neat).
                }
                else
                {
                    _IsTrap = false; //Sets IsTrap dynamically (not sure that it's needed, but it's neat).
                }
                _Count = value;
            }
        }

        private bool _IsTrap = count < 0;
        public bool IsTrap
        {
            get { return _IsTrap; } //No setter since this is connected to count
        }

        public override void AddItemToInventory()
        {
            if (_ItemType == "Gems")
            {
                AdjustGems(_Count);
            }
            else if (_ItemType == "Steps")
            {
                AdjustSteps(_Count);
            }
            else if (_ItemType == "Gold")
            {
                AdjustGold(_Count);
            }
            else if (_ItemType == "Dice")
            {
                AdjustDice(_Count);
            }
            else if (_ItemType == "Keys")
            {
                AdjustKeys(_Count);
            }
            else if (_ItemType == "Luck")
            {
                AdjustLuck(_Count);
            }
            else if (_ItemType == "Stars")
            {
                AdjustStars(_Count);
            }
            else if (_ItemType == "Allowance")
            {
                AdjustAllowance(_Count);
            }
            else
            {
                Logging.LogWarning($"{_ItemType} is an invalid type, or is not currently supported.");
            }
        }

        /// <summary>
        ///     Adjusts the number of gems the player has.
        /// </summary>
        /// <param name="count">The number to adjust by.</param>
        private void AdjustGems(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.GemManager.FindIntVariable("Gem Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + count;
            // I think sound would be neat since it's more noticeable.
            ModInstance.GemManager.SendEvent("Update with Sound");
        }

        /// <summary>
        ///     Adjusts the number of steps the player has.
        /// </summary>
        /// <param name="count">The number to adjust by.</param>
        private void AdjustSteps(int count = 1)
        {
            // change the adjustment amount.
            FsmInt AdjustmentAmount = ModInstance.StepManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + count;
            // Send the "Update" event and the step counter should update.
            ModInstance.StepManager.SendEvent("Update");
        }

        /// <summary>
        ///     Adjusts the number of steps the player has.
        /// </summary>
        /// <param name="count">The number to adjust by.</param>
        private void AdjustGold(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.GoldManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + count;
            ModInstance.GoldManager.SendEvent("Update"); // Might need to be "Add Coins" instead.
        }

        /// <summary>
        ///     Adjusts the number of dice the player has.
        /// </summary>
        /// <param name="count">The number to adjust by.</param>
        private void AdjustDice(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.DiceManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + count;
            ModInstance.DiceManager.SendEvent("Update");
        }

        /// <summary>
        ///     Adjusts the number of keys the player has.
        /// </summary>
        /// <param name="count">The number to adjust by.</param>
        private void AdjustKeys(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.KeyManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + count;
            ModInstance.KeyManager.SendEvent("Update");
        }

        /// <summary>
        ///     Adjusts the amount of luck the player has.
        /// </summary>
        /// <param name="count">The number to adjust by.</param>
        private void AdjustLuck(int count = 1)
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
        }

        /// <summary>
        ///     Adjusts the amount of stars the player has.
        /// </summary>
        /// <param name="count">The number to adjust by.</param>
        private void AdjustStars(int count = 1)
        {
            int totalStars = ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value;
            if (totalStars + 1 > 0)
            {
                ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value += count;
            }
            else
            {
                ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value = 0;
            }
            ModInstance.StarManager.SendEvent("Update");
        }

        /// <summary>
        ///     Adjusts the ammount of allowance the player has.
        /// </summary>
        /// <param name="count">The number to adjust by.</param>
        private void AdjustAllowance(int count = 1)
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
        }
    }
}

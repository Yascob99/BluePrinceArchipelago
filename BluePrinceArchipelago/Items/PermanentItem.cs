using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     A template for creating permanent items (persistent junk items) for the mod.   
    /// </summary>
    /// <param name="name">The name of the item</param>
    /// <param name="gameObject">The gameobject of the item. Usually Null.</param>
    /// <param name="isUnlocked">If the item is unlocked.</param>
    /// <param name="itemType">The type of the Permanent Item.</param>
    /// <param name="count">The count of the permanent item.</param>
    public class PermanentItem(string name, GameObject gameObject, bool isUnlocked, string itemType, int count = 1) : ModItem(name, gameObject, isUnlocked)
    {
        private string _ItemType = itemType;
        public int UnlockedCount = 0;

        public string ItemType
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
                _Count = value;
            }
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
            else
            {
                Logging.LogWarning($"{_ItemType} is an invalid type, or is not currently supported.");
            }
        }

        /// <inheritdoc cref="JunkItem.AdjustGems(int)"/>
        private void AdjustGems(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.GemManager.FindIntVariable("Gem Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + (UnlockedCount * count);
            // I think sound would be neat since it's more noticeable.
            ModInstance.GemManager.SendEvent("Update with Sound");
        }
        /// <inheritdoc cref="JunkItem.AdjustSteps(int)"/>
        private void AdjustSteps(int count = 1)
        {
            // change the adjustment amount.
            FsmInt AdjustmentAmount = ModInstance.StepManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + (UnlockedCount * count);
            // Send the "Update" event and the step counter should update.
            ModInstance.StepManager.SendEvent("Update");
        }
        /// <inheritdoc cref="JunkItem.AdjustGold(int)"/>
        private void AdjustGold(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.GoldManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + (UnlockedCount * count);
            ModInstance.GoldManager.SendEvent("Update"); // Might need to be "Add Coins" instead.
        }
        /// <inheritdoc cref="JunkItem.AdjustDice(int)"/>
        private void AdjustDice(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.DiceManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + (UnlockedCount * count);
            ModInstance.DiceManager.SendEvent("Update");
        }
        /// <inheritdoc cref="JunkItem.AdjustKeys(int)"/>
        private void AdjustKeys(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.KeyManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + (UnlockedCount * count);
            ModInstance.KeyManager.SendEvent("Update");
        }
        /// <inheritdoc cref="JunkItem.AdjustLuck(int)"/>
        private void AdjustLuck(int count = 1)
        {
            int luck = ModInstance.LuckManager.FindIntVariable("LUCK").Value;
            if (luck + count > 0)
            {
                ModInstance.LuckManager.FindIntVariable("LUCK").Value = luck + (UnlockedCount * count);
            }
            else
            {
                ModInstance.LuckManager.FindIntVariable("Luck").Value = 0;
            }
        }
    }
}

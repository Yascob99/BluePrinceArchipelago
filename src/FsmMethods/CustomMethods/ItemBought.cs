using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Triggers;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    /// <summary>
    ///     An event for when a Unique Item is picked up.
    /// </summary>
    /// <param name="itemName">The name of the item that was picked up.</param>
    public class ItemBought(string itemName) : RegisteredCustomFsmMethod
    {
        public new string Name { get; set; } = CustomFsmMethodManager.GetItemBoughtMethodName(itemName);

        public GameUniqueItem Item { get; set; }

        private string ItemName { get; set; } = itemName;

        public override void OnCalled()
        {
            if (Item != null)
            {
                ItemTriggers.OnAfterItemPickup(Item);
            }
            else {
                Logging.Log($"The {Name} custom method couldn't be run due to invalid item: {ItemName}", "CustomFsmMethods");
            }
        }
        public override void Update()
        {
            Item = GameItemManager.GetInstance().GetGameUniqueItem(ItemName);
        }
    }
}

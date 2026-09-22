using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Triggers;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    /// <summary>
    ///     An event for when a Unique Item is Dug up.
    /// </summary>
    /// <param name="itemName">The name of the item that was picked up.</param>
    public class ItemDugUp(string itemName) : RegisteredCustomFsmMethod
    {
        public new string Name { get; set; } = CustomFsmMethodManager.GetItemDugUpMethodName(itemName);

        public UniqueItem Item { get; set; }

        public string ItemName { get; set; } = itemName;

        public override void OnRegister()
        {
            Item = ModItemManager.GetUniqueItem(ItemName);
        }
        public override void OnCalled()
        {
            if (Item != null)
            {
                ItemTriggers.OnItemDugUp(Item);
            }
            else
            {
                Logging.Log($"The {Name} custom method couldn't be run due to invalid item: {ItemName}", "CustomFsmMethods");
            }
        }
        public override void Update()
        {
            Item = ModItemManager.GetUniqueItem(ItemName);
        }
    }
}

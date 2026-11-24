using BluePrinceArchipelago.Resources;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     A template for creating junk items for the mod. 
    /// </summary>
    /// <param name="name">The name of the item</param>
    /// <param name="isUnlocked">If the item is unlocked.</param>
    /// <param name="itemType">The type of the Junk Item.</param>
    /// <param name="resourceAmount">The count of the junk item.</param>
    /// <param name="isPermament"></param>
    public class ResourceItem(string name, bool isUnlocked, string itemType, int resourceAmount = 1, bool isPermament = false) : ArchipelagoItem(name, isUnlocked)
    {
        private readonly string _itemType = itemType;
        private readonly int _resourceAmount = resourceAmount;
        private readonly bool _isPermament = isPermament;

        public override void AddToWorld()
        {
            Logging.Log($"{Name} gives {_resourceAmount * _count} {_itemType}");
            ResourceManager.AdjustResource(_itemType, _resourceAmount * _count);
        }

        public override bool IsPermanent()
        {
            return _isPermament;
        }
    }
}

using BluePrinceArchipelago.Archipelago;
using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
        /// <summary>
    ///     The enum for the Sanity Types.
    /// </summary>
	public enum ItemSanityType
	{
		None,
		Standard,
		Workshop,
		UpgradeDisk,
		Key,
		SpecialShop,
	}

	public class ArchipelagoUniqueItem : ArchipelagoItem
    {
        private readonly ItemSanityType _sanityType;

        private readonly GameItem _gameItem;

        /// <summary>
        ///     Creates a UniqueItem
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="isUnlocked">If the item is unlocked.</param>
        /// <param name="sanityType"></param>
        public ArchipelagoUniqueItem(string name, ItemSanityType sanityType = ItemSanityType.None, bool isUnlocked = false) : base(name, isUnlocked)
        {
            _gameItem = GameItemManager.GetInstance().GetGameUniqueItem(name);
            _sanityType = sanityType;
            if (!InPlay())
            {
                _gameItem.NotInMultiworld();
            }
        }

        private bool InPlay()
        {
            return IsSanityLocked();
        }

        public override void StackItem()
        {
            Logging.LogError($"Error attempting to stack unique items for : {Name}");
        }

        protected override void UnlockHook()
        {
            _gameItem.ArchipelagoUnlocked();
        }

        /// <summary>
        ///     Handles adding the item to the inventory.
        /// </summary>
        public override void AddToWorld()
        {
            if (InPlay())
            {
                _gameItem.AddToWorld();
            } 
            else
            {
                Logging.LogWarning($"Sanity deactivated for {Name}, not giving the item.");
            }
        }

		private bool IsSanityLocked()
		{
			return _sanityType switch
			{
				ItemSanityType.None => false,
				ItemSanityType.Standard => ArchipelagoOptions.StandardItemSanity,
				ItemSanityType.Workshop => ArchipelagoOptions.WorkshopSanity,
				ItemSanityType.UpgradeDisk => ArchipelagoOptions.UpgradeDiskSanity,
				ItemSanityType.Key => ArchipelagoOptions.KeySanity,
				_ => false,
			};
		}
	}
}
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

	public class ArchipelagoUniqueItem : ModItem
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
            if (isUnlocked || !IsSanityLocked())
            {
                _gameItem.ArchipelagoUnlocked();
            }
        }


        /// <summary>
        ///     Handles adding the item to the inventory.
        /// </summary>
        public override void AddToWorld()
        {
            _gameItem.AddToWorld();
        }

		public bool IsSanityLocked()
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
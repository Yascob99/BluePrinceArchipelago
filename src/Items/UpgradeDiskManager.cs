using System.Collections.Generic;

namespace BluePrinceArchipelago.Items
{
	/// <summary>
	///     A manager for common operations associated with Unique Items.
	/// </summary>
	public static class UpgradeDiskManager
	{
		private static readonly Dictionary<string, UpgradeDisksGameItem> _upgradeDisks = new()
		{
			{ITEM.UPGRADE_DISK_ARCHIVES, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_ARCHIVES)},
			{ITEM.UPGRADE_DISK_TRADING_POST_DYNAMITE, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_TRADING_POST_DYNAMITE)},
			{ITEM.UPGRADE_DISK_TOMB, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_TOMB)},
			{ITEM.UPGRADE_DISK_COMMISSARY, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_COMMISSARY)},
			{ITEM.UPGRADE_DISK_FOUNDATION, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_FOUNDATION)},
			{ITEM.UPGRADE_DISK_FREEZER, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_FREEZER)},
			{ITEM.UPGRADE_DISK_GARAGE, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_GARAGE)},
			{ITEM.UPGRADE_DISK_GREAT_HALL, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_GREAT_HALL)},
			{ITEM.UPGRADE_DISK_LOST_AND_FOUND, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_LOST_AND_FOUND)},
			{ITEM.UPGRADE_DISK_HER_LADYSHIPS_CHAMBER, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_HER_LADYSHIPS_CHAMBER)},
			{ITEM.UPGRADE_DISK_MECHANARIUM, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_MECHANARIUM)},
			{ITEM.UPGRADE_DISK_MORNING_ROOM, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_MORNING_ROOM)},
			{ITEM.UPGRADE_DISK_OFFICE, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_OFFICE)},
			{ITEM.UPGRADE_DISK_TRADING_POST_TRADE, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_TRADING_POST_TRADE)},
			{ITEM.UPGRADE_DISK_VAULT, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_VAULT)},
			{ITEM.UPGRADE_DISK_ABANDONED_MINE, new UpgradeDisksGameItem(ITEM.UPGRADE_DISK_ABANDONED_MINE)},
		};

		public static UpgradeDisksGameItem GetUpgradeDisk(string name)
		{
			return _upgradeDisks[name];
		}
	}
}
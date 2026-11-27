using BluePrinceArchipelago.Utils;
namespace BluePrinceArchipelago
{
    /// <summary>
    /// Utils class that stores item names and public methods to get alternative names used in-game.
    /// </summary>
    public class ITEM
    {
        public const string BASEMENT_KEY = "BASEMENT KEY";
        public const string BATTERY_PACK = "BATTERY PACK";
        public const string BROKEN_LEVER = "BROKEN LEVER";
        public const string BURNING_GLASS = "Burning Glass";
        public const string CABINET_KEY_PATIO = "CABINET KEY 1";
        public const string CABINET_KEY_LAUNDRY = "CABINET KEY 2";
        public const string CABINET_KEY_TUNNEL = "CABINET KEY 3";
        public const string CAR_KEYS = "CAR KEYS";
        public const string CHRONOGRAPH = "CHRONOGRAPH";
        public const string COIN_PURSE = "COIN PURSE";
        public const string COMPASS = "COMPASS";
        public const string COUPON_BOOK = "COUPON BOOK";
        public const string CROWN = "CROWN";
        public const string CURSED_EFFIGY = "CURSED EFFIGY";
        public const string DIARY_KEY = "DIARY KEY";
        public const string DETECTOR_SHOVEL = "Detector Shovel";
        public const string DOWSING_ROD = "Dowsing Rod";
        public const string EMERALD_BRACELET = "EMERALD BRACELET";
        public const string ELECTROMAGNET = "Electromagnet";
        public const string GEAR_WRENCH = "GEAR WRENCH";
        public const string HALL_PASS = "HALL PASS";
        public const string JACK_HAMMER = "Jack Hammer";
        public const string KEY_8 = "KEY 8";
        public const string KEY_OF_ARIES = "KEY of Aries";
        public const string KEYCARD = "KEYCARD";
        public const string KNIGHTS_SHIELD = "KNIGHTS SHIELD";
        public const string LOCK_PICK_KIT = "LOCK PICK KIT";
        public const string LUCKY_RABBITS_FOOT = "LUCKY RABBIT'S FOOT";
        public const string LUNCH_BOX = "LUNCH BOX";
        public const string LUCKY_PURSE = "Lucky Purse";
        public const string MAGNIFYING_GLASS = "MAGNIFYING GLASS";
        public const string MASTER_KEY = "MASTER KEY";
        public const string METAL_DETECTOR = "METAL DETECTOR";
        public const string MICROCHIP_SMALL = "MICROCHIP 1";
        public const string MICROCHIP_MIDDLE_SIZED = "MICROCHIP 2";
        public const string MICROCHIP_LARGE = "MICROCHIP 3";
        public const string MOON_PENDANT = "MOON PENDANT";
        public const string MORNING_STAR = "MORNING STAR";
        public const string ORNATE_COMPASS = "ORNATE COMPASS";
        public const string PAPER_CROWN = "PAPER CROWN";
        public const string PRISM_KEY = "PRISM KEY_0";
        public const string PICK_SOUND_AMPLIFIER = "Pick Sound Amplifier";
        public const string POWER_HAMMER = "Power Hammer";
        public const string REPELLENT = "REPELLENT";
        public const string RUNNING_SHOES = "RUNNING SHOES";
        public const string SALT_SHAKER = "SALT SHAKER";
        public const string SECRET_GARDEN_KEY = "SECRET GARDEN KEY";
        public const string SHOVEL = "SHOVEL";
        public const string SILVER_KEY = "SILVER KEY";
        public const string SILVER_SPOON = "SILVER SPOON";
        public const string SLEDGE_HAMMER = "SLEDGE HAMMER";
        public const string SLEEPING_MASK = "SLEEPING MASK";
        public const string STOPWATCH = "STOPWATCH";
        public const string TELESCOPE = "TELESCOPE";
        public const string THE_AXE = "THE AXE";
        public const string TORCH = "TORCH";
        public const string TREASURE_MAP = "TREASURE MAP";
        public const string VAULT_KEY_149 = "VAULT KEY 149";
        public const string VAULT_KEY_233 = "VAULT KEY 233";
        public const string VAULT_KEY_304 = "VAULT KEY 304";
        public const string VAULT_KEY_370 = "VAULT KEY 370";
        public const string WATERING_CAN = "WATERING CAN";

        public const string UPGRADE_DISK_ARCHIVES = "ARCHIVES";
		public const string UPGRADE_DISK_TRADING_POST_DYNAMITE = "TRADING POST DYNAMITE";
		public const string UPGRADE_DISK_TOMB = "TOMB";
		public const string UPGRADE_DISK_COMMISSARY = "COMMISSARY";
		public const string UPGRADE_DISK_FOUNDATION = "FOUNDATION";
		public const string UPGRADE_DISK_FREEZER = "FREEZER";
		public const string UPGRADE_DISK_GARAGE = "GARAGE";
		public const string UPGRADE_DISK_GREAT_HALL = "GREAT HALL";
		public const string UPGRADE_DISK_LOST_AND_FOUND = "LOST AND FOUND";
		public const string UPGRADE_DISK_HER_LADYSHIPS_CHAMBER = "HER LADYSHIPS CHAMBER";
		public const string UPGRADE_DISK_MECHANARIUM = "MECHANARIUM";
		public const string UPGRADE_DISK_MORNING_ROOM = "MORNING ROOM";
		public const string UPGRADE_DISK_OFFICE = "OFFICE";
		public const string UPGRADE_DISK_TRADING_POST_TRADE = "TRADING POST TRADE";
		public const string UPGRADE_DISK_VAULT = "VAULT";
		public const string UPGRADE_DISK_ABANDONED_MINE = "ABANDONED MINE";

        /// <summary>
        ///     Names of the models in the "you found" notifications.
        ///     You may want to use GetItemModelInNotificationName() to access these.
        /// </summary>
        private class MODEL_IN_NOTIFICATION
        {
            public const string KEY_8 = "SILVER KEY";
            public const string WATERING_CAN = "Watering Can pickup";
            public const string LUNCH_BOX = "Joya Lunch Box";
            public const string CURSED_EFFIGY = "cursed effigy- dagger";
            public const string CROWN = "crow";
            public const string HALL_PASS = "hallpass";
            public const string CABINET_KEY = "cabinet key";
            public const string PRISM_KEY = "Prism Key";
            public const string KEY_OF_ARIES = "o Key";
            public const string POWER_HAMMER = "Power Hammer ";
            public const string ELECTROMAGNET = "Powerd Electromagnet";
            public const string TORCH = "Self Lighting Torch";
            public const string MORNING_STAR = "Mornign Star";
        }

        /// <summary>
        ///     Names of the game objects associated to an item.
        ///     You may want to use GetGameObjectName() to access these.
        /// </summary>
        private class GAME_OBJECT_NAME
        {
            public const string CABINET_KEY_TUNNEL = "CABINET KEY 2";
            public const string ELECTROMAGNET = "POWERED ELECTROMAGNET";
            public const string PRISM_KEY = "PRISM KEY";
        }

        /// <summary>
        ///     Names of the you found notification objects associated to an item.
        ///     You may want to use GetYouFoundNotificationName() to access these.
        /// </summary>
        private class YOU_FOUND_NOTIFICATION_NAME
        {
            public const string MAGNIFYING_GLASS = "Mag Glass";
            public const string LOCK_PICK_KIT = "Lock Pick";
            public const string SLEEPING_MASK = "Sleep Mask";
            public const string PRISM_KEY = "Prism Key";
            public const string KEY_OF_ARIES = "The Key of Aries";
            public const string VAULT_KEY_149 = "Key 149";
            public const string VAULT_KEY_233 = "Key 233";
            public const string VAULT_KEY_304 = "Key 304";
            public const string VAULT_KEY_370 = "Key 370";
            public const string CABINET_KEY_PATIO = "Cabinet Key";
            public const string CABINET_KEY_LAUNDRY = "Cabinet Key 5";
            public const string CABINET_KEY_TUNNEL = "Cabinet Key 5";
            public const string LUCKY_PURSE = "lucky purse";
            public const string PICK_SOUND_AMPLIFIER = "pick sound amplifier";
            public const string BURNING_GLASS = "burning glass";
            public const string DETECTOR_SHOVEL = "detector shovel";
            public const string POWER_HAMMER = "power hammer";
            public const string JACK_HAMMER = "jack hammer";
            public const string ELECTROMAGNET = "Electro Magnet";
            public const string KNIGHTS_SHIELD = "Knight's Shield";
        }

        /// <summary>
        ///     Names of the prefab objects associated to an item.
        ///     You may want to use GetPrefabName() to access these.
        /// </summary>
        private class PREFAB_NAME
        {
            public const string ELECTROMAGNET = "POWERED ELECTROMAGNET";
        }

        /// <summary>
        ///     Gets the internally used name of the item model within the item notification.
        /// </summary>
        /// <param name="name">The item name.</param>
        /// <returns>The model name.</returns>
        public static string GetItemModelInNotificationName(string name)
        {
            return name switch
            {
                KEY_8 => MODEL_IN_NOTIFICATION.KEY_8,
                WATERING_CAN => MODEL_IN_NOTIFICATION.WATERING_CAN,
                LUNCH_BOX => MODEL_IN_NOTIFICATION.LUNCH_BOX,
                CURSED_EFFIGY => MODEL_IN_NOTIFICATION.CURSED_EFFIGY,
                CROWN => MODEL_IN_NOTIFICATION.CROWN,
                HALL_PASS => MODEL_IN_NOTIFICATION.HALL_PASS,
                CABINET_KEY_PATIO => MODEL_IN_NOTIFICATION.CABINET_KEY,
                CABINET_KEY_LAUNDRY => MODEL_IN_NOTIFICATION.CABINET_KEY,
                CABINET_KEY_TUNNEL => MODEL_IN_NOTIFICATION.CABINET_KEY,
                PRISM_KEY => MODEL_IN_NOTIFICATION.PRISM_KEY,
                KEY_OF_ARIES => MODEL_IN_NOTIFICATION.KEY_OF_ARIES,
                POWER_HAMMER => MODEL_IN_NOTIFICATION.POWER_HAMMER,
                ELECTROMAGNET => MODEL_IN_NOTIFICATION.ELECTROMAGNET,
                TORCH => MODEL_IN_NOTIFICATION.TORCH,
                MORNING_STAR => MODEL_IN_NOTIFICATION.MORNING_STAR,
                _ => name,
            };
        }

        /// <summary>
        ///     Gets the objectname of items where it doesn't match their name.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <returns></returns>
        public static string GetGameObjectName(string name)
        {
            return name switch
            {
                CABINET_KEY_TUNNEL => GAME_OBJECT_NAME.CABINET_KEY_TUNNEL,
                ELECTROMAGNET => GAME_OBJECT_NAME.ELECTROMAGNET,
                PRISM_KEY => GAME_OBJECT_NAME.PRISM_KEY,
                _ => name,
            };
        }

        /// <summary>
        /// Retrieves the corresponding notification name for a given unique item found by the player.
        /// </summary>
        /// <param name="name">The item name.</param>
        /// <returns>
        /// The in-game notification name formatted in Title Case.
        /// </returns>
        public static string GetYouFoundNotificationName(string name)
        {
            return name switch
            {
                MAGNIFYING_GLASS => YOU_FOUND_NOTIFICATION_NAME.MAGNIFYING_GLASS,
                LOCK_PICK_KIT => YOU_FOUND_NOTIFICATION_NAME.LOCK_PICK_KIT,
                SLEEPING_MASK => YOU_FOUND_NOTIFICATION_NAME.SLEEPING_MASK,
                PRISM_KEY => YOU_FOUND_NOTIFICATION_NAME.PRISM_KEY,
                KEY_OF_ARIES => YOU_FOUND_NOTIFICATION_NAME.KEY_OF_ARIES,
                VAULT_KEY_149 => YOU_FOUND_NOTIFICATION_NAME.VAULT_KEY_149,
                VAULT_KEY_233 => YOU_FOUND_NOTIFICATION_NAME.VAULT_KEY_233,
                VAULT_KEY_304 => YOU_FOUND_NOTIFICATION_NAME.VAULT_KEY_304,
                VAULT_KEY_370 => YOU_FOUND_NOTIFICATION_NAME.VAULT_KEY_370,
                CABINET_KEY_PATIO => YOU_FOUND_NOTIFICATION_NAME.CABINET_KEY_PATIO,
                CABINET_KEY_LAUNDRY => YOU_FOUND_NOTIFICATION_NAME.CABINET_KEY_LAUNDRY,
                CABINET_KEY_TUNNEL => YOU_FOUND_NOTIFICATION_NAME.CABINET_KEY_TUNNEL,
                LUCKY_PURSE => YOU_FOUND_NOTIFICATION_NAME.LUCKY_PURSE,
                PICK_SOUND_AMPLIFIER => YOU_FOUND_NOTIFICATION_NAME.PICK_SOUND_AMPLIFIER,
                BURNING_GLASS => YOU_FOUND_NOTIFICATION_NAME.BURNING_GLASS,
                DETECTOR_SHOVEL => YOU_FOUND_NOTIFICATION_NAME.DETECTOR_SHOVEL,
                POWER_HAMMER => YOU_FOUND_NOTIFICATION_NAME.POWER_HAMMER,
                JACK_HAMMER => YOU_FOUND_NOTIFICATION_NAME.JACK_HAMMER,
                ELECTROMAGNET => YOU_FOUND_NOTIFICATION_NAME.ELECTROMAGNET,
                KNIGHTS_SHIELD => YOU_FOUND_NOTIFICATION_NAME.KNIGHTS_SHIELD,          
                _ => name.ToTitleCase(),
            };
        }

        /// <summary>
        /// Retrieves the corresponding prefab name for a given unique item found by the player.
        /// </summary>
        /// <param name="name">The item name.</param>
        /// <returns>
        /// The prefab name.
        /// </returns>
        public static string GetPrefabName(string name)
        {
            if (name == ELECTROMAGNET)
            {
                return PREFAB_NAME.ELECTROMAGNET;
            }
            return name;
        }
    }
}

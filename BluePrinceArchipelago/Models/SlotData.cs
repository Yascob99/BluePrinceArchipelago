using Newtonsoft.Json;
using System.Collections.Generic;

namespace BluePrinceArchipelago.Models
{
    public class SlotData
    {
        [JsonProperty("room_draft_sanity")]
        public bool RoomDraftSanity { get; set; }

        [JsonProperty("locked_trunks_common")]
        public int LockedTrunksCommon { get; set; }

        [JsonProperty("locked_trunks_rare")]
        public int LockedTrunksRare { get; set; }

        [JsonProperty("locked_trunks_complex")]
        public int LockedTrunksComplex { get; set; }

        [JsonProperty("standard_item_sanity")]
        public bool StandardItemSanity { get; set; }

        [JsonProperty("workshop_sanity")]
        public bool WorkShopSanity { get; set; }

        [JsonProperty("upgrade_disk_sanity")]
        public bool UpgradeDiskSanity { get; set; }

        [JsonProperty("key_sanity")]
        public bool KeySanity { get; set; }

        [JsonProperty("special_shop_sanity")]
        public bool SpecialShopSanity { get; set; }

        //TODO remove this once the APworld is no longer sending it.
        [JsonProperty("item_logic_mode")]
        public ItemLogicMode ItemLogicMode { get; set; }

        [JsonProperty("death_link_type")]
        public DeathLinkType DeathLinkType { get; set; }

        [JsonProperty("death_link_grace")]
        public int DeathLinkGrace { get; set; }

        [JsonProperty("death_link_monk_exception")]
        public int DeathLinkMonkException { get; set; }

        [JsonProperty("goal_type")]
        public GoalType GoalType { get; set; }

        [JsonProperty("goal_sanctum_solves")]
        public int GoalSanctumSolves { get; set; }

        [JsonProperty("start_inventory")]
        public Dictionary<string, int> StartInventory { get; set; }
    }
}

using BluePrinceArchipelago.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     A Handler for all the Upgrade disks and their associated functions. Upgrade Disks persist across days.
    /// </summary>
    public class UpgradeDisksGameItem : GameItem
    {
        private readonly string _upgradeDiskName;
        private readonly string _locationName;
        private readonly int _index;


        private static readonly List<string> _itemNames = ["ARCHIVES", "TRADING POST DYNAMITE", "TOMB", "COMMISSARY", "FOUNDATION", "FREEZER", "GARAGE", "GREAT HALL", "LOST AND FOUND", "HER LADYSHIPS CHAMBER", "MECHANARIUM", "MORNING ROOM", "OFFICE", "TRADING POST TRADE", "VAULT", "ABANDONED MINE"];
        private static readonly List<string> _locationNames = ["ARCHIVES", "TRADING POST TRADE", "TOMB", "COMMISSARY", "THE FOUNDATION", "FREEZER", "GARAGE", "GREAT HALL", "LOST & FOUND", "HER LADYSHIP'S CHAMBER", "MECHANARIUM", "MORNING ROOM", "OFFICE", "TRADING POST TRADE", "VAULT", "ABANDONED MINE"];
        private static readonly List<string> _usedVariables = ["Upgrade Disc - Archives", "Upgrade Disc - Bootleg", "Upgrade Disc - Cloister", "Upgrade Disc - Commissary", "Upgrade Disc - Foundation", "Upgrade Disc - Freezer", "Upgrade Disc - Garage", "Upgrade Disc - Great Hall", "Upgrade Disc - LostFound", "Upgrade Disc - Master Bedroom", "Upgrade Disc - Mechanarium", "Upgrade Disc - Morning Room", "Upgrade Disc - Office", "Upgrade Disc - Shop", "Upgrade Disc - Tomb", "Upgrade Disc - Torch Room"];

        private static readonly List<string> _foundLocations = [];
        public static List<GameObject> YouFoundObjects = [];


        public UpgradeDisksGameItem(string upgradeDiskName) : base("UPGRADE DISK", GameObject.Find("__SYSTEM/Upgrade Disks"))
        {
            _upgradeDiskName = upgradeDiskName;
            _index = _itemNames.IndexOf(upgradeDiskName);
            _locationName = _locationNames[_index];
        }

        public override void AddToWorld()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Unlocks an upgrade disk location if it exists and hasn't already been found.
        /// </summary>
        /// <returns>If the location was unlocked.</returns>
        public bool UnlockLocation()
        {
            if (_foundLocations.Contains(_upgradeDiskName))
            {
                return false;
            }
            _foundLocations.Add(_upgradeDiskName);
            State.UpdateUpgradeDiskData();
            return true;
        }
    }
}
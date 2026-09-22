using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     A template for Grouped Items (multiple similar items).
    /// </summary>
    /// <param name="name">The item name</param>
    /// <param name="gameObject">The GameObject of the item</param>
    /// <param name="isUnlocked">If the item is unlocked.</param>
    /// <param name="count">The number of grouped items</param>
    /// <param name="isPreSpawn">If the item is in the prespawn pool.</param>
    public class GroupedItems(string name, GameObject gameObject, bool isUnlocked, int count = 0, bool isPreSpawn = true) : ModItem(name, gameObject, isUnlocked)
    {
        private int _Count = count;
        public new int Count
        {
            get { return _Count; }
            set
            {
                _Count = value;
            }
        }
        //If it's in the prespawn list
        public bool IsPreSpawn = isPreSpawn;
        // The names of AP Location.
        public List<string> LocationNames = new List<string>();
        // The names of the AP Item.
        public List<string> ItemNames = new List<string>();
        // The locations at which it has been found.
        public List<string> FoundLocations = new List<string>();
        // The locations to which the upgrade disk has been received for;
        public List<string> RecievedItems = new List<string>();
        // The locations to which the upgrade disk received has been used.
        public List<string> UsedLocations = new List<string>();

        public int totalFound
        {
            get
            {
                return LocationNames.Count - FoundLocations.Count;
            }
        }
    }
}

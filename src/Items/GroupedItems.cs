using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     A template for Grouped Items (multiple similar items).
    /// </summary>
    /// <param name="name">The item name</param>
    /// <param name="gameObject">The GameObject of the item</param>
    /// <param name="count">The number of grouped items</param>
    /// <param name="isPreSpawn">If the item is in the prespawn pool.</param>
    /// 
    public abstract class GroupedItems(string name, GameObject gameObject, int count = 0, bool isPreSpawn = true) : GameItem(name, gameObject)
    {
        // The locations at which it has been found.
        public List<string> FoundLocations = new List<string>();

        // The locations to which the upgrade disk has been received for;
        public List<string> RecievedItems = new List<string>();

        // The locations to which the upgrade disk received has been used.
        public List<string> UsedLocations = new List<string>();
    }
}

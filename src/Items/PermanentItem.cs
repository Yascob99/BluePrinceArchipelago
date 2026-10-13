using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
#endif
#if ML
using Il2CppHutongGames.PlayMaker;
#endif
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     A template for creating permanent items (persistent junk items) for the mod.   
    /// </summary>
    /// <param name="name">The name of the item</param>
    /// <param name="gameObject">The gameobject of the item. Usually Null.</param>
    /// <param name="isUnlocked">If the item is unlocked.</param>
    /// <param name="itemType">The type of the Permanent Item.</param>
    /// <param name="count">The count of the permanent item.</param>
    public class PermanentItem(string name, GameObject gameObject, bool isUnlocked, string itemType, int count = 1) : ModItem(name, gameObject, isUnlocked)
    {
        private string _ItemType = itemType;
        public int UnlockedCount = 0;

        public string ItemType
        {
            get { return _ItemType; }
            set { _ItemType = value; }
        }
        private int _Count = count;
        public new int Count
        {
            get { return _Count; }
            set
            {
                _Count = value;
            }
        }
        public override void AddItemToInventory()
        {
            ResourceManager.AdjustResource(_ItemType, _Count);
        }
    }
}

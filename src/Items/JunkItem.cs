using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
#endif
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     A template for creating junk items for the mod. 
    /// </summary>
    /// <param name="name">The name of the item</param>
    /// <param name="gameObject">The gameobject of the item. Usually Null.</param>
    /// <param name="isUnlocked">If the item is unlocked.</param>
    /// <param name="itemType">The type of the Junk Item.</param>
    /// <param name="count">The count of the junk item.</param>
    public class JunkItem(string name, GameObject gameObject, bool isUnlocked, string itemType, int count = 1) : ModItem(name, gameObject, isUnlocked)
    {

        private string _ItemType = itemType;
        public string Itemtype
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
                if (value > 0)
                {
                    _IsTrap = true; //Sets IsTrap dynamically (not sure that it's needed, but it's neat).
                }
                else
                {
                    _IsTrap = false; //Sets IsTrap dynamically (not sure that it's needed, but it's neat).
                }
                _Count = value;
            }
        }

        private bool _IsTrap = count < 0;
        public bool IsTrap
        {
            get { return _IsTrap; } //No setter since this is connected to count
        }

        public override void AddItemToInventory()
        {
            ResourceManager.AdjustResource(_ItemType, _Count);
        }
    }
}

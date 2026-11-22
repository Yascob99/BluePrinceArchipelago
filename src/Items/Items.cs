using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     A class template for handling the item in a mod context.
    /// </summary>
    /// <param name="name">The Name of the item</param>
    /// <param name="isUnlocked">If the item is Unlocked.</param>
    /// <param name="count">The number of the item in the pool.</param>
    public abstract class ModItem(string name, bool isUnlocked, int count = 1) : IObtainable
    {
        private readonly string _name = name;
        public string Name { get; }
        protected bool _isUnlocked = isUnlocked;
        public bool IsUnlocked { get; set; }
        protected int _count = count;

        public bool IsUnique()
        {
            return _count == 1;
        }

        public void StackItem()
        {
            ++_count;
        } 

        public abstract void AddToWorld();

        public virtual bool IsPermanent()
        {
            return false;
        }
    }
}

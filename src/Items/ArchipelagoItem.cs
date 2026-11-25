namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     A class template for handling the item in a mod context.
    /// </summary>
    /// <param name="name">The Name of the item</param>
    /// <param name="isUnlocked">If the item is Unlocked.</param>
    /// <param name="count">The number of the item in the pool.</param>
    public abstract class ArchipelagoItem(string name, bool isUnlocked, int count = 0) : IObtainable
    {
        private readonly string _name = name;
        public string Name { get { return _name; } }
        protected bool _isUnlocked = isUnlocked;
        public bool IsUnlocked 
        { 
            get { return _isUnlocked; }
            set { _isUnlocked = value; UnlockHook(); }
        }
        protected int _count = count;

        public bool IsUnique()
        {
            return _count == 1;
        }

        public virtual void StackItem()
        {
            ++_count;
        } 

        public abstract void AddToWorld();

        protected virtual void UnlockHook() {}

        public virtual bool IsPermanent()
        {
            return false;
        }

        public virtual bool IsTrap()
        {
            return false;
        }
    }
}

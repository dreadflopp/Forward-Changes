using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.Contexts
{
    public class ItemContext<T> : IItemContext<T>
    {
        public T Item { get; set; }
        public string OwnerMod { get; set; }

        public ItemContext(T item, string ownerMod)
        {
            Item = item;
            OwnerMod = ownerMod;
        }
    }
}
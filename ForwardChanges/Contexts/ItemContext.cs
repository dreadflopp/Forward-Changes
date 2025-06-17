using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.Contexts
{
    public class ItemContext<TItem> : IItemContext<TItem>
    {
        public TItem Item { get; set; }
        public string OwnerMod { get; set; }

        public ItemContext(TItem item, string ownerMod)
        {
            Item = item;
            OwnerMod = ownerMod;
        }
    }
}
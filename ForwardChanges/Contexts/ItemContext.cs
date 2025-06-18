using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.Contexts
{
    /// <summary>
    /// Basic implementation of IItemContext that stores an item and its owning mod.
    /// </summary>
    /// <typeparam name="T">The type of the item</typeparam>
    public class ItemContext<T> : IItemContext<T>
    {
        /// <summary>
        /// The item value. May be null to represent unset properties in Skyrim.
        /// </summary>
        public T? Item { get; set; }

        /// <summary>
        /// The mod that last modified this item. Will always be set.
        /// </summary>
        public string OwnerMod { get; set; }

        /// <summary>
        /// Creates a new ItemContext with the specified item and owner mod.
        /// </summary>
        /// <param name="item">The item value, which may be null</param>
        /// <param name="ownerMod">The mod that owns this item</param>
        public ItemContext(T? item, string ownerMod)
        {
            Item = item;
            OwnerMod = ownerMod;
        }
    }
}
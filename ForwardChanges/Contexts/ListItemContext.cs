using System;
using System.Collections.Generic;
using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.Contexts
{    /// <summary>
     /// Represents entries (items, factions, etc.) in list properties.
     /// </summary>
     /// <typeparam name="T">The type of the item</typeparam>
     /// <remarks>
     /// Contains metadata about the item's state in the mod load order:
     /// <list type="bullet">
     ///     <item><description><see cref="IsRemoved"/>: Indicates if the item has been removed by a mod later in the load order</description></item>
     ///     <item><description><see cref="OwnerMod"/>: The mod that last modified the item (added or removed)</description></item>
     ///     <item><description><see cref="ItemsBefore"/>: Items that should come before this one in the list</description></item>
     ///     <item><description><see cref="ItemsAfter"/>: Items that should come after this one in the list</description></item>
     /// </list>
     /// </remarks>
    public class ListItemContext<T> : IItemContext<T>
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
        /// Indicates if the item has been removed by a mod later in the load order.
        /// </summary>
        public bool IsRemoved { get; set; }

        /// <summary>
        /// Items that should come before this one in the list.
        /// </summary>
        public List<string> ItemsBefore { get; set; } = new();  // Items that should come before this one

        /// <summary>
        /// Items that should come after this one in the list.
        /// </summary>
        public List<string> ItemsAfter { get; set; } = new();   // Items that should come after this one

        /// <summary>
        /// Creates a new ListItemContext with the specified item and owner mod.
        /// </summary>
        /// <param name="item">The item value, which may be null</param>
        /// <param name="ownerMod">The mod that owns this item</param>
        public ListItemContext(T? item, string ownerMod)
        {
            Item = item;
            OwnerMod = ownerMod;
            IsRemoved = false;
        }
    }
}
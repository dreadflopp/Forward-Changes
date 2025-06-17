using System;
using System.Collections.Generic;
using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.Contexts
{    /// <summary>
     /// Represents entries (items, factions, etc.) in list properties.
     /// </summary>
     /// <remarks>
     /// Contains metadata about the item's state in the mod load order:
     /// <list type="bullet">
     ///     <item><description><see cref="IsRemoved"/>: Indicates if the item has been removed by a mod later in the load order</description></item>
     ///     <item><description><see cref="OwnerMod"/>: The mod that last modified the item (added or removed)</description></item>
     /// </list>
     /// </remarks>
    public class ListItemContext<TItem> : ItemContext<TItem>
    {
        public bool IsRemoved { get; set; }
        public int OriginalIndex { get; set; }  // For tracking position in context
        public List<string> ItemsBefore { get; set; } = new();  // Items that should come before this one
        public List<string> ItemsAfter { get; set; } = new();   // Items that should come after this one

        public ListItemContext(TItem item, string ownerMod) : base(item, ownerMod)
        {
            IsRemoved = false;
            OriginalIndex = -1;
        }
    }
}
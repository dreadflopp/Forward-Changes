using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts;
using ForwardChanges.Contexts.Interfaces;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public abstract class ItemListPropertyHandler<T> : AbstractListPropertyHandler<T>
    {
        public override string PropertyName => "Items";

        public ItemListPropertyHandler()
        {
        }

        /// <summary>
        /// Override the base UpdatePropertyContext to handle items differently from factions.
        /// Items can have duplicates and count modifications.
        /// </summary>
        public override void UpdatePropertyContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            IPropertyContext propertyContext)
        {
            if (context == null)
            {
                Console.WriteLine($"Error: Context is null for {PropertyName}");
                return;
            }

            LogCollector.Add(PropertyName, $"[{PropertyName}] Processing mod: {context.ModKey}");

            if (propertyContext is not ListPropertyContext<T> listPropertyContext)
            {
                throw new InvalidOperationException($"Error: Property context is not a list property context for {PropertyName}");
            }

            var recordItems = GetValue(context.Record) ?? [];
            var forwardValueContexts = listPropertyContext.ForwardValueContexts;
            if (forwardValueContexts == null)
            {
                Console.WriteLine($"Error: Property context is not properly initialized for {PropertyName}");
                return;
            }

            var recordMod = state.LoadOrder[context.ModKey].Mod;
            if (recordMod == null)
            {
                Console.WriteLine($"Error: Record mod is null for {PropertyName}");
                return;
            }

            // Process items with custom logic for duplicates and count modifications
            ProcessItemListLogic(context, state, listPropertyContext, recordItems, forwardValueContexts, recordMod);
        }

        /// <summary>
        /// Custom processing logic for item lists that allows duplicates and count modifications.
        /// </summary>
        private void ProcessItemListLogic(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            ListPropertyContext<T> listPropertyContext,
            List<T> recordItems,
            List<ListPropertyValueContext<T>> currentForwardItems,
            ISkyrimModGetter recordMod)
        {
            // Process each item in the record
            for (int recordIndex = 0; recordIndex < recordItems.Count; recordIndex++)
            {
                var recordItem = recordItems[recordIndex];
                LogCollector.Add(PropertyName, $"[{PropertyName}] [{context.ModKey}] Processing record item {recordIndex}: {FormatItem(recordItem)}");

                // Check if this exact item already exists in our forward list from the same mod
                var existingItemsFromSameMod = currentForwardItems
                    .Where(i => !i.IsRemoved &&
                               GetItemFormKey(i.Value) == GetItemFormKey(recordItem) &&
                               GetItemCount(i.Value) == GetItemCount(recordItem) &&
                               i.OwnerMod == context.ModKey.ToString())
                    .ToList();

                // Check if this item already exists in our forward list from any mod (same reference and count)
                var existingItemsFromAnyMod = currentForwardItems
                    .Where(i => !i.IsRemoved &&
                               GetItemFormKey(i.Value) == GetItemFormKey(recordItem) &&
                               GetItemCount(i.Value) == GetItemCount(recordItem))
                    .ToList();

                // Check if this item reference exists with a different count (for count updates)
                var existingItemsWithDifferentCount = currentForwardItems
                    .Where(i => !i.IsRemoved &&
                               GetItemFormKey(i.Value) == GetItemFormKey(recordItem) &&
                               GetItemCount(i.Value) != GetItemCount(recordItem))
                    .ToList();

                if (existingItemsFromSameMod.Any())
                {
                    // Item already exists from this mod - this is a duplicate, add it
                    var newItem = new ListPropertyValueContext<T>(recordItem, context.ModKey.ToString());
                    currentForwardItems.Add(newItem);
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding duplicate item {FormatItem(recordItem)} Success");
                }
                else if (existingItemsWithDifferentCount.Any())
                {
                    // Item exists with different count - update the count
                    var existingItem = existingItemsWithDifferentCount.First();
                    var oldCount = GetItemCount(existingItem.Value);

                    LogCollector.Add(PropertyName, $"[{PropertyName}] [{context.ModKey}] Count changed for existing item: {FormatItem(recordItem)}");

                    // Check if we can modify this item
                    var canModify = recordMod.MasterReferences.Any(m => m.Master.ToString() == existingItem.OwnerMod);
                    if (canModify)
                    {
                        // Update the count
                        existingItem.Value = recordItem;
                        existingItem.OwnerMod = context.ModKey.ToString();
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Updated count {oldCount} -> {GetItemCount(recordItem)} for {GetItemFormKey(recordItem)} Success");
                    }
                    else
                    {
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Count update denied {oldCount} -> {GetItemCount(recordItem)} for {GetItemFormKey(recordItem)}. Owned by {existingItem.OwnerMod}");
                    }
                }
                else if (existingItemsFromAnyMod.Any())
                {
                    // Item exists with same reference and count - no change needed
                    LogCollector.Add(PropertyName, $"[{PropertyName}] [{context.ModKey}] Item unchanged: {FormatItem(recordItem)}");
                }
                else
                {
                    // Check if this item was previously removed
                    var previouslyRemovedItems = currentForwardItems
                        .Where(i => i.IsRemoved &&
                                   GetItemFormKey(i.Value) == GetItemFormKey(recordItem))
                        .ToList();

                    if (previouslyRemovedItems.Any())
                    {
                        // Try to add back one of the previously removed items
                        bool addedBack = false;
                        foreach (var removedItem in previouslyRemovedItems)
                        {
                            LogCollector.Add(PropertyName, $"[{PropertyName}] [{context.ModKey}] Found previously removed item: {FormatItem(removedItem.Value)} (owned by {removedItem.OwnerMod})");

                            // Check if we can add it back
                            var canModify = recordMod.MasterReferences.Any(m => m.Master.ToString() == removedItem.OwnerMod);
                            if (canModify)
                            {
                                removedItem.IsRemoved = false;
                                removedItem.OwnerMod = context.ModKey.ToString();
                                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Added back previously removed item {FormatItem(recordItem)} Success");
                                addedBack = true;
                                break; // Only add back one instance
                            }
                            else
                            {
                                LogCollector.Add(PropertyName, $"[{PropertyName}] [{context.ModKey}] Permission denied for adding back item: {FormatItem(recordItem)} (owned by {removedItem.OwnerMod})");
                            }
                        }

                        if (!addedBack)
                        {
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Could not add back previously removed item {FormatItem(recordItem)} - no permissions");
                        }
                    }
                    else
                    {
                        // Check if any other mod has this item reference (regardless of count)
                        var otherModItems = currentForwardItems
                            .Where(i => !i.IsRemoved &&
                                       GetItemFormKey(i.Value) == GetItemFormKey(recordItem))
                            .ToList();

                        if (otherModItems.Any())
                        {
                            // Another mod already has this item - don't add duplicate
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Skipping item {FormatItem(recordItem)} - already exists in mod {otherModItems.First().OwnerMod}");
                        }
                        else
                        {
                            // Truly new item
                            var newItem = new ListPropertyValueContext<T>(recordItem, context.ModKey.ToString());
                            currentForwardItems.Add(newItem);
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding new item {FormatItem(recordItem)} Success");
                        }
                    }
                }
            }

            // Check for items that were removed
            foreach (var forwardItem in currentForwardItems.Where(i => !i.IsRemoved).ToList())
            {
                // Check if this item still exists in the record (same reference and count)
                bool itemStillExists = recordItems.Any(recordItem =>
                    GetItemFormKey(recordItem) == GetItemFormKey(forwardItem.Value) &&
                    GetItemCount(recordItem) == GetItemCount(forwardItem.Value));

                if (!itemStillExists)
                {
                    var canModify = recordMod.MasterReferences.Any(m => m.Master.ToString() == forwardItem.OwnerMod);
                    if (canModify)
                    {
                        var oldOwner = forwardItem.OwnerMod;
                        forwardItem.IsRemoved = true;
                        forwardItem.OwnerMod = context.ModKey.ToString();
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Removing item {FormatItem(forwardItem.Value)} (was owned by {oldOwner}) Success");
                    }
                    else
                    {
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Removing item {FormatItem(forwardItem.Value)} (was owned by {forwardItem.OwnerMod}) Permission denied");
                    }
                }
            }
        }

        /// <summary>
        /// Formats a list of container items into a string.
        /// </summary>
        /// <param name="items">The list of items to format.</param>
        /// <returns>A string representation of the items.</returns>
        public string FormatItemList(List<T>? items)
        {
            if (items == null || items.Count == 0)
                return "No items";

            return string.Join(", ", items.Select(i => FormatItem(i)));
        }

        /// <summary>
        /// Checks if two container items are equal.
        /// </summary>
        /// <param name="item1">The first item to compare.</param>
        /// <param name="item2">The second item to compare.</param>
        /// <returns>True if the items are equal, false otherwise.</returns>
        protected override bool IsItemEqual(T? item1, T? item2)
        {
            return IsItemReferenceEqual(item1, item2);
        }

        /// <summary>
        /// Checks if two items are equal by their reference.
        /// </summary>
        /// <param name="item1">The first item to compare.</param>
        /// <param name="item2">The second item to compare.</param>
        /// <returns>True if the items are equal, false otherwise.</returns>
        protected bool IsItemReferenceEqual(T? item1, T? item2)
        {
            if (ReferenceEquals(item1, item2)) return true;
            if (item1 is null || item2 is null) return false;
            return GetItemFormKey(item1).Equals(GetItemFormKey(item2));
        }

        /// <summary>
        /// Checks if two items are equal by their count.
        /// </summary>
        /// <param name="item1">The first item to compare.</param>
        /// <param name="item2">The second item to compare.</param>
        /// <returns>True if the items are equal, false otherwise.</returns>
        protected bool IsCountEqual(T? item1, T? item2)
        {
            if (ReferenceEquals(item1, item2)) return true;
            if (item1 is null || item2 is null) return false;
            return GetItemCount(item1) == GetItemCount(item2);
        }

        /// <summary>
        /// Formats a container item into a string.
        /// </summary>
        /// <param name="item">The item to format.</param>
        /// <returns>A string representation of the item.</returns>
        protected override string FormatItem(T? item)
        {
            if (item == null) return "null";
            return $"{GetItemFormKey(item)}(Count {GetItemCount(item)})";
        }

        // Abstract methods that concrete implementations must provide
        protected abstract FormKey GetItemFormKey(T item);
        protected abstract int GetItemCount(T item);

        // Abstract methods for record-specific operations
        public abstract override void SetValue(IMajorRecord record, List<T>? value);
        public abstract override List<T>? GetValue(IMajorRecordGetter record);
    }
}
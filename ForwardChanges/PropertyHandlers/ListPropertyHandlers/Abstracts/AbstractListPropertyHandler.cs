using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts
{
    public abstract class AbstractListPropertyHandler<TItem> : IPropertyHandler<IReadOnlyList<TItem>>
    {
        public abstract string PropertyName { get; }
        public bool IsListHandler => true;
        protected virtual bool RequiresOrdering => false;

        public abstract void SetValue(IMajorRecord record, IReadOnlyList<TItem>? value);
        public abstract IReadOnlyList<TItem>? GetValue(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context);

        public virtual bool AreValuesEqual(IReadOnlyList<TItem>? value1, IReadOnlyList<TItem>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            return value1.All(item1 => value2.Any(item2 => IsItemEqual(item1, item2)));
        }

        public virtual bool AreValuesEqualInOrder(IReadOnlyList<TItem>? value1, IReadOnlyList<TItem>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            for (int i = 0; i < value1.Count; i++)
            {
                if (!IsItemEqual(value1[i], value2[i])) return false;
            }
            return true;
        }

        // IPropertyHandlerBase implementation
        void IPropertyHandlerBase.SetValue(IMajorRecord record, object? value)
        {
            try
            {
                SetValue(record, (IReadOnlyList<TItem>?)value);
            }
            catch (InvalidCastException)
            {
                Console.WriteLine($"[{PropertyName}] SetValue failed: Expected type IReadOnlyList<{typeof(TItem)}>, got {value?.GetType() ?? typeof(object)}");
                throw;
            }
        }

        object? IPropertyHandlerBase.GetValue(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            try
            {
                return GetValue(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{PropertyName}] GetValue failed: {ex.Message}");
                throw;
            }
        }

        bool IPropertyHandlerBase.AreValuesEqual(object? value1, object? value2)
        {
            try
            {
                return AreValuesEqual((IReadOnlyList<TItem>?)value1, (IReadOnlyList<TItem>?)value2);
            }
            catch (InvalidCastException)
            {
                Console.WriteLine($"[{PropertyName}] AreValuesEqual failed: Expected type IReadOnlyList<{typeof(TItem)}>, got {value1?.GetType() ?? typeof(object)} and {value2?.GetType() ?? typeof(object)}");
                throw;
            }
        }

        public virtual void UpdatePropertyContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            PropertyContext propertyContext)
        {
            if (context == null)
            {
                Console.WriteLine($"Error: Context is null for {PropertyName}");
                return;
            }

            if (string.IsNullOrEmpty(propertyContext.OriginalValue.OwnerMod))
            {
                Console.WriteLine($"Error: Property state for {PropertyName} not properly initialized");
                return;
            }

            var recordItems = GetValue(context)?.ToList() ?? new List<TItem>();
            var currentForwardItems = (List<ListItemContext<TItem>>)propertyContext.ForwardValue.Item!;
            var recordMod = state.LoadOrder[context.ModKey].Mod;

            if (recordMod == null)
            {
                Console.WriteLine($"Error: Record mod is null for {PropertyName}");
                return;
            }

            // Process removals
            foreach (var item in currentForwardItems.Where(i => !i.IsRemoved).ToList())
            {
                var matchingItemInRecord = recordItems.FirstOrDefault(c => IsItemEqual(c, item.Item));

                if (matchingItemInRecord == null)
                {
                    // Item is being removed
                    var canModify = recordMod.MasterReferences.Any(m => m.Master.ToString() == item.OwnerMod);

                    if (canModify)
                    {
                        var oldOwner = item.OwnerMod;
                        item.IsRemoved = true;
                        item.OwnerMod = context.ModKey.ToString();
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Removing item {FormatItem(item.Item)} (was owned by {oldOwner}) Success");
                    }
                    else
                    {
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Removing item {FormatItem(item.Item)} (was owned by {item.OwnerMod}) Permission denied");
                    }
                }
            }

            // Process additions            
            foreach (var item in recordItems)
            {
                // Check if this specific item was previously removed
                var previouslyRemovedItem = currentForwardItems.FirstOrDefault(e =>
                    e.IsRemoved && IsItemEqual(e.Item, item));

                if (previouslyRemovedItem == null)
                {
                    // Add as new item
                    var newItem = new ListItemContext<TItem>(item, context.ModKey.ToString());
                    InsertItemAtCorrectPosition(newItem, recordItems, currentForwardItems);
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding new item {FormatItem(item)} Success");
                }
                else
                {
                    // Check if we can add it back
                    if (CanAddBack(recordMod, previouslyRemovedItem.OwnerMod))
                    {
                        // Create new item with current data and add it back
                        var newItem = new ListItemContext<TItem>(item, context.ModKey.ToString());
                        // Preserve ordering information
                        newItem.ItemsBefore.AddRange(previouslyRemovedItem.ItemsBefore);
                        newItem.ItemsAfter.AddRange(previouslyRemovedItem.ItemsAfter);
                        newItem.OriginalIndex = previouslyRemovedItem.OriginalIndex;
                        currentForwardItems.Remove(previouslyRemovedItem);
                        InsertItemAtCorrectPosition(newItem, recordItems, currentForwardItems);
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding back item {FormatItem(item)} Success");
                    }
                    else
                    {
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding new item {FormatItem(item)} Permission denied. Previously removed by {previouslyRemovedItem.OwnerMod}");
                    }
                }
            }
            // Process any handler-specific logic
            ProcessHandlerSpecificLogic(context, state, propertyContext, recordItems, currentForwardItems);

            // Update the state
            propertyContext.ForwardValue.Item = currentForwardItems;
        }

        /// <summary>
        /// Process any handler-specific logic after the standard list processing is complete.
        /// </summary>
        /// <param name="context">The mod context</param>
        /// <param name="state">The patcher state</param>
        /// <param name="propertyContext">The property context</param>
        /// <param name="recordItems">The current items in the record</param>
        /// <param name="currentForwardItems">The current forward items</param>
        protected virtual void ProcessHandlerSpecificLogic(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            PropertyContext propertyContext,
            IReadOnlyList<TItem> recordItems,
            List<ListItemContext<TItem>> currentForwardItems)
        {
            // Base implementation does nothing
        }

        protected virtual bool IsItemEqual(TItem item1, TItem item2)
        {
            return Equals(item1, item2);
        }

        protected void InsertItemAtCorrectPosition(
            ListItemContext<TItem> newItem,
            IReadOnlyList<TItem> recordItems,
            List<ListItemContext<TItem>> currentForwardItems)
        {
            if (!RequiresOrdering)
            {
                // If order doesn't matter, just add at the end
                currentForwardItems.Add(newItem);
                return;
            }

            // Get new item's position in source
            var newItemIndex = recordItems.IndexOf(newItem.Item!);
            if (newItemIndex == -1) return;

            // Find items that should come before/after based on source order
            var sourceBeforeItems = recordItems.Take(newItemIndex)
                .Select(r => r?.ToString() ?? string.Empty)
                .ToList();
            var sourceAfterItems = recordItems.Skip(newItemIndex + 1)
                .Select(r => r?.ToString() ?? string.Empty)
                .ToList();

            // Add source ordering to ItemsBefore/ItemsAfter
            newItem.ItemsBefore.AddRange(sourceBeforeItems);
            newItem.ItemsAfter.AddRange(sourceAfterItems);

            // Find insertion point that satisfies both source order and explicit constraints
            var insertIndex = 0;
            for (int i = 0; i < currentForwardItems.Count; i++)
            {
                if (currentForwardItems[i].IsRemoved) continue;

                // Check if this item should come after our new item
                if (newItem.ItemsBefore.Contains(currentForwardItems[i].Item?.ToString() ?? string.Empty))
                {
                    insertIndex = i;
                    break;
                }

                // Check if this item should come before our new item
                if (currentForwardItems[i].ItemsBefore.Contains(newItem.Item?.ToString() ?? string.Empty))
                {
                    insertIndex = i + 1;
                }
            }

            // Insert the new item
            currentForwardItems.Insert(insertIndex, newItem);

            // Update relationships for all items
            UpdateOrderingRelationships(currentForwardItems);
        }

        private void UpdateOrderingRelationships(List<ListItemContext<TItem>> items)
        {
            // Clear existing relationships
            foreach (var item in items.Where(i => !i.IsRemoved))
            {
                item.ItemsBefore.Clear();
                item.ItemsAfter.Clear();
            }

            // Rebuild relationships based on current order
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].IsRemoved) continue;

                // Add all items before this one to ItemsBefore
                for (int j = 0; j < i; j++)
                {
                    if (!items[j].IsRemoved)
                    {
                        var itemStr = items[j].Item?.ToString() ?? string.Empty;
                        items[i].ItemsBefore.Add(itemStr);
                        // Ensure it's not in ItemsAfter
                        items[i].ItemsAfter.Remove(itemStr);
                    }
                }

                // Add all items after this one to ItemsAfter
                for (int j = i + 1; j < items.Count; j++)
                {
                    if (!items[j].IsRemoved)
                    {
                        var itemStr = items[j].Item?.ToString() ?? string.Empty;
                        items[i].ItemsAfter.Add(itemStr);
                        // Ensure it's not in ItemsBefore
                        items[i].ItemsBefore.Remove(itemStr);
                    }
                }
            }
        }

        /// <summary>
        /// Format the item for display in the log.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>The formatted item</returns>
        protected virtual string FormatItem(TItem item)
        {
            return item?.ToString() ?? "null";
        }

        /// <summary>
        /// Updates ordering relationships based on current context and permissions, then sorts items.
        /// </summary>
        public virtual void UpdateOrderingAndSort(
            List<ListItemContext<TItem>> currentForwardItems,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (!RequiresOrdering)
            {
                // If order doesn't matter, no need to sort
                return;
            }

            // Get the list of items from the context's record
            var contextList = GetValue(context)?.ToList() ?? new List<TItem>();
            var recordMod = state.LoadOrder[context.ModKey].Mod;
            if (recordMod == null) return;

            // First, update ordering relationships for items we have permission to modify
            for (int i = 0; i < contextList.Count; i++)
            {
                var contextItem = contextList[i];
                var existingItem = currentForwardItems.FirstOrDefault(w =>
                    !w.IsRemoved &&
                    IsItemEqual(w.Item, contextItem) &&
                    CanAddBack(recordMod, w.OwnerMod));

                if (existingItem != null)
                {
                    // Add items before this one that we have permission to modify
                    if (i > 0)
                    {
                        var beforeItem = contextList[i - 1];
                        var beforeItemStr = beforeItem?.ToString() ?? string.Empty;
                        if (CanAddBack(recordMod, beforeItem?.ToString() ?? string.Empty))
                        {
                            existingItem.ItemsBefore.Add(beforeItemStr);
                            existingItem.ItemsAfter.Remove(beforeItemStr);
                        }
                    }

                    // Add items after this one that we have permission to modify
                    if (i < contextList.Count - 1)
                    {
                        var afterItem = contextList[i + 1];
                        var afterItemStr = afterItem?.ToString() ?? string.Empty;
                        if (CanAddBack(recordMod, afterItem?.ToString() ?? string.Empty))
                        {
                            existingItem.ItemsAfter.Add(afterItemStr);
                            existingItem.ItemsBefore.Remove(afterItemStr);
                        }
                    }
                }
            }

            // Now sort items based on their constraints
            var orderedItems = new List<ListItemContext<TItem>>();
            var remainingItems = new List<ListItemContext<TItem>>(currentForwardItems.Where(i => !i.IsRemoved));

            while (remainingItems.Any())
            {
                // Find items that have no "before" items in the remaining set
                var nextItems = remainingItems
                    .Where(item => !remainingItems.Any(r =>
                        item.ItemsBefore.Contains(r.Item?.ToString() ?? string.Empty)))
                    .ToList();

                if (!nextItems.Any())
                {
                    // If we can't find any items without dependencies, add the rest
                    orderedItems.AddRange(remainingItems);
                    break;
                }

                // Add these items to the ordered list
                orderedItems.AddRange(nextItems);
                remainingItems.RemoveAll(item => nextItems.Contains(item));
            }

            // Update the final list
            currentForwardItems.Clear();
            currentForwardItems.AddRange(orderedItems);

            // Update all relationships based on final order
            UpdateOrderingRelationships(currentForwardItems);
        }

        /// <summary>
        /// Check if the mod can add back an item that was previously removed. It is able to do so if the mod is a master of the owner mod.
        /// or if the mod is the owner mod.
        /// </summary>
        /// <param name="mod">The mod to check</param>
        /// <param name="ownerMod">The mod that owns the item</param>
        /// <returns></returns>
        protected bool CanAddBack(ISkyrimModGetter mod, string ownerMod)
        {
            return mod?.MasterReferences.Any(m => m.Master.ToString() == ownerMod) == true || mod?.ModKey.ToString() == ownerMod;
        }
    }
}
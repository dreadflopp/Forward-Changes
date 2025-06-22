using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts
{
    public abstract class AbstractListPropertyHandler<T> : IPropertyHandler<List<T>>
    {
        public abstract string PropertyName { get; }
        public bool IsListHandler => true;
        protected virtual bool RequiresOrdering => false;

        public abstract void SetValue(IMajorRecord record, List<T>? value);
        public abstract List<T>? GetValue(IMajorRecordGetter record);

        public virtual bool AreValuesEqual(List<T>? value1, List<T>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            return value1.All(item1 => value2.Any(item2 => IsItemEqual(item1, item2)));
        }

        public virtual bool AreValuesEqualInOrder(List<T>? value1, List<T>? value2)
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

        public virtual void UpdatePropertyContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            IPropertyContext propertyContext)
        {
            if (context == null)
            {
                Console.WriteLine($"Error: Context is null for {PropertyName}");
                return;
            }

            Console.WriteLine($"[{PropertyName}] Processing mod: {context.ModKey}");

            if (propertyContext is not ListPropertyContext<T> listPropertyContext)
            {
                throw new InvalidOperationException($"Property context is not a list property context for {PropertyName}");
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

            // Process removals
            foreach (var item in forwardValueContexts.Where(i => !i.IsRemoved).ToList())
            {
                Console.WriteLine($"[{PropertyName}] [{context.ModKey}] Checking removal for item: {FormatItem(item.Value)} (owned by {item.OwnerMod})");
                var matchingItemInRecord = recordItems.FirstOrDefault(c => IsItemEqual(c, item.Value));

                if (matchingItemInRecord == null)
                {
                    Console.WriteLine($"[{PropertyName}] [{context.ModKey}] Item not found in record, marking for removal");
                    // Item is being removed
                    var canModify = recordMod.MasterReferences.Any(m => m.Master.ToString() == item.OwnerMod);

                    if (canModify)
                    {
                        var oldOwner = item.OwnerMod;
                        item.IsRemoved = true;
                        item.OwnerMod = context.ModKey.ToString();
                        Console.WriteLine($"[{PropertyName}] [{context.ModKey}] Marked item as removed: {FormatItem(item.Value)}");
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Removing item {FormatItem(item.Value)} (was owned by {oldOwner}) Success");
                    }
                    else
                    {
                        Console.WriteLine($"[{PropertyName}] [{context.ModKey}] Permission denied for removal: {FormatItem(item.Value)}");
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Removing item {FormatItem(item.Value)} (was owned by {item.OwnerMod}) Permission denied");
                    }
                }
                else
                {
                    Console.WriteLine($"[{PropertyName}] [{context.ModKey}] Item found in record, no removal needed: {FormatItem(matchingItemInRecord)}");
                }
            }

            // Process additions
            foreach (var item in recordItems)
            {
                Console.WriteLine($"[{PropertyName}] [{context.ModKey}] Checking addition for item: {FormatItem(item)}");
                var existingItem = forwardValueContexts.FirstOrDefault(i => IsItemEqual(i.Value, item));
                if (existingItem == null || existingItem.IsRemoved)
                {
                    Console.WriteLine($"[{PropertyName}] [{context.ModKey}] No existing item or existing item is removed, checking if previously removed");
                    // Check if this item was previously removed
                    var previouslyRemovedItem = existingItem?.IsRemoved == true ? existingItem :
                        forwardValueContexts.FirstOrDefault(i => IsItemEqual(i.Value, item) && i.IsRemoved);

                    if (previouslyRemovedItem == null)
                    {
                        Console.WriteLine($"[{PropertyName}] [{context.ModKey}] Adding new item: {FormatItem(item)}");
                        // New item
                        var newItem = new ListPropertyValueContext<T>(item, context.ModKey.ToString());
                        InsertItemAtCorrectPosition(newItem, recordItems, forwardValueContexts);
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding new item {FormatItem(item)} Success");
                    }
                    else
                    {
                        Console.WriteLine($"[{PropertyName}] [{context.ModKey}] Found previously removed item: {FormatItem(previouslyRemovedItem.Value)} (owned by {previouslyRemovedItem.OwnerMod})");
                        // Item was previously removed, check if we can add it back
                        var canModify = recordMod.MasterReferences.Any(m => m.Master.ToString() == previouslyRemovedItem.OwnerMod);
                        Console.WriteLine($"[{PropertyName}] [{context.ModKey}] Can modify: {canModify}");

                        if (canModify)
                        {
                            Console.WriteLine($"[{PropertyName}] [{context.ModKey}] Adding back previously removed item: {FormatItem(item)}");
                            previouslyRemovedItem.IsRemoved = false;
                            previouslyRemovedItem.OwnerMod = context.ModKey.ToString();
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding back previously removed item {FormatItem(item)} Success");
                        }
                        else
                        {
                            Console.WriteLine($"[{PropertyName}] [{context.ModKey}] Permission denied for adding back item: {FormatItem(item)}");
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding new item {FormatItem(item)} Permission denied. Previously removed by {previouslyRemovedItem.OwnerMod}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"[{PropertyName}] [{context.ModKey}] Found existing item: {FormatItem(existingItem.Value)} (removed: {existingItem.IsRemoved})");
                }
            }

            // Process any handler-specific logic
            ProcessHandlerSpecificLogic(context, state, listPropertyContext, recordItems, forwardValueContexts);

            // Update the state
            listPropertyContext.ForwardValueContexts = forwardValueContexts;
        }

        /// <summary>
        /// Process any handler-specific logic after the standard list processing is complete.
        /// </summary>
        /// <param name="context">The mod context</param>
        /// <param name="state">The patcher state</param>
        /// <param name="listPropertyContext">The property context</param>
        /// <param name="recordItems">The current items in the record</param>
        /// <param name="currentForwardItems">The current forward items</param>
        protected virtual void ProcessHandlerSpecificLogic(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            ListPropertyContext<T> listPropertyContext,
            List<T> recordItems,
            List<ListPropertyValueContext<T>> currentForwardItems)
        {
            // Base implementation does nothing
        }

        protected virtual bool IsItemEqual(T? item1, T? item2)
        {
            if (item1 == null && item2 == null)
            {
                return true;
            }

            if (item1 == null || item2 == null)
            {
                return false;
            }

            return Equals(item1, item2);
        }

        protected void InsertItemAtCorrectPosition(
            ListPropertyValueContext<T> newItem,
            List<T> recordItems,
            List<ListPropertyValueContext<T>> currentForwardItems)
        {
            if (!RequiresOrdering)
            {
                // If order doesn't matter, just add at the end
                currentForwardItems.Add(newItem);
                return;
            }

            // Get new item's position in source
            var newItemIndex = recordItems.IndexOf(newItem.Value!);
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
                if (newItem.ItemsBefore.Contains(currentForwardItems[i].Value?.ToString() ?? string.Empty))
                {
                    insertIndex = i;
                    break;
                }

                // Check if this item should come before our new item
                if (currentForwardItems[i].ItemsBefore.Contains(newItem.Value?.ToString() ?? string.Empty))
                {
                    insertIndex = i + 1;
                }
            }

            // Insert the new item
            currentForwardItems.Insert(insertIndex, newItem);

            // Update relationships for all items
            UpdateOrderingRelationships(currentForwardItems);
        }

        private void UpdateOrderingRelationships(List<ListPropertyValueContext<T>> items)
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
                        var itemStr = items[j].Value?.ToString() ?? string.Empty;
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
                        var itemStr = items[j].Value?.ToString() ?? string.Empty;
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
        protected virtual string FormatItem(T? item)
        {
            return item?.ToString() ?? "null";
        }

        /// <summary>
        /// Updates ordering relationships based on current context and permissions, then sorts items.
        /// </summary>
        public virtual void UpdateOrderingAndSort(
            List<ListPropertyValueContext<T>> currentForwardItems,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (!RequiresOrdering)
            {
                // If order doesn't matter, no need to sort
                return;
            }

            // Get the list of items from the context's record
            var itemList = GetValue(context.Record) ?? [];
            var recordMod = state.LoadOrder[context.ModKey].Mod;
            if (recordMod == null) return;

            // First, update ordering relationships for items we have permission to modify
            for (int i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                var existingItem = currentForwardItems.FirstOrDefault(w =>
                    !w.IsRemoved &&
                    IsItemEqual(w.Value, item) &&
                    CanAddBack(recordMod, w.OwnerMod));

                if (existingItem != null)
                {
                    // Add items before this one that we have permission to modify
                    if (i > 0)
                    {
                        var beforeItem = itemList[i - 1];
                        var beforeItemStr = beforeItem?.ToString() ?? string.Empty;
                        if (CanAddBack(recordMod, beforeItem?.ToString() ?? string.Empty))
                        {
                            existingItem.ItemsBefore.Add(beforeItemStr);
                            existingItem.ItemsAfter.Remove(beforeItemStr);
                        }
                    }

                    // Add items after this one that we have permission to modify
                    if (i < itemList.Count - 1)
                    {
                        var afterItem = itemList[i + 1];
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
            var orderedItems = new List<ListPropertyValueContext<T>>();
            var remainingItems = new List<ListPropertyValueContext<T>>(currentForwardItems.Where(i => !i.IsRemoved));

            while (remainingItems.Any())
            {
                // Find items that have no "before" items in the remaining set
                var nextItems = remainingItems
                    .Where(item => !remainingItems.Any(r =>
                        item.ItemsBefore.Contains(r.Value?.ToString() ?? string.Empty)))
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
        public virtual void InitializeContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPropertyContext propertyContext)
        {
            if (propertyContext is not ListPropertyContext<T> listPropertyContext)
            {
                throw new InvalidOperationException($"Property context is not a list property context for {PropertyName}");
            }
            List<T>? originalList = GetValue(originalContext.Record);
            var listItems = originalList == null ? new List<ListPropertyValueContext<T>>() :
                originalList
                    .Select((item, index) =>
                    {
                        var listItem = new ListPropertyValueContext<T>(item, originalContext.ModKey.ToString());

                        // Only populate ordering information if required
                        if (RequiresOrdering)
                        {
                            // Add items before this one to ItemsBefore
                            if (index > 0)
                            {
                                listItem.ItemsBefore.AddRange(
                                    originalList.Take(index)
                                        .Select(i => i?.ToString() ?? string.Empty)
                                );
                            }

                            // Add items after this one to ItemsAfter
                            if (index < originalList.Count - 1)
                            {
                                listItem.ItemsAfter.AddRange(
                                    originalList.Skip(index + 1)
                                        .Select(i => i?.ToString() ?? string.Empty)
                                );
                            }
                        }

                        return listItem;
                    })
                    .ToList();

            listPropertyContext.OriginalValueContexts = listItems;
            listPropertyContext.ForwardValueContexts = listItems;
        }

        // Non-generic interface implementations
        void IPropertyHandler.SetValue(IMajorRecord record, object? value)
        {
            if (value is List<object> objectList)
            {
                // Convert List<object> back to List<T>
                var typedList = objectList.Select(item => (T)item).ToList();
                SetValue(record, typedList);
            }
            else
            {
                SetValue(record, (List<T>?)value);
            }
        }

        object? IPropertyHandler.GetValue(IMajorRecordGetter record)
        {
            return GetValue(record);
        }

        bool IPropertyHandler.AreValuesEqual(object? value1, object? value2)
        {
            // Convert List<object> to List<T> if needed
            List<T>? typedValue1 = null;
            List<T>? typedValue2 = null;

            if (value1 is List<object> objectList1)
            {
                typedValue1 = objectList1.Select(item => (T)item).ToList();
            }
            else
            {
                typedValue1 = (List<T>?)value1;
            }

            if (value2 is List<object> objectList2)
            {
                typedValue2 = objectList2.Select(item => (T)item).ToList();
            }
            else
            {
                typedValue2 = (List<T>?)value2;
            }

            // Call the generic version
            var result = AreValuesEqual(typedValue1, typedValue2);
            return result;
        }

        // Non-generic interface implementation for context creation
        IPropertyContext IPropertyHandler.CreatePropertyContext()
        {
            return new ListPropertyContext<T>();
        }
    }
}
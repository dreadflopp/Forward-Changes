using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.PropertyHandlers.Abstracts
{
    public abstract class AbstractListPropertyHandler<T> : IPropertyHandler<List<T>>
    {
        public abstract string PropertyName { get; }
        public bool RequiresFullLoadOrderProcessing => true;
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

            // Step 1: Process removals
            ProcessRemovals(context, recordMod, recordItems, forwardValueContexts);

            // Step 2: Process additions and re-additions
            ProcessAdditions(context, recordMod, recordItems, forwardValueContexts);

            // Step 3: Process handler-specific logic (metadata updates)
            ProcessHandlerSpecificLogic(context, state, listPropertyContext, recordItems, forwardValueContexts);

            // Step 4: Sort according to record order (only if required)
            if (RequiresOrdering)
            {
                SortAccordingToRecord(recordItems, forwardValueContexts);
            }

            // Update the state
            listPropertyContext.ForwardValueContexts = forwardValueContexts;
        }

        private void ProcessRemovals(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            ISkyrimModGetter recordMod,
            List<T> recordItems,
            List<ListPropertyValueContext<T>> forwardValueContexts)
        {
            // ============================================================================
            // SECTION 1: PREPARE DATA STRUCTURES
            // ============================================================================

            // Group active (non-removed) forward items by their values for efficient lookup
            var activeForwardItems = forwardValueContexts.Where(i => !i.IsRemoved).ToList();

            var forwardItemGroups = activeForwardItems.GroupBy(item => item.Value)
                                                     .Select(g => (Item: g.Key, Items: g.ToList()))
                                                     .ToList();

            // Group record items by their values for efficient lookup (used in both sections)
            var recordItemGroups = recordItems.GroupBy(item => item)
                                             .Select(g => (Item: g.Key, Count: g.Count()))
                                             .ToList();

            // ============================================================================
            // SECTION 2: REMOVE EXCESS ITEMS (MORE FORWARD ITEMS THAN RECORD ITEMS)
            // ============================================================================
            // For each unique item in the current record, check if we have too many forward items
            foreach (var recordGroup in recordItemGroups)
            {
                var recordItem = recordGroup.Item;
                var recordCount = recordGroup.Count;

                // Find if this record item exists in our forward contexts
                var forwardGroup = forwardItemGroups.FirstOrDefault(g => IsItemEqual(g.Item, recordItem));
                if (forwardGroup.Item != null)
                {
                    var forwardItems = forwardGroup.Items;
                    var forwardCount = forwardItems.Count;

                    // If we have more forward items than record items, remove the excess
                    while (forwardCount > recordCount)
                    {
                        // Find an item to remove by working backwards through the list
                        // This avoids collection modification issues during enumeration
                        var itemToRemove = null as ListPropertyValueContext<T>;
                        for (int i = forwardItems.Count - 1; i >= 0; i--)
                        {
                            // Only remove items from mods we have permission to modify
                            // (either our own mod or mods we have as masters)
                            if (HasPermissionsToModify(recordMod, forwardItems[i].OwnerMod))
                            {
                                itemToRemove = forwardItems[i];
                                break;
                            }
                        }

                        if (itemToRemove != null)
                        {
                            // Mark the item as removed and transfer ownership to current mod
                            var oldOwner = itemToRemove.OwnerMod;
                            itemToRemove.IsRemoved = true;
                            itemToRemove.OwnerMod = context.ModKey.ToString();
                            forwardCount--;
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Removing item {FormatItem(recordItem)} (was owned by {oldOwner}) Success");
                        }
                        else
                        {
                            // No items can be removed due to permission restrictions
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Cannot remove item {FormatItem(recordItem)} - no permission");
                            break; // Can't remove any more instances of this item
                        }
                    }
                }
            }

            // ============================================================================
            // SECTION 3: REMOVE ITEMS NOT PRESENT IN CURRENT RECORD
            // ============================================================================
            // Find all forward items that are active but don't exist in the current record
            var itemsNotInRecord = forwardValueContexts
                .Where(item => !item.IsRemoved && !recordItemGroups.Any(g => IsItemEqual(g.Item, item.Value)))
                .ToList();

            // Process each item that should be removed
            foreach (var item in itemsNotInRecord)
            {
                // Check if we have permission to remove this item
                if (HasPermissionsToModify(recordMod, item.OwnerMod))
                {
                    // Mark as removed and transfer ownership
                    var oldOwner = item.OwnerMod;
                    item.IsRemoved = true;
                    item.OwnerMod = context.ModKey.ToString();
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Removing item not in record {FormatItem(item.Value)} (was owned by {oldOwner}, new owner: {item.OwnerMod}) Success");
                }
                else
                {
                    // Log permission denial for debugging
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Cannot remove item not in record {FormatItem(item.Value)} - no permission. Current owner: {item.OwnerMod}");
                }
            }

        }

        private void ProcessAdditions(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            ISkyrimModGetter recordMod,
            List<T> recordItems,
            List<ListPropertyValueContext<T>> forwardValueContexts)
        {
            // ============================================================================
            // SECTION 1: PREPARE DATA STRUCTURES
            // ============================================================================

            // Group forward context items by equality (including removed items for un-removal)
            // This includes both active and removed items to handle un-removal scenarios
            var forwardItemGroups = forwardValueContexts.GroupBy(item => item.Value)
                                                       .Select(g => (Item: g.Key, Items: g.ToList()))
                                                       .ToList();

            // Group record items by their values for efficient lookup
            var recordItemGroups = recordItems.GroupBy(item => item)
                                             .Select(g => (Item: g.Key, Count: g.Count()))
                                             .ToList();

            // ============================================================================
            // SECTION 2: PROCESS EACH RECORD ITEM (ADD MISSING OR UN-REMOVE ITEMS)
            // ============================================================================
            // For each unique item in the current record, ensure we have the right number in forward contexts
            foreach (var recordGroup in recordItemGroups)
            {
                var recordItem = recordGroup.Item;
                var recordCount = recordGroup.Count;

                // Find if this record item exists in our forward contexts
                var forwardGroup = forwardItemGroups.FirstOrDefault(g => IsItemEqual(g.Item, recordItem));

                if (forwardGroup.Item != null)
                {
                    // ============================================================================
                    // SECTION 2A: ITEM EXISTS IN FORWARD CONTEXTS - HANDLE COUNT MISMATCH
                    // ============================================================================
                    var forwardItems = forwardGroup.Items;
                    var activeForwardCount = forwardItems.Count(item => !item.IsRemoved);
                    var removedForwardCount = forwardItems.Count(item => item.IsRemoved);

                    // If we need more active items than we currently have
                    while (activeForwardCount < recordCount)
                    {
                        // ============================================================================
                        // SECTION 2A.1: PRIORITIZE UN-REMOVING EXISTING ITEMS
                        // ============================================================================
                        // First, try to un-remove a removed item that we have permission to modify
                        var itemToUnremove = forwardItems.FirstOrDefault(item =>
                            item.IsRemoved && HasPermissionsToModify(recordMod, item.OwnerMod));

                        if (itemToUnremove != null)
                        {
                            // Successfully un-remove an existing item
                            var oldOwner = itemToUnremove.OwnerMod;
                            itemToUnremove.IsRemoved = false;
                            itemToUnremove.OwnerMod = context.ModKey.ToString();
                            activeForwardCount++;
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding back previously removed item {FormatItem(recordItem)} (was owned by {oldOwner}, new owner: {itemToUnremove.OwnerMod}) Success");
                        }
                        else
                        {
                            // ============================================================================
                            // SECTION 2A.2: HANDLE PERMISSION RESTRICTIONS OR ADD NEW ITEMS
                            // ============================================================================
                            // Check if there's a removed item we can't un-remove due to permissions
                            var removedItemWithoutPermission = forwardItems.FirstOrDefault(item => item.IsRemoved);
                            if (removedItemWithoutPermission != null)
                            {
                                // Permission denied - can't un-remove this item
                                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Cannot add back previously removed item {FormatItem(recordItem)} - no permission (owned by {removedItemWithoutPermission.OwnerMod})");
                                break; // Can't add any more instances of this item
                            }
                            else
                            {
                                // ============================================================================
                                // SECTION 2A.3: ADD COMPLETELY NEW ITEM
                                // ============================================================================
                                // No removed item exists, so add as new item
                                var newItem = new ListPropertyValueContext<T>(recordItem, context.ModKey.ToString());
                                forwardValueContexts.Add(newItem);
                                forwardItems.Add(newItem);
                                activeForwardCount++;
                                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding new item {FormatItem(recordItem)} (new owner: {newItem.OwnerMod}) Success");
                            }
                        }
                    }
                }
                else
                {
                    // ============================================================================
                    // SECTION 2B: ITEM DOESN'T EXIST IN FORWARD CONTEXTS - ADD ALL INSTANCES
                    // ============================================================================
                    // Item doesn't exist in forward contexts, add all required instances
                    for (int i = 0; i < recordCount; i++)
                    {
                        var newItem = new ListPropertyValueContext<T>(recordItem, context.ModKey.ToString());
                        forwardValueContexts.Add(newItem);
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding new item {FormatItem(recordItem)} (new owner: {newItem.OwnerMod}) Success");
                    }
                }
            }

        }

        private void SortAccordingToRecord(
            List<T> recordItems,
            List<ListPropertyValueContext<T>> forwardValueContexts)
        {
            // Separate active and removed items
            var activeItems = forwardValueContexts.Where(item => !item.IsRemoved).ToList();
            var removedItems = forwardValueContexts.Where(item => item.IsRemoved).ToList();

            var sortedActiveItems = new List<ListPropertyValueContext<T>>();
            var remainingActiveItems = activeItems.ToList();

            // First, sort active items according to record order
            foreach (var recordItem in recordItems)
            {
                var matchingItems = remainingActiveItems
                    .Where(item => IsItemEqual(item.Value, recordItem))
                    .ToList();

                foreach (var matchingItem in matchingItems)
                {
                    sortedActiveItems.Add(matchingItem);
                    remainingActiveItems.Remove(matchingItem);
                }
            }

            // Then append remaining active items in their current order
            sortedActiveItems.AddRange(remainingActiveItems);

            // Combine sorted active items with removed items
            var finalSortedItems = new List<ListPropertyValueContext<T>>();
            finalSortedItems.AddRange(sortedActiveItems);
            finalSortedItems.AddRange(removedItems);

            // Update the forward contexts list
            forwardValueContexts.Clear();
            forwardValueContexts.AddRange(finalSortedItems);
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
        /// Check if the mod can modify an item. It is able to do so if has the owner mod in its master list
        /// or if the mod is the owner mod.
        /// </summary>
        /// <param name="mod">The mod to check</param>
        /// <param name="ownerMod">The mod that owns the item</param>
        /// <returns></returns>
        protected bool HasPermissionsToModify(ISkyrimModGetter mod, string ownerMod)
        {
            return mod?.MasterReferences.Any(m => m.Master.ToString() == ownerMod) == true || mod?.ModKey.ToString() == ownerMod;
        }

        /// <summary>
        /// Initialize the context for the list property.
        /// </summary>
        /// <param name="originalContext">The original context</param>
        /// <param name="winningContext">The winning context</param>
        /// <param name="propertyContext">The property context</param>
        public virtual void InitializeContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPropertyContext propertyContext)
        {
            if (propertyContext is not ListPropertyContext<T> listPropertyContext)
            {
                throw new InvalidOperationException($"Error: Property context is not a list property context for {PropertyName}");
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

        /// <summary>
        /// Format a value for display in logs. Overrides the default implementation to handle lists properly.
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <returns>The formatted value</returns>
        public string FormatValue(object? value)
        {
            if (value is List<T> list)
            {
                if (list == null || list.Count == 0)
                    return "Empty";

                return string.Join(", ", list.Select(item => FormatItem(item)));
            }
            return value?.ToString() ?? "null";
        }
    }
}
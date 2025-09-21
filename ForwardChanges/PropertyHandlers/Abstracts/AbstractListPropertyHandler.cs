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
    public enum ListOrdering
    {
        None,           // No ordering needed (Keywords, FormLists)
        PreserveModOrder // Preserve exact order from mod that added items (Conditions, Effects)
    }

    public abstract class AbstractListPropertyHandler<T> : IPropertyHandler<List<T>> where T : class
    {
        public abstract string PropertyName { get; }
        public bool RequiresFullLoadOrderProcessing => true;
        protected virtual ListOrdering Ordering => ListOrdering.None;

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

            // Always process removals and additions first (separation of concerns)
            ProcessRemovals(context, recordMod, recordItems, forwardValueContexts);
            ProcessAdditions(context, recordMod, recordItems, forwardValueContexts, GetValue(context.Record));

            // Then apply sorting algorithm if ordering is required
            if (Ordering == ListOrdering.PreserveModOrder)
            {
                ProcessSortingAlgorithm(context, recordMod, recordItems, forwardValueContexts);
            }

            // Step 4: Process handler-specific logic (metadata updates)
            ProcessHandlerSpecificLogic(context, state, listPropertyContext, recordItems, forwardValueContexts);

            // Update the state
            listPropertyContext.ForwardValueContexts = forwardValueContexts;

            // Debug: Show final result (simplified)
            //LogCollector.Add(PropertyName, $"DEBUG UpdatePropertyContext: Final result has {forwardValueContexts.Count} items");

            // Debug: Show detailed result by printing all forward value contexts
            LogCollector.Add(PropertyName, $"DEBUG UpdatePropertyContext: Final result has {forwardValueContexts.Count} items");
            foreach (var item in forwardValueContexts)
            {
                LogCollector.Add(PropertyName, $"DEBUG   - {FormatItem(item.Value)} (owned by {item.OwnerMod}, status: {(item.IsRemoved ? "REMOVED" : "ACTIVE")})");
            }
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
            List<ListPropertyValueContext<T>> forwardValueContexts,
            List<T>? originalRecordItems)
        {
            // ============================================================================
            // SECTION 1: PREPARE DATA STRUCTURES
            // ============================================================================

            // Group forward context items by equality (including removed items for un-removal)
            // This includes both active and removed items to handle un-removal scenarios
            // Use a custom grouping that respects IsItemEqual for proper condition comparison
            var forwardItemGroups = new List<(T Item, List<ListPropertyValueContext<T>> Items)>();

            foreach (var contextItem in forwardValueContexts)
            {
                // Find existing group with equal item
                var existingGroup = forwardItemGroups.FirstOrDefault(g => IsItemEqual(g.Item, contextItem.Value));
                if (existingGroup.Item != null)
                {
                    // Add to existing group
                    existingGroup.Items.Add(contextItem);
                }
                else
                {
                    // Create new group
                    forwardItemGroups.Add((contextItem.Value, new List<ListPropertyValueContext<T>> { contextItem }));
                }
            }

            // Debug: Log the forward item groups
            // LogCollector.Add(PropertyName, $"DEBUG ProcessAdditions: Found {forwardItemGroups.Count} forward item groups:");
            // foreach (var group in forwardItemGroups)
            // {
            //     var activeCount = group.Items.Count(item => !item.IsRemoved);
            //     var removedCount = group.Items.Count(item => item.IsRemoved);
            //     LogCollector.Add(PropertyName, $"DEBUG   - {FormatItem(group.Item)}: {activeCount} active, {removedCount} removed");
            // }

            // Group record items by their values for efficient lookup
            var recordItemGroups = recordItems.GroupBy(item => item)
                                             .Select(g => (Item: g.Key, Count: g.Count()))
                                             .ToList();


            // ============================================================================
            // SECTION 2: PROCESS EACH RECORD ITEM (ADD MISSING OR UN-REMOVE ITEMS)
            // ============================================================================
            // For each item in the current record (in order), ensure we have the right number in forward contexts
            for (int recordIndex = 0; recordIndex < recordItems.Count; recordIndex++)
            {
                var recordItem = recordItems[recordIndex];

                // Count how many instances of this item we need
                var recordCount = recordItems.Count(item => IsItemEqual(item, recordItem));

                // Find if this record item exists in our forward contexts
                var forwardGroup = forwardItemGroups.FirstOrDefault(g => IsItemEqual(g.Item, recordItem));

                // Debug: Log what we're looking for and what we found
                // LogCollector.Add(PropertyName, $"DEBUG ProcessAdditions: Looking for {FormatItem(recordItem)} in forward groups");
                // if (forwardGroup.Item != null)
                // {
                //     LogCollector.Add(PropertyName, $"DEBUG   Found in forward group: {FormatItem(forwardGroup.Item)}");
                // }
                // else
                // {
                //     LogCollector.Add(PropertyName, $"DEBUG   NOT FOUND in forward groups - will add as new item");
                // }

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
                            itemToUnremove.OrderOwnerMod = context.ModKey.ToString(); // Set order ownership for un-removed items
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
                                newItem.OrderOwnerMod = null; // New items - will be set during sorting

                                // Add new item to forward contexts

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
                    // DEBUG: Show current forward contexts state
                    // LogCollector.Add(PropertyName, $"[{PropertyName}] DEBUG {context.ModKey}: Item {FormatItem(recordItem)} not found in forward contexts. Current forward contexts:");
                    // foreach (var ctx in forwardValueContexts)
                    // {
                    //     var status = ctx.IsRemoved ? "REMOVED" : "ACTIVE";
                    //     LogCollector.Add(PropertyName, $"[{PropertyName}] DEBUG   - {FormatItem(ctx.Value)} (owned by {ctx.OwnerMod}, status: {status})");
                    // }

                    // Item doesn't exist in forward contexts, add all required instances
                    for (int i = 0; i < recordCount; i++)
                    {
                        var newItem = new ListPropertyValueContext<T>(recordItem, context.ModKey.ToString());
                        newItem.OrderOwnerMod = null; // New items - will be set during sorting

                        // Add new item to forward contexts

                        forwardValueContexts.Add(newItem);
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding new item {FormatItem(recordItem)} (new owner: {newItem.OwnerMod}) Success");
                    }
                }
            }

        }

        /// <summary>
        /// NEIGHBOR-BASED PARTIAL REORDERING ALGORITHM - DETAILED DOCUMENTATION
        /// 
        /// This is a partial reordering algorithm that allows mods to reorder only specific items
        /// while preserving the original neighbor relationships of items they don't have permission to move.
        /// It uses permission-aware placement and neighbor-based positioning to maintain item relationships.
        /// 
        /// DETAILED ALGORITHM STEPS:
        /// 
        /// STEP 1: GET CURRENT STATE
        /// - Extract all active (non-removed) items from forwardValueContexts
        /// - Log current order for debugging and verification
        /// 
        /// STEP 2: BUILD PERMISSION-AWARE FINAL ORDER
        /// - For each item the mod declares:
        ///   * Find matching instance in current active items
        ///   * Check if mod has permission to reorder this item (HasPermissionsToModify)
        ///   * If permission granted: Add to finalOrder list
        ///   * If permission denied: Leave in remainingInstances list
        /// - Log which items were included/excluded and why
        /// 
        /// STEP 3: PLACE REMAINING ITEMS BASED ON ORIGINAL "BEFORE" RELATIONSHIPS
        /// - For each item in remainingInstances (excluding new items with OrderOwnerMod == null):
        ///   * Find all items that were originally before this item
        ///   * Place the item after the last "before" item that's already in finalOrder
        ///   * This preserves the original relative positions of items the mod can't reorder
        /// - Keep new items (OrderOwnerMod == null) in remainingInstances for Step 5
        /// 
        /// STEP 4: ASSIGN ORDER OWNERSHIP
        /// - For each item in finalOrder, check if it moved by comparing neighbor relationships
        /// - An item has moved if BOTH its "before" AND "after" neighbors have changed
        /// - Only check items that existed before this mod (OrderOwnerMod != null)
        /// - For each item that moved: Take OrderOwnerMod ownership
        /// - Log ownership changes for debugging
        /// 
        /// STEP 5: PLACE NEW ITEMS BASED ON MOD'S DECLARED ORDER
        /// - Sort remainingInstances (new items) according to the mod's declared order
        /// - For each new item, find what should be before it according to mod's wishes
        /// - Place the item after the last "before" item that's already in finalOrder
        /// - Set OrderOwnerMod for new items (they have OrderOwnerMod == null)
        /// 
        /// STEP 6: UPDATE FORWARD CONTEXTS
        /// - Replace active items in forwardValueContexts with final order
        /// - Preserve removed items at the end
        /// - Log final order for verification
        /// 
        /// KEY FEATURES:
        /// - Mod-Intent Respect: Places desired items in the mod's declared sequence
        /// - Partial Reordering: Can reorder some items while preserving others
        /// - Permission Granularity: Respects permissions per-item, not all-or-nothing
        /// - Before-Relationship Preservation: Remaining items maintain original "before" relationships
        /// - Instance-Aware: Uses actual instances, not just values, for accurate tracking
        /// - Smart Positioning: Finds optimal positions based on original "before" context
        /// - Ownership Tracking: Only takes ownership of items that actually moved
        /// 
        /// TEST CASES:
        /// 
        /// Case 1 - Full reorder with permission:
        /// - Current: A, B
        /// - Mod wants: B, A (has permission for both)
        /// - Desired order: B, A
        /// - Remaining: (none)
        /// - Final: B, A (both items moved, both get order ownership)
        /// 
        /// Case 2 - Partial reorder with mixed permissions:
        /// - Current: A, B, C
        /// - Mod wants: B, A (has permission for A,B but not C)
        /// - Desired order: B, A
        /// - Remaining: C
        /// - Final: B, A, C (C maintains original neighbor relationships)
        /// 
        /// Case 3 - No permission scenario:
        /// - Current: A, B
        /// - Mod wants: B, A (no permission for either)
        /// - Desired order: (none)
        /// - Remaining: A, B
        /// - Final: A, B (original order preserved)
        /// 
        /// Case 4 - Complex partial reorder:
        /// - Current: A, B, C, D, E
        /// - Mod wants: C, A, E (has permission for A,C,E but not B,D)
        /// - Desired order: C, A, E
        /// - Remaining: B, D
        /// - Final: C, A, B, D, E (B,D maintain original neighbor relationships)
        /// 
        /// Case 5 - Duplicates with reordering:
        /// - Current: A, A, B
        /// - Mod wants: A, B, A (has permission for all)
        /// - Desired order: A, B, A
        /// - Remaining: (none)
        /// - Final: A, B, A (reordered according to mod's sequence)
        /// 
        /// Case 6 - Pure reorder with relative position changes:
        /// - Current: A, B, C, D
        /// - Mod wants: D, A, B, C (has permission for all)
        /// - Desired order: D, A, B, C
        /// - Remaining: (none)
        /// - Final: D, A, B, C (only D changed relative position, gets order ownership)
        /// 
        /// Case 7 - Partial permission reorder:
        /// - Current: A, B, C, D
        /// - Mod wants: D, A, B, C (has permission for A,B,C but not D)
        /// - Desired order: A, B, C
        /// - Remaining: D
        /// - Final: A, B, C, D (D stays at end, maintains original position)
        /// </summary>
        private void ProcessSortingAlgorithm(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            ISkyrimModGetter recordMod,
            List<T> recordItems,
            List<ListPropertyValueContext<T>> forwardValueContexts)
        {
            LogCollector.Add(PropertyName, $"=== NEIGHBOR-BASED SORTING ALGORITHM: {context.ModKey} ===");
            LogCollector.Add(PropertyName, $"Mod wants: {string.Join(", ", recordItems.Select((item, i) => $"[{i}]{FormatItem(item)}"))}");

            var modName = context.ModKey.ToString();

            // ============================================================================
            // STEP 1: GET CURRENT STATE
            // ============================================================================
            var currentActiveItems = forwardValueContexts.Where(item => !item.IsRemoved).ToList();
            LogCollector.Add(PropertyName, $"STEP 1 - Current active items: {string.Join(", ", currentActiveItems.Select((item, i) => $"[{i}]{FormatItem(item.Value)}"))}");

            // ============================================================================
            // STEP 2: BUILD PERMISSION-AWARE FINAL ORDER
            // ============================================================================
            var finalOrder = new List<ListPropertyValueContext<T>>();
            var remainingInstances = new List<ListPropertyValueContext<T>>(currentActiveItems);

            foreach (var declaredValue in recordItems)
            {
                var match = remainingInstances.FirstOrDefault(inst => IsItemEqual(inst.Value, declaredValue));
                if (match != null)
                {
                    // Only add to final order if we have permission to reorder this item
                    if (HasPermissionsToModify(recordMod, match.OrderOwnerMod))
                    {
                        finalOrder.Add(match);
                        remainingInstances.Remove(match);
                        LogCollector.Add(PropertyName, $"STEP 2 - Added to final order: {FormatItem(match.Value)} (has permission)");
                    }
                    else
                    {
                        LogCollector.Add(PropertyName, $"STEP 2 - Skipped from final order: {FormatItem(match.Value)} (no permission, owner: {match.OrderOwnerMod})");
                    }
                }
            }

            LogCollector.Add(PropertyName, $"STEP 2 - Final order: {string.Join(", ", finalOrder.Select((item, i) => $"[{i}]{FormatItem(item.Value)}"))}");
            LogCollector.Add(PropertyName, $"STEP 2 - Remaining instances: {string.Join(", ", remainingInstances.Select((item, i) => $"[{i}]{FormatItem(item.Value)}"))}");

            // ============================================================================
            // STEP 3: PLACE REMAINING ITEMS BASED ON ORIGINAL "BEFORE" RELATIONSHIPS
            // ============================================================================
            // Separate existing items from new items
            var existingRemainingItems = remainingInstances.Where(item => item.OrderOwnerMod != null).ToList();
            var newRemainingItems = remainingInstances.Where(item => item.OrderOwnerMod == null).ToList();

            foreach (var remainingItem in existingRemainingItems)
            {
                int position = FindPositionBasedOnBeforeRelationships(remainingItem, currentActiveItems, finalOrder);
                finalOrder.Insert(position, remainingItem);
                LogCollector.Add(PropertyName, $"STEP 3 - Placed existing remaining item: {FormatItem(remainingItem.Value)} at position {position}");
            }

            LogCollector.Add(PropertyName, $"STEP 3 - Final order after existing items placement: {string.Join(", ", finalOrder.Select((item, i) => $"[{i}]{FormatItem(item.Value)}"))}");
            LogCollector.Add(PropertyName, $"STEP 3 - New items to place in Step 5: {string.Join(", ", newRemainingItems.Select(item => FormatItem(item.Value)))}");

            // ============================================================================
            // STEP 4: ASSIGN ORDER OWNERSHIP
            // ============================================================================
            // Check which items actually moved by comparing neighbor relationships
            // Only check items that existed before this mod (OrderOwnerMod != null)
            foreach (var finalItem in finalOrder)
            {
                // Find this item's neighbors in original order
                var originalBefore = GetItemBefore(finalItem, currentActiveItems);
                var originalAfter = GetItemAfter(finalItem, currentActiveItems);

                // Find this item's neighbors in final order
                var finalBefore = GetItemBefore(finalItem, finalOrder);
                var finalAfter = GetItemAfter(finalItem, finalOrder);

                // Check if BOTH neighbors changed (indicating this item moved)
                bool beforeChanged = !AreItemsEqual(originalBefore, finalBefore);
                bool afterChanged = !AreItemsEqual(originalAfter, finalAfter);

                if (beforeChanged && afterChanged)
                {
                    // This item moved - take order ownership
                    finalItem.OrderOwnerMod = modName;
                    LogCollector.Add(PropertyName, $"STEP 4 - Order ownership: {FormatItem(finalItem.Value)} moved (neighbors changed) → {modName}");
                    LogCollector.Add(PropertyName, $"    Original: before={FormatItem(originalBefore?.Value)}, after={FormatItem(originalAfter?.Value)}");
                    LogCollector.Add(PropertyName, $"    Final: before={FormatItem(finalBefore?.Value)}, after={FormatItem(finalAfter?.Value)}");
                }
                else
                {
                    LogCollector.Add(PropertyName, $"STEP 4 - No move: {FormatItem(finalItem.Value)} (neighbors unchanged)");
                }
            }

            // ============================================================================
            // STEP 5: PLACE NEW ITEMS BASED ON MOD'S DECLARED ORDER
            // ============================================================================
            // Sort new items according to the mod's declared order
            var sortedNewItems = new List<ListPropertyValueContext<T>>();
            foreach (var declaredValue in recordItems)
            {
                var newItem = newRemainingItems.FirstOrDefault(item => IsItemEqual(item.Value, declaredValue));
                if (newItem != null)
                {
                    sortedNewItems.Add(newItem);
                }
            }

            LogCollector.Add(PropertyName, $"STEP 5 - New items in mod's declared order: {string.Join(", ", sortedNewItems.Select(item => FormatItem(item.Value)))}");

            // Place each new item based on what should be before it according to mod's wishes
            foreach (var newItem in sortedNewItems)
            {
                int position = FindPositionForNewItem(newItem, recordItems, finalOrder);
                finalOrder.Insert(position, newItem);
                newItem.OrderOwnerMod = modName; // Set order ownership for new items
                LogCollector.Add(PropertyName, $"STEP 5 - Placed new item: {FormatItem(newItem.Value)} at position {position} → {modName}");
            }

            // ============================================================================
            // STEP 6: UPDATE FORWARD CONTEXTS
            // ============================================================================
            var removedItems = forwardValueContexts.Where(x => x.IsRemoved).ToList();
            forwardValueContexts.Clear();
            forwardValueContexts.AddRange(finalOrder);
            forwardValueContexts.AddRange(removedItems);

            LogCollector.Add(PropertyName, $"STEP 6 - Final order: {string.Join(", ", finalOrder.Select((item, i) => $"[{i}]{FormatItem(item.Value)}"))}");

        }

        /// <summary>
        /// Finds the position for a remaining item based on its original "before" relationships.
        /// This method ensures that items maintain their original relative positions to items that were before them.
        /// Uses actual item relationships, not indexes, for robust positioning.
        /// </summary>
        /// <param name="itemToPlace">The remaining item we want to place</param>
        /// <param name="originalOrder">The original order of all items</param>
        /// <param name="currentFinalOrder">The current state of the final order being built</param>
        /// <returns>The position (index) to insert the item</returns>
        private int FindPositionBasedOnBeforeRelationships(
            ListPropertyValueContext<T> itemToPlace,
            List<ListPropertyValueContext<T>> originalOrder,
            List<ListPropertyValueContext<T>> currentFinalOrder)
        {
            // Find all items that were originally before this item (by comparing values, not indexes)
            var originalBeforeItems = new List<ListPropertyValueContext<T>>();
            bool foundItemToPlace = false;

            foreach (var originalItem in originalOrder)
            {
                if (IsItemEqual(originalItem.Value, itemToPlace.Value))
                {
                    foundItemToPlace = true;
                    break; // Stop when we find the item we're placing
                }
                originalBeforeItems.Add(originalItem);
            }

            if (!foundItemToPlace)
            {
                // Item not found in original order (new item added by this mod), place at the end
                LogCollector.Add(PropertyName, $"    FindPosition: {FormatItem(itemToPlace.Value)} not found in original order (new item), placing at end");
                return currentFinalOrder.Count;
            }

            LogCollector.Add(PropertyName, $"    FindPosition: {FormatItem(itemToPlace.Value)} - found {originalBeforeItems.Count} items that were originally before it");
            LogCollector.Add(PropertyName, $"      Original before items: {string.Join(", ", originalBeforeItems.Select(item => FormatItem(item.Value)))}");

            // Find the position after the last "before" item that's already in finalOrder
            int position = 0;
            foreach (var beforeItem in originalBeforeItems)
            {
                int beforeIndex = currentFinalOrder.FindIndex(item => IsItemEqual(item.Value, beforeItem.Value));
                if (beforeIndex != -1)
                {
                    // This "before" item is already placed, position should be after it
                    position = Math.Max(position, beforeIndex + 1);
                    LogCollector.Add(PropertyName, $"        Found 'before' item {FormatItem(beforeItem.Value)} at position {beforeIndex}, updating position to {position}");
                }
            }

            // Ensure we don't exceed the current list length
            position = Math.Min(position, currentFinalOrder.Count);

            LogCollector.Add(PropertyName, $"    FindPosition: Final position for {FormatItem(itemToPlace.Value)}: {position}");
            return position;
        }

        /// <summary>
        /// Finds the position for a new item based on the mod's declared order.
        /// This method ensures that new items are placed according to the mod's intentions.
        /// </summary>
        /// <param name="newItem">The new item we want to place</param>
        /// <param name="recordItems">The mod's declared items in order</param>
        /// <param name="currentFinalOrder">The current state of the final order being built</param>
        /// <returns>The position (index) to insert the item</returns>
        private int FindPositionForNewItem(
            ListPropertyValueContext<T> newItem,
            List<T> recordItems,
            List<ListPropertyValueContext<T>> currentFinalOrder)
        {
            // Find the position of this new item in the mod's declared order
            int declaredIndex = recordItems.FindIndex(item => IsItemEqual(item, newItem.Value));
            if (declaredIndex == -1)
            {
                // Item not found in declared order (shouldn't happen), place at the end
                LogCollector.Add(PropertyName, $"    FindPositionForNewItem: {FormatItem(newItem.Value)} not found in declared order, placing at end");
                return currentFinalOrder.Count;
            }

            LogCollector.Add(PropertyName, $"    FindPositionForNewItem: {FormatItem(newItem.Value)} is at declared index {declaredIndex}");

            // Find all items that should be before this new item according to mod's declared order
            var declaredBeforeItems = new List<T>();
            for (int i = 0; i < declaredIndex; i++)
            {
                declaredBeforeItems.Add(recordItems[i]);
            }

            LogCollector.Add(PropertyName, $"      Declared before items: {string.Join(", ", declaredBeforeItems.Select(item => FormatItem(item)))}");

            // Find the position after the last "before" item that's already in finalOrder
            int position = 0;
            foreach (var beforeItem in declaredBeforeItems)
            {
                int beforeIndex = currentFinalOrder.FindIndex(item => IsItemEqual(item.Value, beforeItem));
                if (beforeIndex != -1)
                {
                    // This "before" item is already placed, position should be after it
                    position = Math.Max(position, beforeIndex + 1);
                    LogCollector.Add(PropertyName, $"        Found 'before' item {FormatItem(beforeItem)} at position {beforeIndex}, updating position to {position}");
                }
            }

            // Ensure we don't exceed the current list length
            position = Math.Min(position, currentFinalOrder.Count);

            LogCollector.Add(PropertyName, $"    FindPositionForNewItem: Final position for {FormatItem(newItem.Value)}: {position}");
            return position;
        }

        /// <summary>
        /// Gets the item that comes before the specified item in the given list.
        /// Returns null if the item is first or not found.
        /// </summary>
        private ListPropertyValueContext<T>? GetItemBefore(
            ListPropertyValueContext<T> item,
            List<ListPropertyValueContext<T>> list)
        {
            int index = list.FindIndex(i => IsItemEqual(i.Value, item.Value));
            if (index <= 0) return null;
            return list[index - 1];
        }

        /// <summary>
        /// Gets the item that comes after the specified item in the given list.
        /// Returns null if the item is last or not found.
        /// </summary>
        private ListPropertyValueContext<T>? GetItemAfter(
            ListPropertyValueContext<T> item,
            List<ListPropertyValueContext<T>> list)
        {
            int index = list.FindIndex(i => IsItemEqual(i.Value, item.Value));
            if (index < 0 || index >= list.Count - 1) return null;
            return list[index + 1];
        }

        /// <summary>
        /// Compares two items for equality, handling null values.
        /// </summary>
        private bool AreItemsEqual(ListPropertyValueContext<T>? item1, ListPropertyValueContext<T>? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;
            return IsItemEqual(item1.Value, item2.Value);
        }

        /// <summary>
        /// Custom equality comparer that wraps the IsItemEqual method for use with collections.
        /// This allows us to use custom equality logic in LINQ operations and dictionary lookups.
        /// </summary>
        private class ItemEqualityComparer<TItem> : IEqualityComparer<TItem>
        {
            private readonly Func<TItem, TItem, bool> _comparer;

            public ItemEqualityComparer(Func<TItem, TItem, bool> comparer)
            {
                _comparer = comparer;
            }

            public bool Equals(TItem? x, TItem? y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                return _comparer(x, y);
            }

            public int GetHashCode(TItem obj)
            {
                return obj?.GetHashCode() ?? 0;
            }
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
        protected bool HasPermissionsToModify(ISkyrimModGetter mod, string? ownerMod)
        {
            if (ownerMod == null)
            {
                return false;
            }
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
            // InitializeContext for {PropertyName}

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
                        listItem.OrderIndex = index; // Set initial order index
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
                try
                {
                    typedValue1 = objectList1.Select(item => (T)item).ToList();
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                typedValue1 = (List<T>?)value1;
            }

            if (value2 is List<object> objectList2)
            {
                try
                {
                    typedValue2 = objectList2.Select(item => (T)item).ToList();
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                typedValue2 = (List<T>?)value2;
            }

            // Call the generic version
            return AreValuesEqual(typedValue1, typedValue2);
        }

        // Non-generic interface implementation for context creation
        IPropertyContext IPropertyHandler.CreatePropertyContext()
        {
            return new ListPropertyContext<T>();
        }

        /// <summary>
        /// Format a value for display in logs. Override this method to provide custom formatting for lists.
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <returns>The formatted value</returns>
        public virtual string FormatValue(object? value)
        {
            if (value is List<T> list)
            {
                if (list == null || list.Count == 0)
                    return "Empty";

                return string.Join(", ", list.Select(item => FormatItem(item)));
            }
            else if (value is List<object> objectList)
            {
                // Convert List<object> to List<T> for formatting
                var typedList = objectList.Select(item => (T)item).ToList();
                return string.Join(", ", typedList.Select(item => FormatItem(item)));
            }
            return value?.ToString() ?? "null";
        }

    }
}
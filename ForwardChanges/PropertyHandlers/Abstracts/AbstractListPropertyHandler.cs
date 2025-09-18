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

    public abstract class AbstractListPropertyHandler<T> : IPropertyHandler<List<T>>
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
            LogCollector.Add(PropertyName, $"DEBUG UpdatePropertyContext: Final result has {forwardValueContexts.Count} items");
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
            LogCollector.Add(PropertyName, $"DEBUG ProcessAdditions: Found {forwardItemGroups.Count} forward item groups:");
            foreach (var group in forwardItemGroups)
            {
                var activeCount = group.Items.Count(item => !item.IsRemoved);
                var removedCount = group.Items.Count(item => item.IsRemoved);
                LogCollector.Add(PropertyName, $"DEBUG   - {FormatItem(group.Item)}: {activeCount} active, {removedCount} removed");
            }

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
                LogCollector.Add(PropertyName, $"DEBUG ProcessAdditions: Looking for {FormatItem(recordItem)} in forward groups");
                if (forwardGroup.Item != null)
                {
                    LogCollector.Add(PropertyName, $"DEBUG   Found in forward group: {FormatItem(forwardGroup.Item)}");
                }
                else
                {
                    LogCollector.Add(PropertyName, $"DEBUG   NOT FOUND in forward groups - will add as new item");
                }

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
                    LogCollector.Add(PropertyName, $"[{PropertyName}] DEBUG {context.ModKey}: Item {FormatItem(recordItem)} not found in forward contexts. Current forward contexts:");
                    foreach (var ctx in forwardValueContexts)
                    {
                        var status = ctx.IsRemoved ? "REMOVED" : "ACTIVE";
                        LogCollector.Add(PropertyName, $"[{PropertyName}] DEBUG   - {FormatItem(ctx.Value)} (owned by {ctx.OwnerMod}, status: {status})");
                    }

                    // Item doesn't exist in forward contexts, add all required instances
                    for (int i = 0; i < recordCount; i++)
                    {
                        var newItem = new ListPropertyValueContext<T>(recordItem, context.ModKey.ToString());

                        // Add new item to forward contexts

                        forwardValueContexts.Add(newItem);
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding new item {FormatItem(recordItem)} (new owner: {newItem.OwnerMod}) Success");
                    }
                }
            }

        }



        /// <summary>
        /// Process the sorting algorithm for list properties that require ordering.
        /// This algorithm focuses solely on ordering items according to the mod's intentions,
        /// after additions and removals have already been processed by ProcessAdditions and ProcessRemovals.
        /// 
        /// ALGORITHM OVERVIEW:
        /// This algorithm handles ordering of items using instance-aware matching to properly handle
        /// duplicate items. It respects mod master relationships for ownership changes and implements
        /// pure reorder detection using multiset (counts) equality for cases where a mod wants to 
        /// reorder existing items.
        /// 
        /// ALGORITHM STEPS:
        /// 1. Get current active items (after ProcessAdditions/ProcessRemovals)
        /// 1.5. CRITICAL FIX A: Snapshot prior active items before any re-adds/creates for accurate position tracking
        /// 2. Instance-aware matching: Map declared values to existing instances using greedy left-to-right matching into slots
        /// 2.5. Handle unmatched declared occurrences: Re-add removed instances or create new instances (robustness)
        /// 2.6. CRITICAL FIX B: Track created/re-added instances this pass for proper permission handling
        /// 3. Check for pure reorder using multiset equality (same counts, different order) - grants ownership to mod
        /// 4. Process ownership changes for instances that moved positions using filtered subsequence comparison (with permission checks)
        /// 5. Build final order respecting mod's placement intentions, skipping locked instances
        /// 6. Reindex OrderIndex for all active items
        /// 
        /// KEY RULES:
        /// - Instance-aware: Each occurrence is treated as a distinct instance, even if values are equal
        /// - Greedy matching: Left-to-right matching of declared values to existing instances into slots
        /// - Robust handling: Unmatched declared values are handled as re-adds or new additions
        /// - CRITICAL FIX A: Prior positions computed from snapshot before re-adds/creates to avoid false position changes
        /// - CRITICAL FIX B: Explicit tracking of created/re-added instances this pass for proper permission handling
        /// - Pure reorder: If multiset of declared active values equals current active values in different order, mod takes ownership
        /// - Filtered comparison: Position changes are detected by comparing filtered subsequences (matched instances only)
        /// - Permission-based ownership: Moves require mod to have item's current owner in masters OR item was created/re-added this pass
        /// - Locked instances: Items the mod cannot move are preserved in their original positions
        /// - Deterministic ordering: Final patch uses OrderIndex for consistent output
        /// 
        /// TEST CASES VERIFIED:
        /// 
        /// Case 1 - Duplicates sequence:
        /// - Mod1: [A,B,C] → A1(Mod1), B1(Mod1), C1(Mod1)
        /// - Mod2: [A,A,B,C] → A1(Mod1), A2(Mod2), B1(Mod1), C1(Mod1) (A2 added)
        /// - Mod3: [A,B,C] → A1(Mod1), B1(Mod1), C1(Mod1) (A2 removed)
        /// - Final: A(Mod2), B(Mod1), C(Mod1) (surviving A owned by Mod2)
        /// 
        /// Case 2 - Pure reorder swap:
        /// - Mod1: [A,B] → A(Mod1), B(Mod1)
        /// - Mod2: [B,A] → Pure reorder detected → B(Mod2), A(Mod2)
        /// - Final: B(Mod2), A(Mod2) (both owned by Mod2)
        /// 
        /// Case 3 - Add C (no stealing):
        /// - Mod1: [A,B] → A(Mod1), B(Mod1)
        /// - Mod2: [C,A,B] → C(Mod2), A(Mod1), B(Mod1) (C added, A/B order unchanged)
        /// - Final: C(Mod2), A(Mod1), B(Mod1) (C owned by Mod2, A/B remain Mod1)
        /// 
        /// Case 4 - Complex removal scenario:
        /// - After Mod3: A(Mod1), B(Mod1), C(Mod3), D(Mod3)
        /// - Mod4: [A,B] → No change (cannot remove C/D)
        /// - Mod5: [A,B,E,C] → A(Mod1), B(Mod1), E(Mod5), C(Mod3) (D removed, E added)
        /// - Final: A(Mod1), B(Mod1), E(Mod5), C(Mod3) (D removed)
        /// 
        /// Case 5 - Pure reorder after removal:
        /// - After Mod4: A(Mod1), C(Mod3) (B removed)
        /// - Mod5: [C,A] → Pure reorder → C(Mod5), A(Mod5)
        /// - Final: C(Mod5), A(Mod5) (both owned by Mod5, B remains removed)
        /// 
        /// Case 6 - A removed, C added:
        /// - Mod1: [A,B] → A(Mod1), B(Mod1)
        /// - Mod2: [B,C] → B(Mod1), C(Mod2) (A removed, C added)
        /// - Mod3: [A,B] → No change (cannot re-add A)
        /// - Final: B(Mod1), C(Mod2) (A removed)
        /// 
        /// Case 7 - Pure reorder with duplicates:
        /// - Mod1: [A,A,B] → A1(Mod1), A2(Mod1), B(Mod1)
        /// - Mod2: [A,B,A] → Pure reorder → A(Mod2), B(Mod2), A(Mod2)
        /// - Final: A(Mod2), B(Mod2), A(Mod2) (all owned by Mod2)
        /// </summary>
        /// <param name="context">The mod context</param>
        /// <param name="recordMod">The mod being processed</param>
        /// <param name="recordItems">The current items in the record</param>
        /// <param name="forwardValueContexts">The forward value contexts</param>
        private void ProcessSortingAlgorithm(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            ISkyrimModGetter recordMod,
            List<T> recordItems,
            List<ListPropertyValueContext<T>> forwardValueContexts)
        {
            LogCollector.Add(PropertyName, $"=== SORTING ALGORITHM DEBUG: {context.ModKey} ===");
            LogCollector.Add(PropertyName, $"Mod wants: {string.Join(", ", recordItems.Select((item, i) => $"[{i}]{FormatItem(item)}"))}");
            LogCollector.Add(PropertyName, $"Current forward (after additions/removals): {string.Join(", ", forwardValueContexts.Select((item, i) => $"[{i}]{FormatItem(item.Value)}"))}");

            var modName = context.ModKey.ToString();
            var modMasters = recordMod.MasterReferences.Select(m => m.Master.ToString()).ToHashSet();

            // Step 1: Get current active items (after ProcessAdditions/ProcessRemovals)
            var currentActiveItems = forwardValueContexts.Where(item => !item.IsRemoved).ToList();
            var currentActiveValues = currentActiveItems.Select(item => item.Value).ToList();

            LogCollector.Add(PropertyName, $"Current active items: {currentActiveItems.Count}");

            // CRITICAL FIX A: Snapshot the active items as they existed BEFORE we do any readds/creates
            // We'll use this snapshot to compute "prior positions" that reflect the load-order state before this mod
            var priorActiveSnapshot = currentActiveItems.ToList();
            var createdOrReaddedThisPass = new HashSet<ListPropertyValueContext<T>>();

            // Step 2: Instance-aware matching with declared slots (preserve positions)
            var unmatchedActiveInstances = new List<ListPropertyValueContext<T>>(currentActiveItems);
            var declaredSlots = new List<ListPropertyValueContext<T>?>(recordItems.Count);

            // Initialize slots with nulls
            for (int i = 0; i < recordItems.Count; i++) declaredSlots.Add(null);

            LogCollector.Add(PropertyName, $"Starting slot-based matching - current active: {currentActiveItems.Count}, declared slots: {recordItems.Count}");

            // Greedy left-to-right matching into slots
            for (int i = 0; i < recordItems.Count; i++)
            {
                var declaredValue = recordItems[i];
                var match = unmatchedActiveInstances.FirstOrDefault(inst => IsItemEqual(inst.Value, declaredValue));

                if (match != null)
                {
                    declaredSlots[i] = match;
                    unmatchedActiveInstances.Remove(match); // consume that instance
                    LogCollector.Add(PropertyName, $"  Matched slot[{i}]: {FormatItem(declaredValue)} -> instance (owner: {match.OwnerMod})");
                }
                else
                {
                    // Check if there's a removed instance with the same value that this mod cannot re-add
                    var removedInstance = forwardValueContexts.FirstOrDefault(inst =>
                        inst.IsRemoved && IsItemEqual(inst.Value, declaredValue) && !modMasters.Contains(inst.OwnerMod));

                    if (removedInstance != null)
                    {
                        // This mod cannot re-add this removed item - mark slot as blocked
                        LogCollector.Add(PropertyName, $"  Slot[{i}] blocked: {FormatItem(declaredValue)} (cannot re-add, owned by {removedInstance.OwnerMod})");
                        declaredSlots[i] = removedInstance; // Use removed instance as placeholder to indicate blocked slot
                    }
                    else
                    {
                        // leave slot null for now (will create/re-add later)
                        LogCollector.Add(PropertyName, $"  Slot[{i}] unmatched: {FormatItem(declaredValue)} (placeholder for add/re-add)");
                    }
                }
            }

            // Build matchedInstancesInModOrder preserving declared positions (nulls and blocked slots omitted)
            var matchedInstancesInModOrder = declaredSlots.Where(s => s != null && !s.IsRemoved).Select(s => s!).ToList();

            // Gather unmatchedDeclaredValues (with their slot indices) for later re-add/create (exclude blocked slots)
            var unmatchedDeclaredIndices = declaredSlots
                .Select((slot, idx) => (slot, idx))
                .Where(x => x.slot == null)
                .Select(x => x.idx)
                .ToList();

            LogCollector.Add(PropertyName, $"Slot matching complete - matched: {matchedInstancesInModOrder.Count}, unmatched slots: {unmatchedDeclaredIndices.Count}, remaining active: {unmatchedActiveInstances.Count}");

            // Step 2.5: Fill null slots by re-adding eligible removed instances or creating new ones
            if (unmatchedDeclaredIndices.Count > 0)
            {
                LogCollector.Add(PropertyName, $"Handling {unmatchedDeclaredIndices.Count} unmatched declared slots (re-add/create)");

                foreach (var slotIndex in unmatchedDeclaredIndices)
                {
                    var declaredValue = recordItems[slotIndex];

                    // Prefer to re-add: find a removed instance with same value where this mod has permission to re-add
                    var candidateRemoved = forwardValueContexts.FirstOrDefault(inst =>
                        inst.IsRemoved && IsItemEqual(inst.Value, declaredValue) && modMasters.Contains(inst.OwnerMod));

                    if (candidateRemoved != null)
                    {
                        candidateRemoved.IsRemoved = false;
                        candidateRemoved.OwnerMod = modName;
                        declaredSlots[slotIndex] = candidateRemoved;
                        unmatchedActiveInstances.Remove(candidateRemoved); // just in case
                        createdOrReaddedThisPass.Add(candidateRemoved); // CRITICAL FIX B

                        LogCollector.Add(PropertyName, $"  Re-added at slot[{slotIndex}] {FormatItem(declaredValue)} (now owner: {candidateRemoved.OwnerMod})");
                        continue;
                    }

                    // Otherwise create a new instance and put it in the slot
                    var newInst = new ListPropertyValueContext<T>(declaredValue, modName) { IsRemoved = false };
                    forwardValueContexts.Add(newInst); // keep global set updated
                    declaredSlots[slotIndex] = newInst;
                    createdOrReaddedThisPass.Add(newInst); // CRITICAL FIX B

                    LogCollector.Add(PropertyName, $"  Created new instance at slot[{slotIndex}] {FormatItem(declaredValue)} (owner: {modName})");
                }

                // Rebuild the matchedInstancesInModOrder now that declaredSlots are filled
                matchedInstancesInModOrder = declaredSlots.Where(s => s != null).Select(s => s!).ToList();

                // Refresh currentActiveItems because we changed forwardValueContexts (and created/readded)
                currentActiveItems = forwardValueContexts.Where(item => !item.IsRemoved).ToList();
            }

            // Step 3: Check for pure reorder using multiset (counts) equality
            // Refresh currentActiveValues to match currentActiveItems (in case we re-added/created)
            currentActiveValues = currentActiveItems.Select(item => item.Value).ToList();

            var itemComparer = new ItemEqualityComparer<T>(IsItemEqual);
            var currentValueCounts = CountValuesList(currentActiveValues, itemComparer);
            var matchedValueCounts = CountValuesList(matchedInstancesInModOrder.Select(i => i.Value).ToList(), itemComparer);

            bool sameMultiset = CountsEqualDict(currentValueCounts, matchedValueCounts, itemComparer);
            bool isOrderDifferent = !matchedInstancesInModOrder.Select(i => i.Value).SequenceEqual(currentActiveValues, itemComparer);
            bool isPureReorder = sameMultiset && isOrderDifferent;

            LogCollector.Add(PropertyName, $"Is pure reorder: {isPureReorder}");

            if (isPureReorder)
            {
                // Pure reorder: mod takes ownership of all items and uses its order
                LogCollector.Add(PropertyName, "Processing pure reorder");

                foreach (var instance in matchedInstancesInModOrder)
                {
                    instance.OwnerMod = modName;
                }

                // Build final order based on mod's order (matched instances)
                var pureReorderFinalOrder = new List<ListPropertyValueContext<T>>();
                pureReorderFinalOrder.AddRange(matchedInstancesInModOrder);

                // Add any remaining active instances that weren't matched (shouldn't happen in pure reorder, but safety check)
                // Rebuild from snapshot for exactness - these are items that existed before this mod but weren't in the mod's list
                var remainingFromSnapshot = priorActiveSnapshot.Where(inst => !matchedInstancesInModOrder.Contains(inst)).ToList();
                pureReorderFinalOrder.AddRange(remainingFromSnapshot);

                // Replace active items with final order
                var removedItems = forwardValueContexts.Where(item => item.IsRemoved).ToList();
                forwardValueContexts.Clear();
                forwardValueContexts.AddRange(pureReorderFinalOrder);
                forwardValueContexts.AddRange(removedItems);

                // Reindex
                ReindexOrderIndices(forwardValueContexts);
                LogCollector.Add(PropertyName, $"Pure reorder result: {string.Join(", ", pureReorderFinalOrder.Select((item, i) => $"[{i}]{FormatItem(item.Value)}"))}");
                LogCollector.Add(PropertyName, "=== END SORTING ALGORITHM DEBUG ===");
                return;
            }

            // Step 4: Non-pure case - process ownership changes for items that moved positions
            LogCollector.Add(PropertyName, "Processing non-pure case - checking for ownership changes");

            // Track items that cannot be moved by this mod (locked items)
            var lockedItems = new HashSet<ListPropertyValueContext<T>>();

            // CRITICAL FIX A: Build the prior filtered order from the snapshot (state before creating/re-adding)
            var priorFilteredOrder = priorActiveSnapshot.Where(inst => matchedInstancesInModOrder.Contains(inst)).ToList();
            var priorFilteredIndex = new Dictionary<ListPropertyValueContext<T>, int>();
            for (int i = 0; i < priorFilteredOrder.Count; i++)
            {
                priorFilteredIndex[priorFilteredOrder[i]] = i;
            }

            // Check each matched instance for position changes using the filtered indices
            for (int i = 0; i < matchedInstancesInModOrder.Count; i++)
            {
                var instance = matchedInstancesInModOrder[i];
                int modPos = i;

                // If this instance wasn't present in priorFilteredOrder for any reason, treat as currentPos = -1
                int priorPos = priorFilteredIndex.TryGetValue(instance, out var p) ? p : -1;

                if (priorPos != modPos)
                {
                    LogCollector.Add(PropertyName, $"  Position change detected (filtered): {FormatItem(instance.Value)} (was at {priorPos}, mod wants at {modPos})");

                    // CRITICAL FIX B: Use explicit marker for instances created/re-added this pass
                    bool isNewOrReaddedThisPass = createdOrReaddedThisPass.Contains(instance);

                    if (isNewOrReaddedThisPass || modMasters.Contains(instance.OwnerMod))
                    {
                        instance.OwnerMod = modName; // ownership is already set for new/readded, harmless to reassign
                        LogCollector.Add(PropertyName, $"    → Moved and took ownership (owner: {instance.OwnerMod})");
                    }
                    else
                    {
                        // Mod lacks permission - lock this item to prevent placement
                        lockedItems.Add(instance);
                        LogCollector.Add(PropertyName, $"    → Cannot move (locked) - no permission for owner {instance.OwnerMod}");
                    }
                }
                else
                {
                    LogCollector.Add(PropertyName, $"  No position change: {FormatItem(instance.Value)} (stays at filtered index {priorPos})");
                }
            }

            // Step 5: Build final order with interleaved baseline placement (preserves relative order)
            var finalOrder = new List<ListPropertyValueContext<T>>();
            var processedItems = new HashSet<ListPropertyValueContext<T>>();

            LogCollector.Add(PropertyName, $"Building final order (interleaving baseline) - locked items: {lockedItems.Count}");

            // baseline = the prior active items as they were BEFORE this mod (we took priorActiveSnapshot earlier)
            // Keep only instances that are still active now (defensive)
            var baseline = priorActiveSnapshot.Where(inst => !inst.IsRemoved && currentActiveItems.Contains(inst)).ToList();

            // Build a baseline index map: instance -> index in baseline
            var baselineIndex = new Dictionary<ListPropertyValueContext<T>, int>();
            for (int i = 0; i < baseline.Count; i++)
            {
                baselineIndex[baseline[i]] = i;
            }

            int baselineCursor = 0;

            // For declared instances that existed in baseline we can get a baseline index; for new/readded ones use -1
            int GetBaselineIndex(ListPropertyValueContext<T> inst)
            {
                return baselineIndex.TryGetValue(inst, out var idx) ? idx : -1;
            }

            // Walk declared/matched instances in declared order; before placing each declared instance,
            // append baseline items whose baseline index is < the declared instance's baseline index.
            foreach (var instance in matchedInstancesInModOrder)
            {
                int priorIdx = GetBaselineIndex(instance);

                // Append baseline items that appear before this declared instance in the prior baseline
                // If priorIdx == -1 (new instance), we append nothing here so the new item stays before remaining baseline items.
                while (baselineCursor < baseline.Count && (priorIdx >= 0 ? baselineCursor < priorIdx : false))
                {
                    var b = baseline[baselineCursor];
                    if (!processedItems.Contains(b))
                    {
                        finalOrder.Add(b);
                        processedItems.Add(b);
                        LogCollector.Add(PropertyName, $"  Placed baseline before declared: {FormatItem(b.Value)} (owner: {b.OwnerMod})");
                    }
                    baselineCursor++;
                }

                // Place the declared instance if allowed and not already placed
                if (!lockedItems.Contains(instance) && !processedItems.Contains(instance))
                {
                    finalOrder.Add(instance);
                    processedItems.Add(instance);
                    LogCollector.Add(PropertyName, $"  Placed declared instance: {FormatItem(instance.Value)} (owner: {instance.OwnerMod})");
                }
                else if (lockedItems.Contains(instance))
                {
                    LogCollector.Add(PropertyName, $"  Skipped locked declared instance (kept in baseline): {FormatItem(instance.Value)} (owner: {instance.OwnerMod})");
                }
            }

            // Handle blocked slots (removed instances that cannot be re-added) - skip them entirely
            for (int i = 0; i < declaredSlots.Count; i++)
            {
                var slot = declaredSlots[i];
                if (slot != null && slot.IsRemoved)
                {
                    LogCollector.Add(PropertyName, $"  Skipped blocked slot[{i}]: {FormatItem(slot.Value)} (cannot re-add, owned by {slot.OwnerMod})");
                }
            }

            // After placing declared items, append any remaining baseline items not yet placed
            while (baselineCursor < baseline.Count)
            {
                var b = baseline[baselineCursor];
                if (!processedItems.Contains(b))
                {
                    finalOrder.Add(b);
                    processedItems.Add(b);
                    LogCollector.Add(PropertyName, $"  Appending leftover baseline: {FormatItem(b.Value)} (owner: {b.OwnerMod})");
                }
                baselineCursor++;
            }

            // Defensive: append any active items not yet placed
            var remainingItems = currentActiveItems.Where(item => !processedItems.Contains(item)).ToList();
            if (remainingItems.Count > 0)
            {
                LogCollector.Add(PropertyName, $"Appending other remaining items: {remainingItems.Count}");
                foreach (var item in remainingItems)
                {
                    finalOrder.Add(item);
                    processedItems.Add(item);
                    LogCollector.Add(PropertyName, $"  Appending remaining item: {FormatItem(item.Value)} (owner: {item.OwnerMod})");
                }
            }

            // Replace active items with final order
            var allRemovedItems = forwardValueContexts.Where(item => item.IsRemoved).ToList();
            forwardValueContexts.Clear();
            forwardValueContexts.AddRange(finalOrder);
            forwardValueContexts.AddRange(allRemovedItems);

            // Reindex
            ReindexOrderIndices(forwardValueContexts);

            LogCollector.Add(PropertyName, $"Final result: {string.Join(", ", finalOrder.Select((item, i) => $"[{i}]{FormatItem(item.Value)}"))}");
            LogCollector.Add(PropertyName, "=== END SORTING ALGORITHM DEBUG ===");
        }

        private void ReindexOrderIndices(List<ListPropertyValueContext<T>> forwardValueContexts)
        {
            var activeItems = forwardValueContexts.Where(item => !item.IsRemoved).ToList();
            for (int i = 0; i < activeItems.Count; i++)
            {
                activeItems[i].OrderIndex = i;
            }
        }

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

        private Dictionary<object, int> CountValuesList(List<T> values, ItemEqualityComparer<T> comparer)
        {
            var counts = new Dictionary<object, int>();
            foreach (var v in values)
            {
                if (v == null) continue;

                // Find existing key using custom equality
                var existingKey = counts.Keys.FirstOrDefault(key => comparer.Equals((T)key, v));
                if (existingKey != null)
                {
                    counts[existingKey]++;
                }
                else
                {
                    counts[v!] = 1;
                }
            }
            return counts;
        }

        private bool CountsEqualDict(Dictionary<object, int> a, Dictionary<object, int> b, ItemEqualityComparer<T> comparer)
        {
            if (a.Count != b.Count) return false;
            foreach (var kvp in a)
            {
                // Find matching key in b using custom equality
                var matchingKey = b.Keys.FirstOrDefault(key =>
                    kvp.Key is T key1 && key is T key2 && comparer.Equals(key1, key2));

                if (matchingKey == null || b[matchingKey] != kvp.Value)
                {
                    return false;
                }
            }
            return true;
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
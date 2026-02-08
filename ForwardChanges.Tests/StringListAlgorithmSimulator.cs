using ForwardChanges.Contexts;

namespace ForwardChanges.Tests;

/// <summary>
/// Standalone simulation of the NEIGHBOR-BASED PARTIAL REORDERING ALGORITHM
/// using strings instead of IMajorRecord. Uses the same context types (IPropertyContext,
/// IPropertyValueContext via ListPropertyContext&lt;string&gt;, ListPropertyValueContext&lt;string&gt;)
/// and a copy of the algorithm logic for testing without Mutagen or real records.
/// </summary>
public static class StringListAlgorithmSimulator
{
    /// <summary>
    /// Runs the full update: removals, then additions, then sorting.
    /// </summary>
    /// <param name="recordItems">Current record list (mod's declared list)</param>
    /// <param name="forwardValueContexts">Forward list (mutated in place)</param>
    /// <param name="modKey">Current mod key (e.g. "MyMod.esp")</param>
    /// <param name="masters">Set of mod keys that the current mod has as masters (can modify their items)</param>
    public static void RunFullUpdate(
        List<string> recordItems,
        List<ListPropertyValueContext<string>> forwardValueContexts,
        string modKey,
        IReadOnlySet<string> masters)
    {
        ProcessRemovals(recordItems, forwardValueContexts, modKey, masters);
        ProcessAdditions(recordItems, forwardValueContexts, modKey, masters);
        ProcessSortingAlgorithm(recordItems, forwardValueContexts, modKey, masters);
    }

    /// <summary>
    /// Runs only the sorting (reordering) step. Use when you only want to test the neighbor-based reordering.
    /// </summary>
    public static void RunSortingOnly(
        List<string> recordItems,
        List<ListPropertyValueContext<string>> forwardValueContexts,
        string modKey,
        IReadOnlySet<string> masters)
    {
        ProcessSortingAlgorithm(recordItems, forwardValueContexts, modKey, masters);
    }

    /// <summary>
    /// Runs full update with a custom sorting algorithm. Used to test alternatives.
    /// </summary>
    public static void RunFullUpdateWithSort(
        List<string> recordItems,
        List<ListPropertyValueContext<string>> forwardValueContexts,
        string modKey,
        IReadOnlySet<string> masters,
        SortingAlgorithmAlternatives.SortAlgorithm sortAlgorithm)
    {
        ProcessRemovals(recordItems, forwardValueContexts, modKey, masters);
        ProcessAdditions(recordItems, forwardValueContexts, modKey, masters);
        sortAlgorithm(recordItems, forwardValueContexts, modKey, masters);
    }

    /// <summary>
    /// Runs only the sorting step with a custom algorithm. Used to test alternatives.
    /// </summary>
    public static void RunSortingOnlyWithSort(
        List<string> recordItems,
        List<ListPropertyValueContext<string>> forwardValueContexts,
        string modKey,
        IReadOnlySet<string> masters,
        SortingAlgorithmAlternatives.SortAlgorithm sortAlgorithm)
    {
        sortAlgorithm(recordItems, forwardValueContexts, modKey, masters);
    }

    private static bool IsItemEqual(string? a, string? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a == b;
    }

    private static bool HasPermissionsToModify(string recordModKey, IReadOnlySet<string> masters, string? ownerMod)
    {
        if (ownerMod == null) return false;
        return recordModKey == ownerMod || masters.Contains(ownerMod);
    }

    private static void ProcessRemovals(
        List<string> recordItems,
        List<ListPropertyValueContext<string>> forwardValueContexts,
        string modKey,
        IReadOnlySet<string> masters)
    {
        var activeForwardItems = forwardValueContexts.Where(i => !i.IsRemoved).ToList();
        var forwardItemGroups = activeForwardItems
            .GroupBy(item => item.Value)
            .Select(g => (Item: g.Key, Items: g.ToList()))
            .ToList();
        var recordItemGroups = recordItems
            .GroupBy(item => item)
            .Select(g => (Item: g.Key, Count: g.Count()))
            .ToList();

        foreach (var recordGroup in recordItemGroups)
        {
            var recordItem = recordGroup.Item;
            var recordCount = recordGroup.Count;
            var forwardGroup = forwardItemGroups.FirstOrDefault(g => IsItemEqual(g.Item, recordItem));
            if (forwardGroup.Item != null)
            {
                var forwardItems = forwardGroup.Items;
                var forwardCount = forwardItems.Count;
                while (forwardCount > recordCount)
                {
                    ListPropertyValueContext<string>? itemToRemove = null;
                    for (int i = forwardItems.Count - 1; i >= 0; i--)
                    {
                        if (!forwardItems[i].IsRemoved && HasPermissionsToModify(modKey, masters, forwardItems[i].OwnerMod))
                        {
                            itemToRemove = forwardItems[i];
                            break;
                        }
                    }
                    if (itemToRemove != null)
                    {
                        itemToRemove.IsRemoved = true;
                        itemToRemove.OwnerMod = modKey;
                        forwardCount--;
                    }
                    else
                        break;
                }
            }
        }

        var itemsNotInRecord = forwardValueContexts
            .Where(item => !item.IsRemoved && !recordItemGroups.Any(g => IsItemEqual(g.Item, item.Value)))
            .ToList();
        foreach (var item in itemsNotInRecord)
        {
            if (HasPermissionsToModify(modKey, masters, item.OwnerMod))
            {
                item.IsRemoved = true;
                item.OwnerMod = modKey;
            }
        }
    }

    private static void ProcessAdditions(
        List<string> recordItems,
        List<ListPropertyValueContext<string>> forwardValueContexts,
        string modKey,
        IReadOnlySet<string> masters)
    {
        var forwardItemGroups = new List<(string Item, List<ListPropertyValueContext<string>> Items)>();
        foreach (var contextItem in forwardValueContexts)
        {
            var existingGroup = forwardItemGroups.FirstOrDefault(g => IsItemEqual(g.Item, contextItem.Value));
            if (existingGroup.Item != null)
            {
                existingGroup.Items.Add(contextItem);
            }
            else
            {
                forwardItemGroups.Add((contextItem.Value!, new List<ListPropertyValueContext<string>> { contextItem }));
            }
        }

        var recordItemGroups = recordItems
            .GroupBy(item => item)
            .Select(g => (Item: g.Key, Count: g.Count()))
            .ToList();

        for (int recordIndex = 0; recordIndex < recordItems.Count; recordIndex++)
        {
            var recordItem = recordItems[recordIndex];
            var recordCount = recordItems.Count(item => IsItemEqual(item, recordItem));
            var forwardGroup = forwardItemGroups.FirstOrDefault(g => IsItemEqual(g.Item, recordItem));

            if (forwardGroup.Item != null)
            {
                var forwardItems = forwardGroup.Items;
                var activeForwardCount = forwardItems.Count(item => !item.IsRemoved);

                while (activeForwardCount < recordCount)
                {
                    var itemToUnremove = forwardItems.FirstOrDefault(item =>
                        item.IsRemoved && HasPermissionsToModify(modKey, masters, item.OwnerMod));
                    if (itemToUnremove != null)
                    {
                        itemToUnremove.IsRemoved = false;
                        itemToUnremove.OwnerMod = modKey;
                        itemToUnremove.OrderOwnerMod = modKey;
                        activeForwardCount++;
                    }
                    else
                    {
                        var removedWithoutPermission = forwardItems.FirstOrDefault(item => item.IsRemoved);
                        if (removedWithoutPermission != null)
                            break;
                        var newItem = new ListPropertyValueContext<string>(recordItem, modKey);
                        newItem.OrderOwnerMod = null;
                        forwardValueContexts.Add(newItem);
                        forwardItems.Add(newItem);
                        activeForwardCount++;
                    }
                }
            }
            else
            {
                var newGroupItems = new List<ListPropertyValueContext<string>>();
                for (int i = 0; i < recordCount; i++)
                {
                    var newItem = new ListPropertyValueContext<string>(recordItem, modKey);
                    newItem.OrderOwnerMod = null;
                    forwardValueContexts.Add(newItem);
                    newGroupItems.Add(newItem);
                }
                forwardItemGroups.Add((recordItem, newGroupItems));
            }
        }
    }

    private static void ProcessSortingAlgorithm(
        List<string> recordItems,
        List<ListPropertyValueContext<string>> forwardValueContexts,
        string modKey,
        IReadOnlySet<string> masters)
    {
        var currentActiveItems = forwardValueContexts.Where(item => !item.IsRemoved).ToList();
        var finalOrder = new List<ListPropertyValueContext<string>>();
        var remainingInstances = new List<ListPropertyValueContext<string>>(currentActiveItems);

        foreach (var declaredValue in recordItems)
        {
            var match = remainingInstances.FirstOrDefault(inst => IsItemEqual(inst.Value, declaredValue));
            if (match != null && HasPermissionsToModify(modKey, masters, match.OrderOwnerMod))
            {
                finalOrder.Add(match);
                remainingInstances.Remove(match);
            }
        }

        var existingRemainingItems = remainingInstances.Where(item => item.OrderOwnerMod != null).ToList();
        var newRemainingItems = remainingInstances.Where(item => item.OrderOwnerMod == null).ToList();

        foreach (var remainingItem in existingRemainingItems)
        {
            int position = FindPositionBasedOnBeforeRelationships(remainingItem, currentActiveItems, finalOrder);
            finalOrder.Insert(position, remainingItem);
        }

        foreach (var finalItem in finalOrder)
        {
            var originalBefore = GetItemBefore(finalItem, currentActiveItems);
            var originalAfter = GetItemAfter(finalItem, currentActiveItems);
            var finalBefore = GetItemBefore(finalItem, finalOrder);
            var finalAfter = GetItemAfter(finalItem, finalOrder);
            bool beforeChanged = !AreContextItemsEqual(originalBefore, finalBefore);
            bool afterChanged = !AreContextItemsEqual(originalAfter, finalAfter);
            if (beforeChanged && afterChanged)
                finalItem.OrderOwnerMod = modKey;
        }

        var sortedNewItems = new List<(ListPropertyValueContext<string> Item, int RecordIndex)>();
        var remainingToMatch = new List<ListPropertyValueContext<string>>(newRemainingItems);
        for (int recordIndex = 0; recordIndex < recordItems.Count; recordIndex++)
        {
            var declaredValue = recordItems[recordIndex];
            var matchIndex = remainingToMatch.FindIndex(item => IsItemEqual(item.Value, declaredValue));
            if (matchIndex >= 0)
            {
                sortedNewItems.Add((remainingToMatch[matchIndex], recordIndex));
                remainingToMatch.RemoveAt(matchIndex);
            }
        }

        foreach (var (newItem, declaredIndex) in sortedNewItems)
        {
            int position = FindPositionForNewItem(newItem, recordItems, finalOrder, declaredIndex);
            finalOrder.Insert(position, newItem);
            newItem.OrderOwnerMod = modKey;
        }

        var removedItems = forwardValueContexts.Where(x => x.IsRemoved).ToList();
        forwardValueContexts.Clear();
        forwardValueContexts.AddRange(finalOrder);
        forwardValueContexts.AddRange(removedItems);
    }

    private static int FindPositionBasedOnBeforeRelationships(
        ListPropertyValueContext<string> itemToPlace,
        List<ListPropertyValueContext<string>> originalOrder,
        List<ListPropertyValueContext<string>> currentFinalOrder)
    {
        var originalBeforeItems = new List<ListPropertyValueContext<string>>();
        bool foundItemToPlace = false;
        foreach (var originalItem in originalOrder)
        {
            // Use reference equality for duplicate handling: find the exact instance we're placing
            if (ReferenceEquals(originalItem, itemToPlace))
            {
                foundItemToPlace = true;
                break;
            }
            originalBeforeItems.Add(originalItem);
        }
        if (!foundItemToPlace)
            return currentFinalOrder.Count;
        int position = 0;
        foreach (var beforeItem in originalBeforeItems)
        {
            // Use reference equality for duplicates: find the exact instance already placed
            int beforeIndex = currentFinalOrder.FindIndex(item => ReferenceEquals(item, beforeItem));
            if (beforeIndex != -1)
                position = Math.Max(position, beforeIndex + 1);
        }
        return Math.Min(position, currentFinalOrder.Count);
    }

    private static int FindPositionForNewItem(
        ListPropertyValueContext<string> newItem,
        List<string> recordItems,
        List<ListPropertyValueContext<string>> currentFinalOrder,
        int? declaredIndex = null)
    {
        int index = declaredIndex ?? recordItems.FindIndex(item => IsItemEqual(item, newItem.Value));
        if (index < 0)
            return currentFinalOrder.Count;
        // Use FindLastIndex so duplicate values place after the last occurrence, not the first
        int position = 0;
        for (int i = 0; i < index; i++)
        {
            int beforeIndex = currentFinalOrder.FindLastIndex(item => IsItemEqual(item.Value, recordItems[i]));
            if (beforeIndex != -1)
                position = Math.Max(position, beforeIndex + 1);
        }
        return Math.Min(position, currentFinalOrder.Count);
    }

    private static ListPropertyValueContext<string>? GetItemBefore(
        ListPropertyValueContext<string> item,
        List<ListPropertyValueContext<string>> list)
    {
        int index = list.FindIndex(i => ReferenceEquals(i, item));
        if (index <= 0) return null;
        return list[index - 1];
    }

    private static ListPropertyValueContext<string>? GetItemAfter(
        ListPropertyValueContext<string> item,
        List<ListPropertyValueContext<string>> list)
    {
        int index = list.FindIndex(i => ReferenceEquals(i, item));
        if (index < 0 || index >= list.Count - 1) return null;
        return list[index + 1];
    }

    private static bool AreContextItemsEqual(ListPropertyValueContext<string>? a, ListPropertyValueContext<string>? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return IsItemEqual(a.Value, b.Value);
    }
}

using ForwardChanges.Contexts;
using ForwardChanges.Contexts.Interfaces;
using ForwardChanges.PropertyHandlers.Formatting;
using ForwardChanges.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;

namespace ForwardChanges.PropertyHandlers.Abstracts;

public enum ListSemantics
{
    Unordered,
    SortedKeyed,
    AlignedOrdered,
    /// <summary>
    /// Entries are identified by their zero-based position and compared as atomic
    /// values. No sort key, value identity, or sequence alignment is applied.
    /// </summary>
    ExactOrdered
}

public abstract class AbstractListPropertyHandler<T> : IPropertyHandler<List<T>> where T : class
{
    public abstract string PropertyName { get; }
    public bool RequiresFullLoadOrderProcessing => true;
    public virtual ListSemantics Semantics => ListSemantics.Unordered;
    protected virtual bool CanBeNull => false;

    public abstract void SetValue(IMajorRecord record, List<T>? value);
    public abstract List<T>? GetValue(IMajorRecordGetter record);

    public virtual bool AreValuesEqual(List<T>? value1, List<T>? value2)
    {
        if (value1 == null && value2 == null) return true;
        if (value1 == null || value2 == null || value1.Count != value2.Count) return false;

        if (Semantics is ListSemantics.AlignedOrdered or ListSemantics.ExactOrdered)
        {
            return value1.Zip(value2, IsItemContentEqual).All(equal => equal);
        }

        var unmatched = value2.ToList();
        foreach (var item1 in value1)
        {
            var matchIndex = unmatched.FindIndex(item2 => IsItemContentEqual(item1, item2));
            if (matchIndex < 0) return false;
            unmatched.RemoveAt(matchIndex);
        }

        return true;
    }

    public virtual void UpdatePropertyContext(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
        IPropertyContext propertyContext)
    {
        if (propertyContext is not ListPropertyContext<T> listPropertyContext)
        {
            throw new InvalidOperationException(
                $"Property context is not a list property context for {PropertyName}");
        }

        var recordValue = GetValue(context.Record);
        var recordItems = recordValue ?? [];
        var forwardValueContexts = listPropertyContext.ForwardValueContexts
            ?? throw new InvalidOperationException(
                $"Property context is not initialized for {PropertyName}");
        var recordMod = state.LoadOrder[context.ModKey].Mod;
        if (recordMod == null)
        {
            Console.WriteLine($"Error: Record mod is null for {PropertyName}");
            return;
        }

        ProcessPresence(context, recordMod, recordValue == null, listPropertyContext);

        if (Semantics == ListSemantics.AlignedOrdered)
        {
            var declaredAlignmentRows = AlignCurrentList(listPropertyContext, recordItems);
            ProcessAlignedItems(
                context.ModKey.ToString(),
                recordMod,
                recordItems,
                declaredAlignmentRows,
                listPropertyContext,
                forwardValueContexts);
        }
        else if (Semantics == ListSemantics.ExactOrdered)
        {
            ProcessExactOrderedItems(
                context.ModKey.ToString(),
                recordMod,
                recordItems,
                listPropertyContext,
                forwardValueContexts);
        }
        else
        {
            ProcessRemovals(context, recordMod, recordItems, forwardValueContexts);
            ProcessAdditions(context, recordMod, recordItems, forwardValueContexts);
        }

        ProcessHandlerSpecificLogic(
            context,
            state,
            listPropertyContext,
            recordItems,
            forwardValueContexts);

        if (Semantics is not (ListSemantics.AlignedOrdered or ListSemantics.ExactOrdered))
        {
            ProcessSameIdentityReplacements(
                context,
                recordMod,
                recordItems,
                listPropertyContext,
                forwardValueContexts);
        }

        if (Semantics == ListSemantics.SortedKeyed)
        {
            SortActiveItems(state, forwardValueContexts);
        }
        listPropertyContext.ForwardValueContexts = forwardValueContexts;
    }

    /// <summary>
    /// Reconciles a list whose entries have only positional identity. Each value at
    /// an index is atomic: a content change replaces and takes ownership of that
    /// complete entry. This matches xEdit arrays whose entries expose no display
    /// sort key, so xEdit presents their columns by ordinal position.
    /// </summary>
    protected void ProcessExactOrderedItems(
        string modName,
        ISkyrimModGetter recordMod,
        IReadOnlyList<T> recordItems,
        ListPropertyContext<T> listPropertyContext,
        List<ListPropertyValueContext<T>> forwardValueContexts)
    {
        for (var position = 0; position < recordItems.Count; position++)
        {
            var declaredItem = recordItems[position];
            var activeItem = forwardValueContexts.FirstOrDefault(item =>
                !item.IsRemoved && item.AlignmentRowId == position);
            var originalItem = listPropertyContext.OriginalValueContexts?.FirstOrDefault(item =>
                item.AlignmentRowId == position);

            if (activeItem != null)
            {
                if (IsItemContentEqual(activeItem.Value, declaredItem)) continue;

                var returnsToOriginal = originalItem != null
                    && IsItemContentEqual(originalItem.Value, declaredItem);
                if (returnsToOriginal && !HasPermissionsToModify(recordMod, activeItem.OwnerMod))
                {
                    LogCollector.Add(
                        PropertyName,
                        $"[{PropertyName}] {modName}: Cannot revert positional item at index {position} to {FormatItem(declaredItem)} - no permission. Current owner: {activeItem.OwnerMod}");
                    continue;
                }

                var oldOwner = activeItem.OwnerMod;
                activeItem.Value = declaredItem;
                activeItem.OwnerMod = modName;
                activeItem.AlignmentOwnerMod = modName;
                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {modName}: Replacing positional item at index {position} with {FormatItem(declaredItem)} (was owned by {oldOwner}, new owner: {activeItem.OwnerMod}) Success");
                continue;
            }

            var removedItemsAtPosition = forwardValueContexts
                .Where(item => item.IsRemoved && item.AlignmentRowId == position)
                .ToList();
            var returnsToRemovedOriginal = originalItem != null
                && IsItemContentEqual(originalItem.Value, declaredItem);
            var itemToRestore = removedItemsAtPosition.FirstOrDefault(item =>
                HasPermissionsToModify(recordMod, item.OwnerMod));

            if (returnsToRemovedOriginal
                && itemToRestore == null
                && removedItemsAtPosition.Count > 0)
            {
                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {modName}: Cannot restore positional item at index {position} {FormatItem(declaredItem)} - no permission. Current owner: {removedItemsAtPosition[0].OwnerMod}");
                continue;
            }

            if (itemToRestore != null)
            {
                var oldOwner = itemToRestore.OwnerMod;
                itemToRestore.Value = declaredItem;
                itemToRestore.IsRemoved = false;
                itemToRestore.OwnerMod = modName;
                itemToRestore.AlignmentOwnerMod = modName;
                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {modName}: Restoring positional item at index {position} as {FormatItem(declaredItem)} (was owned by {oldOwner}, new owner: {itemToRestore.OwnerMod}) Success");
                continue;
            }

            var newItem = new ListPropertyValueContext<T>(declaredItem, modName)
            {
                AlignmentRowId = position,
                AlignmentOwnerMod = modName
            };
            forwardValueContexts.Add(newItem);
            LogCollector.Add(
                PropertyName,
                $"[{PropertyName}] {modName}: Adding positional item at index {position} {FormatItem(declaredItem)} (new owner: {modName}) Success");
        }

        foreach (var item in forwardValueContexts
                     .Where(item => !item.IsRemoved && item.AlignmentRowId >= recordItems.Count)
                     .ToList())
        {
            var position = item.AlignmentRowId!.Value;
            if (!HasPermissionsToModify(recordMod, item.OwnerMod))
            {
                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {modName}: Cannot remove positional item at index {position} {FormatItem(item.Value)} - no permission. Current owner: {item.OwnerMod}");
                continue;
            }

            var oldOwner = item.OwnerMod;
            item.IsRemoved = true;
            item.OwnerMod = modName;
            item.AlignmentOwnerMod = modName;
            LogCollector.Add(
                PropertyName,
                $"[{PropertyName}] {modName}: Removing positional item at index {position} {FormatItem(item.Value)} (was owned by {oldOwner}, new owner: {item.OwnerMod}) Success");
        }

        OrderExactItemsByPosition(forwardValueContexts);
    }

    private static void OrderExactItemsByPosition(
        List<ListPropertyValueContext<T>> forwardValueContexts)
    {
        var priorPositions = new Dictionary<ListPropertyValueContext<T>, int>(
            ReferenceEqualityComparer.Instance);
        for (var index = 0; index < forwardValueContexts.Count; index++)
        {
            priorPositions[forwardValueContexts[index]] = index;
        }
        var activeItems = forwardValueContexts
            .Where(item => !item.IsRemoved)
            .OrderBy(item => item.AlignmentRowId ?? int.MaxValue)
            .ThenBy(item => priorPositions[item])
            .ToList();
        var removedItems = forwardValueContexts.Where(item => item.IsRemoved).ToList();

        forwardValueContexts.Clear();
        forwardValueContexts.AddRange(activeItems);
        forwardValueContexts.AddRange(removedItems);
    }

    private void ProcessPresence(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
        ISkyrimModGetter recordMod,
        bool recordIsNull,
        ListPropertyContext<T> listPropertyContext)
    {
        if (recordIsNull)
        {
            listPropertyContext.CanBeNull = true;
        }

        if (!listPropertyContext.CanBeNull)
        {
            listPropertyContext.ForwardIsNull = false;
            return;
        }

        var originalIsNull = listPropertyContext.OriginalIsNull;
        var forwardIsNull = listPropertyContext.ForwardIsNull;
        if (recordIsNull != originalIsNull && recordIsNull != forwardIsNull)
        {
            listPropertyContext.ForwardIsNull = recordIsNull;
            listPropertyContext.ForwardPresenceOwnerMod = context.ModKey.ToString();
            return;
        }

        if (recordIsNull == originalIsNull
            && recordIsNull != forwardIsNull
            && HasPermissionsToModify(recordMod, listPropertyContext.ForwardPresenceOwnerMod))
        {
            listPropertyContext.ForwardIsNull = recordIsNull;
            listPropertyContext.ForwardPresenceOwnerMod = context.ModKey.ToString();
        }
    }

    private void ProcessRemovals(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
        ISkyrimModGetter recordMod,
        List<T> recordItems,
        List<ListPropertyValueContext<T>> forwardValueContexts)
    {
        var recordGroups = GroupItems(recordItems);
        var activeGroups = GroupValueContexts(forwardValueContexts.Where(item => !item.IsRemoved));

        foreach (var recordGroup in recordGroups)
        {
            var forwardGroup = activeGroups.FirstOrDefault(group =>
                IsItemIdentityEqual(group.Item, recordGroup.Item));
            if (forwardGroup.Item == null) continue;

            var activeCount = forwardGroup.Items.Count;
            while (activeCount > recordGroup.Count)
            {
                var itemToRemove = forwardGroup.Items.LastOrDefault(item =>
                    !item.IsRemoved && HasPermissionsToModify(recordMod, item.OwnerMod));
                if (itemToRemove == null)
                {
                    LogCollector.Add(
                        PropertyName,
                        $"[{PropertyName}] {context.ModKey}: Cannot remove excess item {FormatItem(recordGroup.Item)} - no permission");
                    break;
                }

                var oldOwner = itemToRemove.OwnerMod;
                itemToRemove.IsRemoved = true;
                itemToRemove.OwnerMod = context.ModKey.ToString();
                activeCount--;
                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {context.ModKey}: Removing excess item {FormatItem(recordGroup.Item)} (was owned by {oldOwner}, new owner: {itemToRemove.OwnerMod}) Success");
            }
        }

        var itemsNotInRecord = forwardValueContexts
            .Where(item =>
                !item.IsRemoved
                && !recordGroups.Any(group => IsItemIdentityEqual(group.Item, item.Value)))
            .ToList();
        foreach (var item in itemsNotInRecord)
        {
            if (!HasPermissionsToModify(recordMod, item.OwnerMod))
            {
                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {context.ModKey}: Cannot remove item not in record {FormatItem(item.Value)} - no permission. Current owner: {item.OwnerMod}");
                continue;
            }

            var oldOwner = item.OwnerMod;
            item.IsRemoved = true;
            item.OwnerMod = context.ModKey.ToString();
            LogCollector.Add(
                PropertyName,
                $"[{PropertyName}] {context.ModKey}: Removing item not in record {FormatItem(item.Value)} (was owned by {oldOwner}, new owner: {item.OwnerMod}) Success");
        }
    }

    private void ProcessAdditions(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
        ISkyrimModGetter recordMod,
        List<T> recordItems,
        List<ListPropertyValueContext<T>> forwardValueContexts)
    {
        foreach (var recordGroup in GroupItems(recordItems))
        {
            var matchingContexts = forwardValueContexts
                .Where(item => IsItemIdentityEqual(item.Value, recordGroup.Item))
                .ToList();
            var activeCount = matchingContexts.Count(item => !item.IsRemoved);

            while (activeCount < recordGroup.Count)
            {
                var itemToRestore = matchingContexts.FirstOrDefault(item =>
                    item.IsRemoved && HasPermissionsToModify(recordMod, item.OwnerMod));
                if (itemToRestore != null)
                {
                    var oldOwner = itemToRestore.OwnerMod;
                    itemToRestore.IsRemoved = false;
                    itemToRestore.OwnerMod = context.ModKey.ToString();
                    itemToRestore.AlignmentOwnerMod = context.ModKey.ToString();
                    activeCount++;
                    LogCollector.Add(
                        PropertyName,
                        $"[{PropertyName}] {context.ModKey}: Adding back previously removed item {FormatItem(recordGroup.Item)} (was owned by {oldOwner}, new owner: {itemToRestore.OwnerMod}) Success");
                    continue;
                }

                var inaccessibleRemovedItem = matchingContexts.FirstOrDefault(item => item.IsRemoved);
                if (inaccessibleRemovedItem != null)
                {
                    LogCollector.Add(
                        PropertyName,
                        $"[{PropertyName}] {context.ModKey}: Cannot add back previously removed item {FormatItem(recordGroup.Item)} - no permission (owned by {inaccessibleRemovedItem.OwnerMod})");
                    break;
                }

                var newItem = new ListPropertyValueContext<T>(
                    recordGroup.Item,
                    context.ModKey.ToString())
                {
                    AlignmentOwnerMod = null
                };
                forwardValueContexts.Add(newItem);
                matchingContexts.Add(newItem);
                activeCount++;
                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {context.ModKey}: Adding new item {FormatItem(recordGroup.Item)} (new owner: {newItem.OwnerMod}) Success");
            }
        }
    }

    /// <summary>
    /// Progressively aligns the accumulated row sequence with one mod's declared
    /// sequence. This is the active ordering implementation for every
    /// <see cref="ListSemantics.AlignedOrdered"/> handler.
    /// </summary>
    protected int[] AlignCurrentList(
        ListPropertyContext<T> listPropertyContext,
        IReadOnlyList<T> recordItems)
    {
        var result = XEditSequenceAligner.Align(
            listPropertyContext.AlignmentRows,
            recordItems,
            (left, right) => IsAlignmentEqual(left, right),
            () => listPropertyContext.NextAlignmentRowId++);
        listPropertyContext.AlignmentRows = result.Rows;
        return result.RightRowIds;
    }

    /// <summary>
    /// Reconciles one aligned plugin column by occurrence-specific xEdit row ID.
    /// Row identity locates an existing entry; content equality determines whether
    /// that row's value changed. This distinction is required for conditions, whose
    /// operator and comparison value are data within an xEdit-aligned row.
    /// </summary>
    protected void ProcessAlignedItems(
        string modName,
        ISkyrimModGetter recordMod,
        IReadOnlyList<T> recordItems,
        IReadOnlyList<int> declaredRowIds,
        ListPropertyContext<T> listPropertyContext,
        List<ListPropertyValueContext<T>> forwardValueContexts)
    {
        if (recordItems.Count != declaredRowIds.Count)
        {
            throw new InvalidOperationException(
                $"Alignment row count does not match the declared {PropertyName} item count");
        }

        var activeItems = forwardValueContexts.Where(item => !item.IsRemoved).ToList();
        var consumedActiveItems = new HashSet<ListPropertyValueContext<T>>(
            ReferenceEqualityComparer.Instance);

        for (var declaredIndex = 0; declaredIndex < recordItems.Count; declaredIndex++)
        {
            var declaredItem = recordItems[declaredIndex];
            var declaredRowId = declaredRowIds[declaredIndex];

            // A row match takes precedence over value identity. An aligned row can
            // legitimately contain edited data that is not semantically equal to the
            // value from the preceding plugin column.
            var activeItem = activeItems.FirstOrDefault(item =>
                !consumedActiveItems.Contains(item)
                && item.AlignmentRowId == declaredRowId);
            if (activeItem != null)
            {
                consumedActiveItems.Add(activeItem);
                ReplaceAlignedRowValue(
                    modName,
                    recordMod,
                    declaredItem,
                    declaredRowId,
                    activeItem,
                    listPropertyContext);
                continue;
            }

            // xEdit represents a move as an old-row gap plus a new row. Preserve the
            // existing ownership rule by recognizing the same logical item before
            // deciding that the new row is an unrelated addition.
            activeItem = activeItems.FirstOrDefault(item =>
                !consumedActiveItems.Contains(item)
                && IsItemIdentityEqual(item.Value, declaredItem));
            if (activeItem != null)
            {
                consumedActiveItems.Add(activeItem);
                var originalRowId = activeItem.AlignmentRowId;
                if (activeItem.AlignmentRowId == null
                    || activeItem.AlignmentOwnerMod == null
                    || HasPermissionsToModify(recordMod, activeItem.AlignmentOwnerMod))
                {
                    activeItem.AlignmentRowId = declaredRowId;
                    activeItem.AlignmentOwnerMod = modName;
                }

                ReplaceAlignedRowValue(
                    modName,
                    recordMod,
                    declaredItem,
                    originalRowId,
                    activeItem,
                    listPropertyContext);
                continue;
            }

            var removedIdentityItems = forwardValueContexts
                .Where(item =>
                    item.IsRemoved
                    && IsItemIdentityEqual(item.Value, declaredItem))
                .ToList();
            var itemToRestore = removedIdentityItems.FirstOrDefault(item =>
                    item.AlignmentRowId == declaredRowId
                    && HasPermissionsToModify(recordMod, item.OwnerMod))
                ?? removedIdentityItems.FirstOrDefault(item =>
                    HasPermissionsToModify(recordMod, item.OwnerMod));
            if (itemToRestore != null)
            {
                var oldOwner = itemToRestore.OwnerMod;
                var originalRowId = itemToRestore.AlignmentRowId;
                itemToRestore.IsRemoved = false;
                itemToRestore.OwnerMod = modName;
                itemToRestore.AlignmentRowId = declaredRowId;
                itemToRestore.AlignmentOwnerMod = modName;
                activeItems.Add(itemToRestore);
                consumedActiveItems.Add(itemToRestore);
                ReplaceAlignedRowValue(
                    modName,
                    recordMod,
                    declaredItem,
                    originalRowId,
                    itemToRestore,
                    listPropertyContext);
                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {modName}: Adding back previously removed item {FormatItem(declaredItem)} (was owned by {oldOwner}, new owner: {itemToRestore.OwnerMod}) Success");
                continue;
            }

            var inaccessibleRemovedItem = removedIdentityItems.FirstOrDefault();
            if (inaccessibleRemovedItem != null)
            {
                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {modName}: Cannot add back previously removed item {FormatItem(declaredItem)} - no permission (owned by {inaccessibleRemovedItem.OwnerMod})");
                continue;
            }

            var newItem = new ListPropertyValueContext<T>(declaredItem, modName)
            {
                AlignmentRowId = declaredRowId,
                AlignmentOwnerMod = modName
            };
            forwardValueContexts.Add(newItem);
            activeItems.Add(newItem);
            consumedActiveItems.Add(newItem);
            LogCollector.Add(
                PropertyName,
                $"[{PropertyName}] {modName}: Adding new item {FormatItem(declaredItem)} (new owner: {modName}) Success");
        }

        foreach (var item in activeItems.Where(item => !consumedActiveItems.Contains(item)))
        {
            if (!HasPermissionsToModify(recordMod, item.OwnerMod))
            {
                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {modName}: Cannot remove item not in record {FormatItem(item.Value)} - no permission. Current owner: {item.OwnerMod}");
                continue;
            }

            var oldOwner = item.OwnerMod;
            item.IsRemoved = true;
            item.OwnerMod = modName;
            LogCollector.Add(
                PropertyName,
                $"[{PropertyName}] {modName}: Removing item not in record {FormatItem(item.Value)} (was owned by {oldOwner}, new owner: {item.OwnerMod}) Success");
        }

        OrderActiveItemsByAlignmentRows(listPropertyContext, forwardValueContexts);
    }

    private void ReplaceAlignedRowValue(
        string modName,
        ISkyrimModGetter recordMod,
        T declaredItem,
        int? originalRowId,
        ListPropertyValueContext<T> forwardItem,
        ListPropertyContext<T> listPropertyContext)
    {
        if (IsItemContentEqual(forwardItem.Value, declaredItem)) return;

        var originalItem = originalRowId == null
            ? null
            : listPropertyContext.OriginalValueContexts?.FirstOrDefault(item =>
                item.AlignmentRowId == originalRowId);
        var returnsToOriginal = originalItem != null
            && IsItemContentEqual(originalItem.Value, declaredItem);
        if (returnsToOriginal && !HasPermissionsToModify(recordMod, forwardItem.OwnerMod))
        {
            LogCollector.Add(
                PropertyName,
                $"[{PropertyName}] {modName}: Cannot revert aligned row {FormatItem(declaredItem)} - no permission. Current owner: {forwardItem.OwnerMod}");
            return;
        }

        var oldOwner = forwardItem.OwnerMod;
        forwardItem.Value = declaredItem;
        forwardItem.OwnerMod = modName;
        LogCollector.Add(
            PropertyName,
            $"[{PropertyName}] {modName}: Replacing aligned row value {FormatItem(declaredItem)} (was owned by {oldOwner}, new owner: {forwardItem.OwnerMod}) Success");
    }

    private static void OrderActiveItemsByAlignmentRows(
        ListPropertyContext<T> listPropertyContext,
        List<ListPropertyValueContext<T>> forwardValueContexts)
    {
        var activeItems = forwardValueContexts.Where(item => !item.IsRemoved).ToList();
        var rowPositions = listPropertyContext.AlignmentRows
            .Select((row, index) => (row.Id, index))
            .ToDictionary(entry => entry.Id, entry => entry.index);
        var priorPositions = new Dictionary<ListPropertyValueContext<T>, int>(
            ReferenceEqualityComparer.Instance);
        for (var index = 0; index < forwardValueContexts.Count; index++)
        {
            priorPositions[forwardValueContexts[index]] = index;
        }
        var orderedActiveItems = activeItems
            .OrderBy(item => item.AlignmentRowId is { } rowId
                && rowPositions.TryGetValue(rowId, out var rowPosition)
                    ? rowPosition
                    : int.MaxValue)
            .ThenBy(item => priorPositions[item])
            .ToList();
        var removedItems = forwardValueContexts.Where(item => item.IsRemoved).ToList();

        forwardValueContexts.Clear();
        forwardValueContexts.AddRange(orderedActiveItems);
        forwardValueContexts.AddRange(removedItems);
    }

    protected virtual void ProcessHandlerSpecificLogic(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
        ListPropertyContext<T> listPropertyContext,
        List<T> recordItems,
        List<ListPropertyValueContext<T>> currentForwardItems)
    {
    }

    private void ProcessSameIdentityReplacements(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
        ISkyrimModGetter recordMod,
        IReadOnlyList<T> recordItems,
        ListPropertyContext<T> listPropertyContext,
        List<ListPropertyValueContext<T>> forwardItems)
    {
        var originalItems = listPropertyContext.OriginalValueContexts?
            .Select(item => item.Value)
            .ToList() ?? [];
        var consumed = new HashSet<ListPropertyValueContext<T>>(ReferenceEqualityComparer.Instance);

        foreach (var recordItem in recordItems)
        {
            var forwardItem = forwardItems.FirstOrDefault(item =>
                !item.IsRemoved
                && !consumed.Contains(item)
                && IsItemIdentityEqual(item.Value, recordItem));
            if (forwardItem == null) continue;
            consumed.Add(forwardItem);

            if (IsItemContentEqual(forwardItem.Value, recordItem)) continue;

            var originalItem = originalItems.FirstOrDefault(item =>
                IsItemIdentityEqual(item, recordItem));
            var returnsToOriginal = originalItem != null
                && IsItemContentEqual(originalItem, recordItem);
            if (returnsToOriginal && !HasPermissionsToModify(recordMod, forwardItem.OwnerMod))
            {
                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {context.ModKey}: Cannot revert keyed item {FormatItem(recordItem)} - no permission. Current owner: {forwardItem.OwnerMod}");
                continue;
            }

            var oldOwner = forwardItem.OwnerMod;
            forwardItem.Value = recordItem;
            forwardItem.OwnerMod = context.ModKey.ToString();
            LogCollector.Add(
                PropertyName,
                $"[{PropertyName}] {context.ModKey}: Replacing keyed item {FormatItem(recordItem)} (was owned by {oldOwner}, new owner: {forwardItem.OwnerMod}) Success");
        }
    }

    private void SortActiveItems(
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
        List<ListPropertyValueContext<T>> forwardItems)
    {
        var loadOrder = state.RawLoadOrder
            .Select((listing, index) => (listing.ModKey, index))
            .GroupBy(entry => entry.ModKey)
            .ToDictionary(group => group.Key, group => group.First().index);
        var priorPositions = new Dictionary<ListPropertyValueContext<T>, int>(ReferenceEqualityComparer.Instance);
        for (var index = 0; index < forwardItems.Count; index++)
        {
            priorPositions[forwardItems[index]] = index;
        }
        var sortKeyComparer = new XEditSortKeyComparer(loadOrder);
        var activeItems = forwardItems
            .Where(item => !item.IsRemoved)
            .OrderBy(
                item => GetSortKey(item.Value),
                sortKeyComparer)
            .ThenBy(item => priorPositions[item])
            .ToList();
        var removedItems = forwardItems.Where(item => item.IsRemoved).ToList();

        forwardItems.Clear();
        forwardItems.AddRange(activeItems);
        forwardItems.AddRange(removedItems);
    }

    /// <summary>
    /// Full semantic equality used for additions, removals, ownership, and the
    /// final value comparison.
    /// </summary>
    protected virtual bool IsItemEqual(T? item1, T? item2)
    {
        if (item1 == null && item2 == null) return true;
        if (item1 == null || item2 == null) return false;
        return Equals(item1, item2);
    }

    /// <summary>
    /// Full value equality. Sorted keyed handlers override identity separately so
    /// edits to non-key data remain changes to one logical entry.
    /// </summary>
    protected virtual bool IsItemContentEqual(T? item1, T? item2)
        => IsItemEqual(item1, item2);

    /// <summary>
    /// Logical entry identity used for ownership, additions, removals, and
    /// same-key replacement. The default preserves the legacy full-value identity.
    /// </summary>
    protected virtual bool IsItemIdentityEqual(T? item1, T? item2)
        => IsItemEqual(item1, item2);

    /// <summary>
    /// Logical row identity used only for progressive alignment. By default it
    /// is semantic equality; handlers such as Conditions override it with the
    /// corresponding xEdit alignment sort key.
    /// </summary>
    protected virtual bool IsAlignmentEqual(T? item1, T? item2)
        => IsItemIdentityEqual(item1, item2);

    /// <summary>
    /// Creates an independent mutable value for the forward context when a handler
    /// updates item fields in place. Immutable/getter-only values can share the value
    /// object while their ownership and removal context remains independent.
    /// </summary>
    protected virtual T CopyItemForForwardContext(T item) => item;

    /// <summary>
    /// xEdit StructSK components in declaration order. Scalar sorted handlers
    /// return one component; composite handlers return every key field.
    /// </summary>
    protected virtual IReadOnlyList<object?> GetSortKey(T item)
        => throw new InvalidOperationException(
            $"Sorted list handler {GetType().Name} must define an xEdit sort key for {PropertyName}.");

    protected virtual string FormatItem(T? item)
        => DiagnosticValueFormatter.Format(item);

    protected bool HasPermissionsToModify(ISkyrimModGetter mod, string? ownerMod)
    {
        if (ownerMod == null) return false;
        return mod.MasterReferences.Any(master =>
                   string.Equals(
                       master.Master.ToString(),
                       ownerMod,
                       StringComparison.OrdinalIgnoreCase))
               || string.Equals(
                   mod.ModKey.ToString(),
                   ownerMod,
                   StringComparison.OrdinalIgnoreCase);
    }

    public virtual void InitializeContext(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPropertyContext propertyContext)
    {
        if (propertyContext is not ListPropertyContext<T> listPropertyContext)
        {
            throw new InvalidOperationException(
                $"Property context is not a list property context for {PropertyName}");
        }

        var originalList = GetValue(originalContext.Record);
        var ownerMod = originalContext.ModKey.ToString();
        var originalItems = (originalList ?? [])
            .Select(item => new ListPropertyValueContext<T>(item, ownerMod))
            .ToList();

        listPropertyContext.AlignmentRows = [];
        listPropertyContext.NextAlignmentRowId = 0;

        if (Semantics is ListSemantics.AlignedOrdered or ListSemantics.ExactOrdered)
        {
            foreach (var item in originalItems)
            {
                var rowId = listPropertyContext.NextAlignmentRowId++;
                item.AlignmentRowId = rowId;
                item.AlignmentOwnerMod = ownerMod;
                if (Semantics == ListSemantics.AlignedOrdered)
                {
                    listPropertyContext.AlignmentRows.Add(
                        new ListAlignmentRow<T>(rowId, item.Value));
                }
            }
        }

        // Original ownership/removal state must remain immutable for reversion
        // detection. Every semantic gets independent context objects; handlers that
        // mutate item fields in place override CopyItemForForwardContext as well.
        var forwardItems = originalItems
            .Select(item => new ListPropertyValueContext<T>(
                CopyItemForForwardContext(item.Value),
                item.OwnerMod)
            {
                AlignmentRowId = item.AlignmentRowId,
                AlignmentOwnerMod = item.AlignmentOwnerMod
            })
            .ToList();
        listPropertyContext.OriginalValueContexts = originalItems;
        listPropertyContext.ForwardValueContexts = forwardItems;

        listPropertyContext.CanBeNull = CanBeNull || originalList == null;
        listPropertyContext.OriginalIsNull = originalList == null;
        listPropertyContext.ForwardIsNull = listPropertyContext.OriginalIsNull;
        listPropertyContext.ForwardPresenceOwnerMod = ownerMod;
        listPropertyContext.IsResolved = false;

        if (!((IPropertyHandler)this).AreValuesEqual(
                listPropertyContext.GetForwardValue(),
                originalList))
        {
            throw new InvalidOperationException(
                $"List context initialization changed the representation of {PropertyName}");
        }
    }

    private List<(T Item, List<ListPropertyValueContext<T>> Items)> GroupValueContexts(
        IEnumerable<ListPropertyValueContext<T>> contexts)
    {
        var groups = new List<(T Item, List<ListPropertyValueContext<T>> Items)>();
        foreach (var context in contexts)
        {
            var groupIndex = groups.FindIndex(group => IsItemIdentityEqual(group.Item, context.Value));
            if (groupIndex < 0)
            {
                groups.Add((context.Value, [context]));
            }
            else
            {
                groups[groupIndex].Items.Add(context);
            }
        }

        return groups;
    }

    private List<(T Item, int Count)> GroupItems(IEnumerable<T> items)
    {
        var groups = new List<(T Item, int Count)>();
        foreach (var item in items)
        {
            var groupIndex = groups.FindIndex(group => IsItemIdentityEqual(group.Item, item));
            if (groupIndex < 0)
            {
                groups.Add((item, 1));
            }
            else
            {
                var group = groups[groupIndex];
                groups[groupIndex] = (group.Item, group.Count + 1);
            }
        }

        return groups;
    }

    void IPropertyHandler.SetValue(IMajorRecord record, object? value)
    {
        if (value is List<object> objectList)
        {
            SetValue(record, objectList.Select(item => (T)item).ToList());
            return;
        }

        SetValue(record, (List<T>?)value);
    }

    object? IPropertyHandler.GetValue(IMajorRecordGetter record)
        => GetValue(record);

    bool IPropertyHandler.AreValuesEqual(object? value1, object? value2)
    {
        try
        {
            var typedValue1 = value1 is List<object> objectList1
                ? objectList1.Select(item => (T)item).ToList()
                : (List<T>?)value1;
            var typedValue2 = value2 is List<object> objectList2
                ? objectList2.Select(item => (T)item).ToList()
                : (List<T>?)value2;
            return AreValuesEqual(typedValue1, typedValue2);
        }
        catch (InvalidCastException)
        {
            return false;
        }
    }

    IPropertyContext IPropertyHandler.CreatePropertyContext()
        => new ListPropertyContext<T> { CanBeNull = CanBeNull };

    public virtual string FormatValue(object? value)
    {
        if (value is List<T> list)
        {
            return list.Count == 0
                ? "Empty"
                : string.Join(", ", list.Select(FormatItem));
        }

        if (value is List<object> objectList)
        {
            return objectList.Count == 0
                ? "Empty"
                : string.Join(", ", objectList.Cast<T>().Select(FormatItem));
        }

        return DiagnosticValueFormatter.Format(value);
    }
}

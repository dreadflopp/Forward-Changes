using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.PropertyStates;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ListHandlers.Abstracts
{
    public abstract class AbstractListPropertyHandler<TRecord, TItem> : IPropertyHandler
        where TRecord : class, IMajorRecordGetter
        where TItem : class
    {
        protected readonly string _propertyName;
        protected readonly Func<TRecord, IReadOnlyList<TItem>> ListAccessor;

        protected AbstractListPropertyHandler(
            string propertyName,
            Func<TRecord, IReadOnlyList<TItem>> listAccessor)
        {
            _propertyName = propertyName;
            ListAccessor = listAccessor;
        }

        public abstract string PropertyName { get; }

        public virtual object? GetValue(IMajorRecordGetter record)
        {
            if (record is not TRecord typedRecord)
                return null;

            var listState = new ItemStateCollection<TItem>();
            listState.Items = ListAccessor(typedRecord).Select(item => new ItemState<TItem>(item, "")).ToList();
            return listState;
        }

        public abstract void SetValue(IMajorRecord record, object? value);

        public virtual object? GetValueFromContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is not TRecord typedRecord)
                return null;

            var listState = new ItemStateCollection<TItem>();
            listState.Items = ListAccessor(typedRecord).Select(item => new ItemState<TItem>(item, context.ModKey.ToString())).ToList();
            return listState;
        }

        public abstract bool AreValuesEqual(object? value1, object? value2);

        protected abstract bool IsItemEqual(TItem item1, TItem item2);

        public virtual PropertyState CreateState(string lastChangedByMod, object? originalValue = null)
        {
            var listState = new ItemStateCollection<TItem>();
            try
            {
                if (originalValue == null)
                {
                    Console.WriteLine($"Warning: Null {_propertyName} list value encountered");
                    return new PropertyState
                    {
                        OriginalValue = originalValue,
                        FinalValue = listState,
                        LastChangedByMod = lastChangedByMod
                    };
                }

                if (originalValue is ItemStateCollection<TItem> originalListState)
                {
                    listState.Items = originalListState.Items.Select(item => new ItemState<TItem>(item.Item, lastChangedByMod)).ToList();
                }
                else
                {
                    Console.WriteLine($"Error: Expected ItemStateCollection<{typeof(TItem).Name}> but got {originalValue.GetType().Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {_propertyName} list: {ex.Message}");
            }

            var state = new PropertyState
            {
                OriginalValue = originalValue,
                FinalValue = listState,
                LastChangedByMod = lastChangedByMod
            };

            // Debug output
            if (originalValue is ItemStateCollection<TItem> debugListState)
            {
                LogCollector.Add(_propertyName, $"[{_propertyName}] CreateState. LastChangedByMod: {lastChangedByMod} Original items: {string.Join(", ", debugListState.Items.Select(i => FormatItem(i.Item)))}");
            }
            else
            {
                LogCollector.Add(_propertyName, $"[{_propertyName}] No original items");
            }

            return state;
        }

        public abstract void UpdatePropertyState(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            PropertyState propertyState);

        protected void InsertItemAtCorrectPosition(
            ItemState<TItem> newItem,
            IReadOnlyList<TItem> recordItems,
            ItemStateCollection<TItem> currentFinalItems)
        {
            // Find the position in the record items
            var recordIndex = recordItems.IndexOf(newItem.Item);
            if (recordIndex == -1)
            {
                // If not found, add at the end
                currentFinalItems.Items.Add(newItem);
                return;
            }

            // Find the position in the final items
            var finalIndex = currentFinalItems.Items.FindIndex(item =>
                !item.IsRemoved && recordItems.IndexOf(item.Item) > recordIndex);

            if (finalIndex == -1)
            {
                // If no items after this one, add at the end
                currentFinalItems.Items.Add(newItem);
            }
            else
            {
                // Insert at the correct position
                currentFinalItems.Items.Insert(finalIndex, newItem);
            }
        }

        /// <summary>
        /// Tells the record handler that this is a list handler.
        /// </summary>
        public bool IsListHandler => true;

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
        /// Get the item state collection for the given context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>The item state collection</returns>
        public virtual object? GetItemStateCollection(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is not TRecord record)
                return null;

            var itemStateCollection = new ItemStateCollection<TItem>();
            itemStateCollection.Items = ListAccessor(record).Select(item => new ItemState<TItem>(item, context.ModKey.ToString())).ToList();
            return itemStateCollection;
        }

        /// <summary>
        /// Sort the items to match the context order.
        /// </summary>
        /// <param name="contextItems"></param>
        /// <param name="itemStates"></param>
        /// <param name="state"></param>
        /// <param name="context"></param>
        public virtual void SortItemsToMatchContextOrder(
            IEnumerable<TItem> contextItems,
            ItemStateCollection<TItem> itemStates,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            var contextList = contextItems.ToList();
            // Only include items that the context has permission to reorder
            var activeItems = itemStates.Items.Where(i => !i.IsRemoved &&
                contextList.Any(c => IsItemEqual(c, i.Item)) &&
                (i.OwnerMod == context.ModKey.ToString() || HasModInMasters(context.ModKey.ToString(), i.OwnerMod, state))).ToList();

            if (activeItems.Count > 1)
            {
                LogCollector.Add(_propertyName, $"[{_propertyName}] {context.ModKey}: Reordering {activeItems.Count} items");
            }

            // Update ordering information for each item
            for (int i = 0; i < contextList.Count; i++)
            {
                var contextItem = contextList[i];
                var existingItem = activeItems.FirstOrDefault(w => IsItemEqual(w.Item, contextItem));

                if (existingItem != null)
                {
                    existingItem.OriginalIndex = i;

                    // Add items before this one
                    if (i > 0)
                    {
                        var beforeItem = contextList[i - 1]?.ToString();
                        if (beforeItem != null)
                            existingItem.ItemsBefore.Add(beforeItem);
                    }

                    // Add items after this one
                    if (i < contextList.Count - 1)
                    {
                        var afterItem = contextList[i + 1]?.ToString();
                        if (afterItem != null)
                            existingItem.ItemsAfter.Add(afterItem);
                    }
                }
            }

            // Reorder items based on their relationships
            var orderedItems = new List<ItemState<TItem>>();
            var remainingItems = new List<ItemState<TItem>>(activeItems);

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

            // Update the final list, keeping non-reordered items in their original positions
            var nonReorderedItems = itemStates.Items.Except(activeItems).ToList();
            itemStates.Items.Clear();
            itemStates.Items.AddRange(orderedItems);
            itemStates.Items.AddRange(nonReorderedItems);

            if (activeItems.Count > 1)
            {
                LogCollector.Add(_propertyName, $"[{_propertyName}] {context.ModKey}: Reordering complete");
            }
        }

        private bool HasModInMasters(string modKey, string ownerMod, IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var mod = state.LoadOrder[modKey].Mod;
            return mod?.MasterReferences.Any(m => m.Master.ToString() == ownerMod) == true;
        }
    }
}
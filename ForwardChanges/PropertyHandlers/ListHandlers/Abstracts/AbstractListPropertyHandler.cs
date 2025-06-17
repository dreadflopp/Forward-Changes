using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ListHandlers.Abstracts
{
    public abstract class AbstractListPropertyHandler<TRecord, TItem> : IPropertyHandler<List<ListItemContext<TItem>>>
        where TRecord : class, IMajorRecordGetter
        where TItem : class
    {
        protected readonly Func<TRecord, IReadOnlyList<ListItemContext<TItem>>> ListAccessor;

        /// <summary>
        /// Tells the record handler that this is a list handler.
        /// </summary>
        public bool IsListHandler => true;

        protected AbstractListPropertyHandler(
            Func<TRecord, IReadOnlyList<ListItemContext<TItem>>> listAccessor)
        {
            ListAccessor = listAccessor;
        }

        public abstract string PropertyName { get; }

        public abstract void SetValue(IMajorRecord record, List<ListItemContext<TItem>>? value);

        public virtual List<ListItemContext<TItem>>? GetValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is not TRecord typedRecord)
                return null;

            return ListAccessor(typedRecord).ToList();
        }

        public abstract bool AreValuesEqual(List<ListItemContext<TItem>>? value1, List<ListItemContext<TItem>>? value2);

        protected abstract bool IsItemEqual(TItem item1, TItem item2);

        public abstract void UpdatePropertyContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            PropertyContext propertyContext);

        protected void InsertItemAtCorrectPosition(
            ListItemContext<TItem> newItem,
            IReadOnlyList<TItem> recordItems,
            List<ListItemContext<TItem>> currentForwardItems)
        {
            // Find the position in the record items
            var recordIndex = recordItems.IndexOf(newItem.Item);
            if (recordIndex == -1)
            {
                // If not found, add at the end
                currentForwardItems.Add(newItem);
                return;
            }

            // Find the position in the final items
            var finalIndex = currentForwardItems.FindIndex(item =>
                !item.IsRemoved && recordItems.IndexOf(item.Item) > recordIndex);

            if (finalIndex == -1)
            {
                // If no items after this one, add at the end
                currentForwardItems.Add(newItem);
            }
            else
            {
                // Insert at the correct position
                currentForwardItems.Insert(finalIndex, newItem);
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
        /// Sort the items to match the context order.
        /// </summary>
        /// <param name="currentForwardItems">The list of items to reorder</param>
        /// <param name="state">The patcher state</param>
        /// <param name="context">The mod context</param>
        public virtual void SortItemsToMatchContextOrder(
            List<ListItemContext<TItem>> currentForwardItems,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is not TRecord record)
                return;

            var contextList = ListAccessor(record).ToList();
            // Only include items that the context has permission to reorder
            var activeItems = currentForwardItems.Where(i => !i.IsRemoved &&
                contextList.Any(c => IsItemEqual(c.Item, i.Item)) &&
                (i.OwnerMod == context.ModKey.ToString() || HasModInMasters(context.ModKey.ToString(), i.OwnerMod, state))).ToList();

            if (activeItems.Count > 1)
            {
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Reordering {activeItems.Count} items");
            }

            // Update ordering information for each item
            for (int i = 0; i < contextList.Count; i++)
            {
                var contextItem = contextList[i];
                var existingItem = activeItems.FirstOrDefault(w => IsItemEqual(w.Item, contextItem.Item));

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
            var orderedItems = new List<ListItemContext<TItem>>();
            var remainingItems = new List<ListItemContext<TItem>>(activeItems);

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
            var nonReorderedItems = currentForwardItems.Except(activeItems).ToList();
            currentForwardItems.Clear();
            currentForwardItems.AddRange(orderedItems);
            currentForwardItems.AddRange(nonReorderedItems);

            if (activeItems.Count > 1)
            {
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Reordering complete");
            }
        }

        private bool HasModInMasters(string modKey, string ownerMod, IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var mod = state.LoadOrder[modKey].Mod;
            return mod?.MasterReferences.Any(m => m.Master.ToString() == ownerMod) == true;
        }
    }
}
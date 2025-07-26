using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Noggog;

namespace ForwardChanges.PropertyHandlers.Container
{
    public class ContainerItemListPropertyHandler : AbstractListPropertyHandler<ContainerEntry>
    {
        public override string PropertyName => "Items";

        public override void SetValue(IMajorRecord record, List<ContainerEntry>? value)
        {
            if (record is IContainer container)
            {
                container.Items = value != null ? new ExtendedList<ContainerEntry>(value) : null;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IContainer for {PropertyName}");
            }
        }

        public override List<ContainerEntry>? GetValue(IMajorRecordGetter record)
        {
            if (record is IContainerGetter container)
            {
                return container.Items?.Select(item => new ContainerEntry
                {
                    Item = new ContainerItem
                    {
                        Item = new FormLink<IItemGetter>(item.Item.Item.FormKey),
                        Count = item.Item.Count
                    }
                }).ToList();
            }

            Console.WriteLine($"Error: Record does not implement IContainerGetter for {PropertyName}");
            return null;
        }

        protected override bool IsItemEqual(ContainerEntry? item1, ContainerEntry? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            return item1.Item.Item.FormKey == item2.Item.Item.FormKey &&
                   item1.Item.Count == item2.Item.Count;
        }

        protected override string FormatItem(ContainerEntry? item)
        {
            if (item == null) return "null";
            return $"{item.Item.Item.FormKey} (Count: {item.Item.Count})";
        }

        protected override void ProcessHandlerSpecificLogic(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            ListPropertyContext<ContainerEntry> listPropertyContext,
            List<ContainerEntry> recordItems,
            List<ListPropertyValueContext<ContainerEntry>> currentForwardItems)
        {
            var recordMod = state.LoadOrder[context.ModKey].Mod;
            if (recordMod == null) return;

            // Update count metadata for items that are in the record and not removed
            foreach (var forwardItem in currentForwardItems.Where(i => !i.IsRemoved))
            {
                var matchingRecordItem = recordItems.FirstOrDefault(recordItem =>
                    recordItem.Item.Item.FormKey == forwardItem.Value.Item.Item.FormKey);

                if (matchingRecordItem != null)
                {
                    // Update count if it's different and we have permissions
                    if (matchingRecordItem.Item.Count != forwardItem.Value.Item.Count)
                    {
                        if (HasPermissionsToModify(recordMod, forwardItem.OwnerMod))
                        {
                            var oldCount = forwardItem.Value.Item.Count;
                            var oldOwner = forwardItem.OwnerMod;
                            forwardItem.Value.Item.Count = matchingRecordItem.Item.Count;
                            forwardItem.OwnerMod = context.ModKey.ToString();
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Updated count {oldCount} -> {matchingRecordItem.Item.Count} for {forwardItem.Value.Item.Item.FormKey} (was owned by {oldOwner}) Success");
                        }
                        else
                        {
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Cannot update count for {forwardItem.Value.Item.Item.FormKey} - no permission (owned by {forwardItem.OwnerMod})");
                        }
                    }
                }
            }
        }
    }
}
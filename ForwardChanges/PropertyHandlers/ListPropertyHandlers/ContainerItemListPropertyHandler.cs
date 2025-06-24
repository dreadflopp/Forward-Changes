using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Noggog;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class ContainerItemListPropertyHandler : ItemListPropertyHandler<ContainerEntry>
    {
        public override string PropertyName => "Items";

        public override void SetValue(IMajorRecord record, List<ContainerEntry>? value)
        {
            if (record is IContainer container)
            {
                container.Items = value != null ? new ExtendedList<ContainerEntry>(value) : null;
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
            return null;
        }

        protected override FormKey GetItemFormKey(ContainerEntry item)
        {
            return item.Item.Item.FormKey;
        }

        protected override int GetItemCount(ContainerEntry item)
        {
            return item.Item.Count;
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
    }
}
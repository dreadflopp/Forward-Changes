using System.Collections.Generic;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.ConstructibleObject
{
    public class ItemsHandler : AbstractListPropertyHandler<IContainerEntryGetter>
    {
        public override string PropertyName => "Items";
        public override ListSemantics Semantics => ListSemantics.SortedKeyed;

        protected override bool IsItemIdentityEqual(IContainerEntryGetter? left, IContainerEntryGetter? right) =>
            left?.Item.Item.FormKey == right?.Item.Item.FormKey;

        protected override IReadOnlyList<object?> GetSortKey(IContainerEntryGetter item) => [item.Item.Item.FormKey];

        public override List<IContainerEntryGetter>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            if (record is IConstructibleObjectGetter constructibleObject)
            {
                return constructibleObject.Items?.ToList();
            }

            return null;
        }

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, List<IContainerEntryGetter>? value)
        {
            if (record is not IConstructibleObject constructibleObject)
            {
                return;
            }

            if (constructibleObject.Items == null)
            {
                return;
            }

            constructibleObject.Items.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var entry in value)
            {
                if (entry == null) continue;
                constructibleObject.Items.Add(entry.DeepCopy());
            }
        }
    }
}

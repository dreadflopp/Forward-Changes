using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class SpawnContainerHandler : AbstractFormLinkPropertyHandler<IPlacedObject, IPlacedObjectGetter, IPlacedObjectGetter>
    {
        public override string PropertyName => "SpawnContainer";

        protected override IFormLinkNullableGetter<IPlacedObjectGetter> GetFormLinkValue(IPlacedObjectGetter record)
        {
            return record.SpawnContainer;
        }

        protected override void SetFormLinkValue(IPlacedObject record, IFormLinkNullableGetter<IPlacedObjectGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.SpawnContainer = new FormLinkNullable<IPlacedObjectGetter>(value.FormKey);
            }
            else
            {
                record.SpawnContainer.Clear();
            }
        }
    }
}
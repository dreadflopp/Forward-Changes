using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class MultiboundReferenceHandler : AbstractFormLinkPropertyHandler<IPlacedObject, IPlacedObjectGetter, IPlacedObjectGetter>
    {
        public override string PropertyName => "MultiboundReference";

        protected override IFormLinkNullableGetter<IPlacedObjectGetter> GetFormLinkValue(IPlacedObjectGetter record)
        {
            return record.MultiboundReference;
        }

        protected override void SetFormLinkValue(IPlacedObject record, IFormLinkNullableGetter<IPlacedObjectGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.MultiboundReference = new FormLinkNullable<IPlacedObjectGetter>(value.FormKey);
            }
            else
            {
                record.MultiboundReference.Clear();
            }
        }
    }
}
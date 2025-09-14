using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class AttachRefHandler : AbstractFormLinkPropertyHandler<IPlacedObject, IPlacedObjectGetter, IPlacedThingGetter>
    {
        public override string PropertyName => "AttachRef";

        protected override IFormLinkNullableGetter<IPlacedThingGetter> GetFormLinkValue(IPlacedObjectGetter record)
        {
            return record.AttachRef;
        }

        protected override void SetFormLinkValue(IPlacedObject record, IFormLinkNullableGetter<IPlacedThingGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.AttachRef = new FormLinkNullable<IPlacedThingGetter>(value.FormKey);
            }
            else
            {
                record.AttachRef.Clear();
            }
        }
    }
}
using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class LocationReferenceHandler : AbstractFormLinkPropertyHandler<IPlacedNpc, IPlacedNpcGetter, ILocationRecordGetter>
    {
        public override string PropertyName => "LocationReference";

        protected override IFormLinkNullableGetter<ILocationRecordGetter> GetFormLinkValue(IPlacedNpcGetter record)
        {
            return record.LocationReference;
        }

        protected override void SetFormLinkValue(IPlacedNpc record, IFormLinkNullableGetter<ILocationRecordGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.LocationReference = new FormLinkNullable<ILocationRecordGetter>(value.FormKey);
            }
            else
            {
                record.LocationReference.Clear();
            }
        }
    }
}
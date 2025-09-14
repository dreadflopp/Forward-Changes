using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class LocationReferenceHandler : AbstractFormLinkPropertyHandler<IPlacedObject, IPlacedObjectGetter, ILocationRecordGetter>
    {
        public override string PropertyName => "LocationReference";

        protected override IFormLinkNullableGetter<ILocationRecordGetter>? GetFormLinkValue(IPlacedObjectGetter record)
        {
            return record.LocationReference;
        }

        protected override void SetFormLinkValue(IPlacedObject record, IFormLinkNullableGetter<ILocationRecordGetter>? value)
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


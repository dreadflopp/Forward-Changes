using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class LocationReferenceHandler : AbstractPropertyHandler<IFormLinkNullableGetter<ILocationRecordGetter>>
    {
        public override string PropertyName => "LocationReference";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<ILocationRecordGetter>? value)
        {
            if (record is IPlacedObject placedObject)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    placedObject.LocationReference = new FormLinkNullable<ILocationRecordGetter>(value.FormKey);
                }
                else
                {
                    placedObject.LocationReference.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            }
        }

        public override IFormLinkNullableGetter<ILocationRecordGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObject)
            {
                return placedObject.LocationReference;
            }
            else
            {
                Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<ILocationRecordGetter>? value1, IFormLinkNullableGetter<ILocationRecordGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}


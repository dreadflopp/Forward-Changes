using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.EncounterZone
{
    public class LocationHandler : AbstractPropertyHandler<IFormLinkNullableGetter<ILocationGetter>>
    {
        public override string PropertyName => "Location";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<ILocationGetter>? value)
        {
            if (record is IEncounterZone encounterZoneRecord)
            {
                if (value != null)
                {
                    encounterZoneRecord.Location.SetTo(value.FormKey);
                }
                else
                {
                    encounterZoneRecord.Location.SetTo(null);
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IEncounterZone for {PropertyName}");
            }
        }

        public override IFormLinkNullableGetter<ILocationGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IEncounterZoneGetter encounterZoneRecord)
            {
                return encounterZoneRecord.Location as IFormLinkNullableGetter<ILocationGetter>;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IEncounterZoneGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<ILocationGetter>? value1, IFormLinkNullableGetter<ILocationGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}
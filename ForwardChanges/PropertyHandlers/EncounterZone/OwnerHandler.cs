using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.EncounterZone
{
    public class OwnerHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IOwnerGetter>>
    {
        public override string PropertyName => "Owner";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IOwnerGetter>? value)
        {
            if (record is IEncounterZone encounterZoneRecord)
            {
                if (value != null)
                {
                    encounterZoneRecord.Owner.SetTo(value.FormKey);
                }
                else
                {
                    encounterZoneRecord.Owner.SetTo(null);
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IEncounterZone for {PropertyName}");
            }
        }

        public override IFormLinkNullableGetter<IOwnerGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IEncounterZoneGetter encounterZoneRecord)
            {
                return encounterZoneRecord.Owner as IFormLinkNullableGetter<IOwnerGetter>;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IEncounterZoneGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IOwnerGetter>? value1, IFormLinkNullableGetter<IOwnerGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}
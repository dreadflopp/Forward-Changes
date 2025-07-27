using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.EncounterZone
{
    public class FlagsHandler : AbstractPropertyHandler<Mutagen.Bethesda.Skyrim.EncounterZone.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.EncounterZone.Flag value)
        {
            if (record is IEncounterZone encounterZoneRecord)
            {
                encounterZoneRecord.Flags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IEncounterZone for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.EncounterZone.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IEncounterZoneGetter encounterZoneRecord)
            {
                return encounterZoneRecord.Flags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IEncounterZoneGetter for {PropertyName}");
            }
            return 0;
        }

        public override bool AreValuesEqual(Mutagen.Bethesda.Skyrim.EncounterZone.Flag value1, Mutagen.Bethesda.Skyrim.EncounterZone.Flag value2)
        {
            return value1 == value2;
        }
    }
}
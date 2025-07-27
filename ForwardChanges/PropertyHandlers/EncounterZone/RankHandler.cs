using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.EncounterZone
{
    public class RankHandler : AbstractPropertyHandler<byte>
    {
        public override string PropertyName => "Rank";

        public override void SetValue(IMajorRecord record, byte value)
        {
            if (record is IEncounterZone encounterZoneRecord)
            {
                encounterZoneRecord.Rank = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IEncounterZone for {PropertyName}");
            }
        }

        public override byte GetValue(IMajorRecordGetter record)
        {
            if (record is IEncounterZoneGetter encounterZoneRecord)
            {
                return encounterZoneRecord.Rank;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IEncounterZoneGetter for {PropertyName}");
            }
            return 0;
        }

        public override bool AreValuesEqual(byte value1, byte value2)
        {
            return value1 == value2;
        }
    }
}
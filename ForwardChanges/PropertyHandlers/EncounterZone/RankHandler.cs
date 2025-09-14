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
            var encounterZoneRecord = TryCastRecord<IEncounterZone>(record, PropertyName);
            if (encounterZoneRecord != null)
            {
                encounterZoneRecord.Rank = value;
            }
        }

        public override byte GetValue(IMajorRecordGetter record)
        {
            var encounterZoneRecord = TryCastRecord<IEncounterZoneGetter>(record, PropertyName);
            if (encounterZoneRecord != null)
            {
                return encounterZoneRecord.Rank;
            }
            return 0;
        }


    }
}
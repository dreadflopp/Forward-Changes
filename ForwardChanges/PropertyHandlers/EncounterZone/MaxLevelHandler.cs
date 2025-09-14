using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.EncounterZone
{
    public class MaxLevelHandler : AbstractPropertyHandler<byte>
    {
        public override string PropertyName => "MaxLevel";

        public override void SetValue(IMajorRecord record, byte value)
        {
            var encounterZoneRecord = TryCastRecord<IEncounterZone>(record, PropertyName);
            if (encounterZoneRecord != null)
            {
                encounterZoneRecord.MaxLevel = value;
            }
        }

        public override byte GetValue(IMajorRecordGetter record)
        {
            var encounterZoneRecord = TryCastRecord<IEncounterZoneGetter>(record, PropertyName);
            if (encounterZoneRecord != null)
            {
                return encounterZoneRecord.MaxLevel;
            }
            return 0;
        }


    }
}
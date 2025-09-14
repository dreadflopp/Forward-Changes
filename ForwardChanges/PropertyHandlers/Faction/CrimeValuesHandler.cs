using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class CrimeValuesHandler : AbstractPropertyHandler<CrimeValues>
    {
        public override string PropertyName => "CrimeValues";

        public override CrimeValues? GetValue(IMajorRecordGetter record)
        {
            var factionRecord = TryCastRecord<IFactionGetter>(record, PropertyName);
            if (factionRecord != null)
            {
                return factionRecord.CrimeValues as CrimeValues;
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, CrimeValues? value)
        {
            var factionRecord = TryCastRecord<IFaction>(record, PropertyName);
            if (factionRecord != null)
            {
                factionRecord.CrimeValues = value;
            }
        }
    }
}


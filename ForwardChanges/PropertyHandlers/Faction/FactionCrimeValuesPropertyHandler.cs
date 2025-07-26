using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class FactionCrimeValuesPropertyHandler : AbstractPropertyHandler<CrimeValues>
    {
        public override string PropertyName => "CrimeValues";

        public override CrimeValues? GetValue(IMajorRecordGetter record)
        {
            if (record is IFactionGetter factionRecord)
            {
                return factionRecord.CrimeValues as CrimeValues;
            }

            Console.WriteLine($"Error: Record does not implement IFactionGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, CrimeValues? value)
        {
            if (record is IFaction factionRecord)
            {
                factionRecord.CrimeValues = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IFaction for {PropertyName}");
            }
        }
    }
}
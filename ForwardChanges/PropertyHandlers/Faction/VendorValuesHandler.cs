using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class VendorValuesHandler : AbstractPropertyHandler<VendorValues>
    {
        public override string PropertyName => "VendorValues";

        public override VendorValues? GetValue(IMajorRecordGetter record)
        {
            var factionRecord = TryCastRecord<IFactionGetter>(record, PropertyName);
            if (factionRecord != null)
            {
                return factionRecord.VendorValues as VendorValues;
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, VendorValues? value)
        {
            var factionRecord = TryCastRecord<IFaction>(record, PropertyName);
            if (factionRecord != null)
            {
                factionRecord.VendorValues = value;
            }
        }
    }
}


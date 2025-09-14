using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class VendorLocationHandler : AbstractPropertyHandler<LocationTargetRadius>
    {
        public override string PropertyName => "VendorLocation";

        public override LocationTargetRadius? GetValue(IMajorRecordGetter record)
        {
            var factionRecord = TryCastRecord<IFactionGetter>(record, PropertyName);
            if (factionRecord != null)
            {
                return factionRecord.VendorLocation as LocationTargetRadius;
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, LocationTargetRadius? value)
        {
            var factionRecord = TryCastRecord<IFaction>(record, PropertyName);
            if (factionRecord != null)
            {
                factionRecord.VendorLocation = value;
            }
        }
    }
}


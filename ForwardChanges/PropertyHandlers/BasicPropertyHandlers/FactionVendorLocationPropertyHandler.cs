using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class FactionVendorLocationPropertyHandler : AbstractPropertyHandler<LocationTargetRadius>
    {
        public override string PropertyName => "VendorLocation";

        public override LocationTargetRadius? GetValue(IMajorRecordGetter record)
        {
            if (record is IFactionGetter factionRecord)
            {
                return factionRecord.VendorLocation as LocationTargetRadius;
            }

            Console.WriteLine($"Error: Record does not implement IFactionGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, LocationTargetRadius? value)
        {
            if (record is IFaction factionRecord)
            {
                factionRecord.VendorLocation = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IFaction for {PropertyName}");
            }
        }
    }
}
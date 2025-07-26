using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class LocationWorldLocationRadiusPropertyHandler : AbstractPropertyHandler<float?>
    {
        public override string PropertyName => "WorldLocationRadius";

        public override float? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.WorldLocationRadius;
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, float? value)
        {
            if (record is ILocation locationRecord)
            {
                locationRecord.WorldLocationRadius = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ILocation for {PropertyName}");
            }
        }
    }
}
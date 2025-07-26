using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class LocationWorldLocationMarkerRefPropertyHandler : AbstractPropertyHandler<IFormLinkNullable<IPlacedSimpleGetter>>
    {
        public override string PropertyName => "WorldLocationMarkerRef";

        public override IFormLinkNullable<IPlacedSimpleGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.WorldLocationMarkerRef as IFormLinkNullable<IPlacedSimpleGetter>;
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkNullable<IPlacedSimpleGetter>? value)
        {
            if (record is ILocation locationRecord)
            {
                locationRecord.WorldLocationMarkerRef = value ?? new FormLinkNullable<IPlacedSimpleGetter>();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ILocation for {PropertyName}");
            }
        }
    }
}
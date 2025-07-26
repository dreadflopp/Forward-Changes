using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class LocationHorseMarkerRefPropertyHandler : AbstractPropertyHandler<IFormLinkNullable<IPlacedObjectGetter>>
    {
        public override string PropertyName => "HorseMarkerRef";

        public override IFormLinkNullable<IPlacedObjectGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.HorseMarkerRef as IFormLinkNullable<IPlacedObjectGetter>;
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkNullable<IPlacedObjectGetter>? value)
        {
            if (record is ILocation locationRecord)
            {
                locationRecord.HorseMarkerRef = value ?? new FormLinkNullable<IPlacedObjectGetter>();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ILocation for {PropertyName}");
            }
        }
    }
}
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class LocationCellMarkerReferenceHandler : AbstractListPropertyHandler<IFormLinkGetter<IPlacedGetter>>
    {
        public override string PropertyName => "LocationCellMarkerReference";

        public override List<IFormLinkGetter<IPlacedGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.LocationCellMarkerReference?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<IPlacedGetter>>? value)
        {
            if (record is ILocation locationRecord)
            {
                if (locationRecord.LocationCellMarkerReference != null)
                {
                    locationRecord.LocationCellMarkerReference.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            locationRecord.LocationCellMarkerReference.Add(item);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ILocation for {PropertyName}");
            }
        }
    }
}


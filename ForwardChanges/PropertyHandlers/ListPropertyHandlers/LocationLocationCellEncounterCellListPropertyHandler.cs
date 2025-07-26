using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class LocationLocationCellEncounterCellListPropertyHandler : AbstractListPropertyHandler<ILocationCoordinateGetter>
    {
        public override string PropertyName => "LocationCellEncounterCell";

        public override List<ILocationCoordinateGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.LocationCellEncounterCell?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<ILocationCoordinateGetter>? value)
        {
            if (record is ILocation locationRecord)
            {
                if (locationRecord.LocationCellEncounterCell != null)
                {
                    locationRecord.LocationCellEncounterCell.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            if (item is LocationCoordinate castItem)
                            {
                                locationRecord.LocationCellEncounterCell.Add(castItem);
                            }
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
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class LocationCellUniquesHandler : AbstractListPropertyHandler<ILocationCellUniqueGetter>
    {
        public override string PropertyName => "LocationCellUniques";

        public override List<ILocationCellUniqueGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.LocationCellUniques?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<ILocationCellUniqueGetter>? value)
        {
            if (record is ILocation locationRecord)
            {
                if (locationRecord.LocationCellUniques != null)
                {
                    locationRecord.LocationCellUniques.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            if (item is LocationCellUnique castItem)
                            {
                                locationRecord.LocationCellUniques.Add(castItem);
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


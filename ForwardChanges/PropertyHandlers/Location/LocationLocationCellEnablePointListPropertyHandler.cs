using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class LocationLocationCellEnablePointListPropertyHandler : AbstractListPropertyHandler<ILocationCellEnablePointGetter>
    {
        public override string PropertyName => "LocationCellEnablePoint";

        public override List<ILocationCellEnablePointGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.LocationCellEnablePoint?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<ILocationCellEnablePointGetter>? value)
        {
            if (record is ILocation locationRecord)
            {
                if (locationRecord.LocationCellEnablePoint != null)
                {
                    locationRecord.LocationCellEnablePoint.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            if (item is LocationCellEnablePoint castItem)
                            {
                                locationRecord.LocationCellEnablePoint.Add(castItem);
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
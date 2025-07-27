using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class ReferenceCellEncounterCellHandler : AbstractListPropertyHandler<ILocationCoordinateGetter>
    {
        public override string PropertyName => "ReferenceCellEncounterCell";

        public override List<ILocationCoordinateGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.ReferenceCellEncounterCell?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<ILocationCoordinateGetter>? value)
        {
            if (record is ILocation locationRecord)
            {
                if (locationRecord.ReferenceCellEncounterCell != null)
                {
                    locationRecord.ReferenceCellEncounterCell.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            if (item is LocationCoordinate castItem)
                            {
                                locationRecord.ReferenceCellEncounterCell.Add(castItem);
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


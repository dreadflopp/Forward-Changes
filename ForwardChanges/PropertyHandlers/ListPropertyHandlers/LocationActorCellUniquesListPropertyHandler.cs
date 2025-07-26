using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class LocationActorCellUniquesListPropertyHandler : AbstractListPropertyHandler<ILocationCellUniqueGetter>
    {
        public override string PropertyName => "ActorCellUniques";

        public override List<ILocationCellUniqueGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.ActorCellUniques?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<ILocationCellUniqueGetter>? value)
        {
            if (record is ILocation locationRecord)
            {
                if (locationRecord.ActorCellUniques != null)
                {
                    locationRecord.ActorCellUniques.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            if (item is LocationCellUnique castItem)
                            {
                                locationRecord.ActorCellUniques.Add(castItem);
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
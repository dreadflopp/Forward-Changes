using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class LocationActorCellMarkerReferenceListPropertyHandler : AbstractListPropertyHandler<IFormLinkGetter<IPlacedGetter>>
    {
        public override string PropertyName => "ActorCellMarkerReference";

        public override List<IFormLinkGetter<IPlacedGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.ActorCellMarkerReference?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<IPlacedGetter>>? value)
        {
            if (record is ILocation locationRecord)
            {
                if (locationRecord.ActorCellMarkerReference != null)
                {
                    locationRecord.ActorCellMarkerReference.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            locationRecord.ActorCellMarkerReference.Add(item);
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
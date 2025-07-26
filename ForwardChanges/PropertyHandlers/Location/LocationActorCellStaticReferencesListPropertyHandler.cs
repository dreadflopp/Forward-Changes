using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class LocationActorCellStaticReferencesListPropertyHandler : AbstractListPropertyHandler<ILocationCellStaticReferenceGetter>
    {
        public override string PropertyName => "ActorCellStaticReferences";

        public override List<ILocationCellStaticReferenceGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.ActorCellStaticReferences?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<ILocationCellStaticReferenceGetter>? value)
        {
            if (record is ILocation locationRecord)
            {
                if (locationRecord.ActorCellStaticReferences != null)
                {
                    locationRecord.ActorCellStaticReferences.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            if (item is LocationCellStaticReference castItem)
                            {
                                locationRecord.ActorCellStaticReferences.Add(castItem);
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
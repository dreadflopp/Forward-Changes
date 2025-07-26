using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class LocationActorCellPersistentReferencesListPropertyHandler : AbstractListPropertyHandler<ILocationReferenceGetter>
    {
        public override string PropertyName => "ActorCellPersistentReferences";

        public override List<ILocationReferenceGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.ActorCellPersistentReferences?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<ILocationReferenceGetter>? value)
        {
            if (record is ILocation locationRecord)
            {
                if (locationRecord.ActorCellPersistentReferences != null)
                {
                    locationRecord.ActorCellPersistentReferences.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            if (item is LocationReference castItem)
                            {
                                locationRecord.ActorCellPersistentReferences.Add(castItem);
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
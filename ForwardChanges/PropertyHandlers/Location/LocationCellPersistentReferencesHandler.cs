using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class LocationCellPersistentReferencesHandler : AbstractListPropertyHandler<ILocationReferenceGetter>
    {
        public override string PropertyName => "LocationCellPersistentReferences";

        public override List<ILocationReferenceGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.LocationCellPersistentReferences?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<ILocationReferenceGetter>? value)
        {
            if (record is ILocation locationRecord)
            {
                if (locationRecord.LocationCellPersistentReferences != null)
                {
                    locationRecord.LocationCellPersistentReferences.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            if (item is LocationReference castItem)
                            {
                                locationRecord.LocationCellPersistentReferences.Add(castItem);
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


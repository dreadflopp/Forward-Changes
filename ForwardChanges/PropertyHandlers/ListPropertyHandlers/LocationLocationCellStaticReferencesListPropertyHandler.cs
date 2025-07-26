using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class LocationLocationCellStaticReferencesListPropertyHandler : AbstractListPropertyHandler<ILocationCellStaticReferenceGetter>
    {
        public override string PropertyName => "LocationCellStaticReferences";

        public override List<ILocationCellStaticReferenceGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.LocationCellStaticReferences?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<ILocationCellStaticReferenceGetter>? value)
        {
            if (record is ILocation locationRecord)
            {
                if (locationRecord.LocationCellStaticReferences != null)
                {
                    locationRecord.LocationCellStaticReferences.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            if (item is LocationCellStaticReference castItem)
                            {
                                locationRecord.LocationCellStaticReferences.Add(castItem);
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
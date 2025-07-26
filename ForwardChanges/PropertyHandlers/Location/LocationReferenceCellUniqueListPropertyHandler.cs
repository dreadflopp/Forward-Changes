using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class LocationReferenceCellUniqueListPropertyHandler : AbstractListPropertyHandler<IFormLinkGetter<INpcGetter>>
    {
        public override string PropertyName => "ReferenceCellUnique";

        public override List<IFormLinkGetter<INpcGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.ReferenceCellUnique?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<INpcGetter>>? value)
        {
            if (record is ILocation locationRecord)
            {
                if (locationRecord.ReferenceCellUnique != null)
                {
                    locationRecord.ReferenceCellUnique.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            locationRecord.ReferenceCellUnique.Add(item);
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
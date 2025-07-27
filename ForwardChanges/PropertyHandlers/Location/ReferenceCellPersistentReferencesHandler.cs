using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class ReferenceCellPersistentReferencesHandler : AbstractListPropertyHandler<IFormLinkGetter<IPlacedSimpleGetter>>
    {
        public override string PropertyName => "ReferenceCellPersistentReferences";

        public override List<IFormLinkGetter<IPlacedSimpleGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.ReferenceCellPersistentReferences?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<IPlacedSimpleGetter>>? value)
        {
            if (record is ILocation locationRecord)
            {
                if (locationRecord.ReferenceCellPersistentReferences != null)
                {
                    locationRecord.ReferenceCellPersistentReferences.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            locationRecord.ReferenceCellPersistentReferences.Add(item);
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


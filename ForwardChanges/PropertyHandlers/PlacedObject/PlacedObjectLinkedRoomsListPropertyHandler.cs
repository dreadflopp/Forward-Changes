using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class PlacedObjectLinkedRoomsListPropertyHandler : AbstractListPropertyHandler<IFormLinkGetter<IPlacedObjectGetter>>
    {
        public override string PropertyName => "LinkedRooms";

                public override List<IFormLinkGetter<IPlacedObjectGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.LinkedRooms?.ToList();
            }
            
            Console.WriteLine($"Error: Record does not implement IPlacedObjectGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<IPlacedObjectGetter>>? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.LinkedRooms.Clear();
                if (value != null)
                {
                    foreach (var item in value)
                    {
                        placedObjectRecord.LinkedRooms.Add(item);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IPlacedObject for {PropertyName}");
            }
        }
    }
}
using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class ItemCountHandler : AbstractPropertyHandler<int?>
    {
        public override string PropertyName => "ItemCount";

        public override void SetValue(IMajorRecord record, int? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.ItemCount = value;
            }
        }

        public override int? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.ItemCount;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }
    }
}
using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class CollisionLayerHandler : AbstractPropertyHandler<uint?>
    {
        public override string PropertyName => "CollisionLayer";

        public override void SetValue(IMajorRecord record, uint? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.CollisionLayer = value;
            }
        }

        public override uint? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.CollisionLayer;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }
    }
}
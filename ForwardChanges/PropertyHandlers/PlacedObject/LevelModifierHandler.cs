using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class LevelModifierHandler : AbstractPropertyHandler<Level?>
    {
        public override string PropertyName => "LevelModifier";

        public override void SetValue(IMajorRecord record, Level? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.LevelModifier = value;
            }
        }

        public override Level? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.LevelModifier;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }
    }
}
using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class HeadTrackingWeightHandler : AbstractPropertyHandler<float?>
    {
        public override string PropertyName => "HeadTrackingWeight";

        public override void SetValue(IMajorRecord record, float? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.HeadTrackingWeight = value;
            }
        }

        public override float? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.HeadTrackingWeight;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        public override bool AreValuesEqual(float? value1, float? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Use epsilon comparison for float values
            return Math.Abs(value1.Value - value2.Value) < 0.0001f;
        }
    }
}
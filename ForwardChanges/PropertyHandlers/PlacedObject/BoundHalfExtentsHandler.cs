using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class BoundHalfExtentsHandler : AbstractPropertyHandler<P3Float?>
    {
        public override string PropertyName => "BoundHalfExtents";

        public override void SetValue(IMajorRecord record, P3Float? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.BoundHalfExtents = value;
            }
        }

        public override P3Float? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.BoundHalfExtents;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        public override bool AreValuesEqual(P3Float? value1, P3Float? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // P3Float has Equals method that uses EqualsWithin for float comparison
            return value1.Value.Equals(value2.Value);
        }
    }
}
using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class RoomPortalHandler : AbstractPropertyHandler<IBoundingGetter?>
    {
        public override string PropertyName => "RoomPortal";

        public override void SetValue(IMajorRecord record, IBoundingGetter? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                if (value != null)
                {
                    // Create new Bounding and copy properties
                    var newBounding = new Bounding
                    {
                        Width = value.Width,
                        Height = value.Height,
                        Position = value.Position,
                        RotationQ1 = value.RotationQ1,
                        RotationQ2 = value.RotationQ2,
                        RotationQ3 = value.RotationQ3,
                        RotationQ4 = value.RotationQ4
                    };
                    placedObjectRecord.RoomPortal = newBounding;
                }
                else
                {
                    placedObjectRecord.RoomPortal = null;
                }
            }
        }

        public override IBoundingGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.RoomPortal;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        public override bool AreValuesEqual(IBoundingGetter? value1, IBoundingGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties
            if (value1.Width != value2.Width) return false;
            if (value1.Height != value2.Height) return false;
            if (!value1.Position.Equals(value2.Position)) return false;
            if (value1.RotationQ1 != value2.RotationQ1) return false;
            if (value1.RotationQ2 != value2.RotationQ2) return false;
            if (value1.RotationQ3 != value2.RotationQ3) return false;
            if (value1.RotationQ4 != value2.RotationQ4) return false;

            return true;
        }
    }
}
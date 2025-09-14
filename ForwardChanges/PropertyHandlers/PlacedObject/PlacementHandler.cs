using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class PlacementHandler : AbstractPropertyHandler<IPlacementGetter?>
    {
        public override string PropertyName => "Placement";

        public override void SetValue(IMajorRecord record, IPlacementGetter? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                if (value != null)
                {
                    // Create new Placement and copy properties
                    var newPlacement = new Placement
                    {
                        Position = value.Position,
                        Rotation = value.Rotation
                    };
                    placedObjectRecord.Placement = newPlacement;
                }
                else
                {
                    placedObjectRecord.Placement = null;
                }
            }
        }

        public override IPlacementGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.Placement;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        public override bool AreValuesEqual(IPlacementGetter? value1, IPlacementGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties
            if (!value1.Position.Equals(value2.Position)) return false;
            if (!value1.Rotation.Equals(value2.Rotation)) return false;

            return true;
        }
    }
}
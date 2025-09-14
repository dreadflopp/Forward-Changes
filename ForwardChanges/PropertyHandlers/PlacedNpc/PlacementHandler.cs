using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class PlacementHandler : AbstractPropertyHandler<IPlacementGetter?>
    {
        public override string PropertyName => "Placement";

        public override void SetValue(IMajorRecord record, IPlacementGetter? value)
        {
            if (record is IPlacedNpc placedNpcRecord)
            {
                if (value == null)
                {
                    placedNpcRecord.Placement = null;
                    return;
                }

                // Create a deep copy
                var newPlacement = new Placement
                {
                    Position = value.Position,
                    Rotation = value.Rotation
                };

                placedNpcRecord.Placement = newPlacement;
            }
        }

        public override IPlacementGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedNpcGetter placedNpcRecord)
            {
                return placedNpcRecord.Placement;
            }
            return null;
        }

        public override bool AreValuesEqual(IPlacementGetter? value1, IPlacementGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            return value1.Position == value2.Position && value1.Rotation == value2.Rotation;
        }

        public override string FormatValue(object? value)
        {
            if (value is not IPlacementGetter placement)
            {
                return value?.ToString() ?? "null";
            }

            return $"Position: {placement.Position}, Rotation: {placement.Rotation}";
        }
    }
}
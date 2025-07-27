using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Noggog;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class PositionHandler : AbstractPropertyHandler<P3Float?>
    {
        public override string PropertyName => "Placement.Position";

        public override void SetValue(IMajorRecord record, P3Float? value)
        {
            if (record is IPlacedObject placedObject)
            {
                if (value.HasValue)
                {
                    // Ensure Placement exists
                    if (placedObject.Placement == null)
                    {
                        placedObject.Placement = new Placement();
                    }

                    placedObject.Placement.Position = value.Value;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            }
        }

        public override P3Float? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObject)
            {
                return placedObject.Placement?.Position;
            }
            else
            {
                Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(P3Float? value1, P3Float? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.Value.Equals(value2.Value); // P3Float has proper Equals implementation
        }
    }
}


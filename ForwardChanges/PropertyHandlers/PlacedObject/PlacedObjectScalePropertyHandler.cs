using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject 
{
    public class PlacedObjectScalePropertyHandler : AbstractPropertyHandler<float?>
    {
        public override string PropertyName => "Scale";

        public override void SetValue(IMajorRecord record, float? value)
        {
            if (record is IPlacedObject placedObject)
            {
                placedObject.Scale = value;
            }
            else
            {
                Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            }
        }

        public override float? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObject)
            {
                return placedObject.Scale;
            }
            else
            {
                Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(float? value1, float? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            // Use a smaller epsilon for more precise float comparison to handle rounding issues
            return Math.Abs(value1.Value - value2.Value) < 0.0001f;
        }
    }
}
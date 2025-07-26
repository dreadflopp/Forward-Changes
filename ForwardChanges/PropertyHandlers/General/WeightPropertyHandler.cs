using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Aspects;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.General
{
    public class WeightPropertyHandler : AbstractPropertyHandler<float?>
    {
        public override string PropertyName => "Weight";

        public override void SetValue(IMajorRecord record, float? value)
        {
            if (record is IWeightValue weightValue)
            {
                weightValue.Weight = value ?? 0f;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWeightValue for {PropertyName}");
            }
        }

        public override float? GetValue(IMajorRecordGetter record)
        {
            if (record is IWeightValueGetter weightValue)
            {
                return weightValue.Weight;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWeightValueGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(float? value1, float? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return Math.Abs(value1.Value - value2.Value) < 0.001f; // Use small epsilon for float comparison
        }
    }
}
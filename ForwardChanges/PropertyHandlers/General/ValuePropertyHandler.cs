using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Aspects;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.General
{
    public class ValuePropertyHandler : AbstractPropertyHandler<uint?>
    {
        public override string PropertyName => "Value";

        public override void SetValue(IMajorRecord record, uint? value)
        {
            if (record is IWeightValue weightValue)
            {
                weightValue.Value = value ?? 0u;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWeightValue for {PropertyName}");
            }
        }

        public override uint? GetValue(IMajorRecordGetter record)
        {
            if (record is IWeightValueGetter weightValue)
            {
                return weightValue.Value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWeightValueGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(uint? value1, uint? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.Value == value2.Value;
        }
    }
}
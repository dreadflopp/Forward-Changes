using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ObjectEffect
{
    public class ObjectEffectEnchantmentAmountPropertyHandler : AbstractPropertyHandler<int?>
    {
        public override string PropertyName => "EnchantmentAmount";

        public override void SetValue(IMajorRecord record, int? value)
        {
            if (record is IObjectEffect objectEffectRecord)
            {
                objectEffectRecord.EnchantmentAmount = value ?? 0;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffect for {PropertyName}");
            }
        }

        public override int? GetValue(IMajorRecordGetter record)
        {
            if (record is IObjectEffectGetter objectEffectRecord)
            {
                return objectEffectRecord.EnchantmentAmount;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffectGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(int? value1, int? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.Value == value2.Value;
        }
    }
}
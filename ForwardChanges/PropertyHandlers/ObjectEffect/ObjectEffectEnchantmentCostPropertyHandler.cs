using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ObjectEffect
{
    public class ObjectEffectEnchantmentCostPropertyHandler : AbstractPropertyHandler<uint?>
    {
        public override string PropertyName => "EnchantmentCost";

        public override void SetValue(IMajorRecord record, uint? value)
        {
            if (record is IObjectEffect objectEffectRecord)
            {
                objectEffectRecord.EnchantmentCost = value ?? 0u;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffect for {PropertyName}");
            }
        }

        public override uint? GetValue(IMajorRecordGetter record)
        {
            if (record is IObjectEffectGetter objectEffectRecord)
            {
                return objectEffectRecord.EnchantmentCost;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffectGetter for {PropertyName}");
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
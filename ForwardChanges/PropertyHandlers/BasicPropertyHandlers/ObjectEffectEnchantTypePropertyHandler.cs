using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class ObjectEffectEnchantTypePropertyHandler : AbstractPropertyHandler<ObjectEffect.EnchantTypeEnum?>
    {
        public override string PropertyName => "EnchantType";

        public override void SetValue(IMajorRecord record, ObjectEffect.EnchantTypeEnum? value)
        {
            if (record is IObjectEffect objectEffectRecord)
            {
                objectEffectRecord.EnchantType = value ?? ObjectEffect.EnchantTypeEnum.Enchantment;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffect for {PropertyName}");
            }
        }

        public override ObjectEffect.EnchantTypeEnum? GetValue(IMajorRecordGetter record)
        {
            if (record is IObjectEffectGetter objectEffectRecord)
            {
                return objectEffectRecord.EnchantType;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffectGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(ObjectEffect.EnchantTypeEnum? value1, ObjectEffect.EnchantTypeEnum? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.Value == value2.Value;
        }
    }
}
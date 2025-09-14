using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ObjectEffect
{
    public class EnchantTypeHandler : AbstractPropertyHandler<Mutagen.Bethesda.Skyrim.ObjectEffect.EnchantTypeEnum?>
    {
        public override string PropertyName => "EnchantType";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.ObjectEffect.EnchantTypeEnum? value)
        {
            var objectEffectRecord = TryCastRecord<IObjectEffect>(record, PropertyName);
            if (objectEffectRecord != null)
            {
                objectEffectRecord.EnchantType = value ?? Mutagen.Bethesda.Skyrim.ObjectEffect.EnchantTypeEnum.Enchantment;
            }
        }

        public override Mutagen.Bethesda.Skyrim.ObjectEffect.EnchantTypeEnum? GetValue(IMajorRecordGetter record)
        {
            var objectEffectRecord = TryCastRecord<IObjectEffectGetter>(record, PropertyName);
            if (objectEffectRecord != null)
            {
                return objectEffectRecord.EnchantType;
            }
            return null;
        }


    }
}


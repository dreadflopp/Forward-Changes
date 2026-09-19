using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ObjectEffect
{
    public class EnchantmentCostHandler : AbstractPropertyHandler<uint?>
    {
        public override string PropertyName => "EnchantmentCost";

        public override void SetValue(IMajorRecord record, uint? value)
        {
            var objectEffectRecord = TryCastRecord<IObjectEffect>(record, PropertyName);
            if (objectEffectRecord != null)
            {
                objectEffectRecord.EnchantmentCost = value ?? 0u;
            }
        }

        public override uint? GetValue(IMajorRecordGetter record)
        {
            var objectEffectRecord = TryCastRecord<IObjectEffectGetter>(record, PropertyName);
            if (objectEffectRecord != null)
            {
                return objectEffectRecord.EnchantmentCost;
            }
            return null;
        }


    }
}


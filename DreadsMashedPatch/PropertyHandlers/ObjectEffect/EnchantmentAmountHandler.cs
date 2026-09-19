using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;

namespace DreadsMashedPatch.PropertyHandlers.ObjectEffect
{
    public class EnchantmentAmountHandler : AbstractPropertyHandler<int?>
    {
        public override string PropertyName => "EnchantmentAmount";

        public override void SetValue(IMajorRecord record, int? value)
        {
            var objectEffectRecord = TryCastRecord<IObjectEffect>(record, PropertyName);
            if (objectEffectRecord != null)
            {
                objectEffectRecord.EnchantmentAmount = value ?? 0;
            }
        }

        public override int? GetValue(IMajorRecordGetter record)
        {
            var objectEffectRecord = TryCastRecord<IObjectEffectGetter>(record, PropertyName);
            if (objectEffectRecord != null)
            {
                return objectEffectRecord.EnchantmentAmount;
            }
            return null;
        }


    }
}


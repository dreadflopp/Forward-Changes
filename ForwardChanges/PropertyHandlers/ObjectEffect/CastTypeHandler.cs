using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ObjectEffect
{
    public class CastTypeHandler : AbstractPropertyHandler<CastType?>
    {
        public override string PropertyName => "CastType";

        public override void SetValue(IMajorRecord record, CastType? value)
        {
            var objectEffectRecord = TryCastRecord<IObjectEffect>(record, PropertyName);
            if (objectEffectRecord != null)
            {
                objectEffectRecord.CastType = value ?? CastType.ConstantEffect;
            }
        }

        public override CastType? GetValue(IMajorRecordGetter record)
        {
            var objectEffectRecord = TryCastRecord<IObjectEffectGetter>(record, PropertyName);
            if (objectEffectRecord != null)
            {
                return objectEffectRecord.CastType;
            }
            return null;
        }


    }
}


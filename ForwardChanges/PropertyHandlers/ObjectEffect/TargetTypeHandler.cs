using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ObjectEffect
{
    public class TargetTypeHandler : AbstractPropertyHandler<TargetType?>
    {
        public override string PropertyName => "TargetType";

        public override void SetValue(IMajorRecord record, TargetType? value)
        {
            var objectEffectRecord = TryCastRecord<IObjectEffect>(record, PropertyName);
            if (objectEffectRecord != null)
            {
                objectEffectRecord.TargetType = value ?? TargetType.Self;
            }
        }

        public override TargetType? GetValue(IMajorRecordGetter record)
        {
            var objectEffectRecord = TryCastRecord<IObjectEffectGetter>(record, PropertyName);
            if (objectEffectRecord != null)
            {
                return objectEffectRecord.TargetType;
            }
            return null;
        }


    }
}


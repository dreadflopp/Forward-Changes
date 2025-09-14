using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class TargetTypeHandler : AbstractPropertyHandler<TargetType>
    {
        public override string PropertyName => "TargetType";

        public override void SetValue(IMajorRecord record, TargetType value)
        {
            if (record is IMagicEffect magicEffect)
            {
                magicEffect.TargetType = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override TargetType GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.TargetType;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return TargetType.Self;
        }
    }
}

using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class CastTypeHandler : AbstractPropertyHandler<CastType>
    {
        public override string PropertyName => "CastType";

        public override void SetValue(IMajorRecord record, CastType value)
        {
            if (record is IMagicEffect magicEffect)
            {
                magicEffect.CastType = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override CastType GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.CastType;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return CastType.ConstantEffect;
        }
    }
}

using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class HitShaderHandler : AbstractPropertyHandler<IFormLinkGetter<IEffectShaderGetter>>
    {
        public override string PropertyName => "HitShader";

        public override void SetValue(IMajorRecord record, IFormLinkGetter<IEffectShaderGetter>? value)
        {
            if (record is IMagicEffect magicEffect)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    magicEffect.HitShader = new FormLink<IEffectShaderGetter>(value.FormKey);
                }
                else
                {
                    magicEffect.HitShader.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override IFormLinkGetter<IEffectShaderGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.HitShader;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return null;
        }
    }
}

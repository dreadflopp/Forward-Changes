using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class CastingLightHandler : AbstractPropertyHandler<IFormLinkGetter<ILightGetter>>
    {
        public override string PropertyName => "CastingLight";

        public override void SetValue(IMajorRecord record, IFormLinkGetter<ILightGetter>? value)
        {
            if (record is IMagicEffect magicEffect)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    magicEffect.CastingLight = new FormLink<ILightGetter>(value.FormKey);
                }
                else
                {
                    magicEffect.CastingLight.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override IFormLinkGetter<ILightGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.CastingLight;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return null;
        }
    }
}

using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class ImageSpaceModifierHandler : AbstractPropertyHandler<IFormLinkGetter<IImageSpaceAdapterGetter>>
    {
        public override string PropertyName => "ImageSpaceModifier";

        public override void SetValue(IMajorRecord record, IFormLinkGetter<IImageSpaceAdapterGetter>? value)
        {
            if (record is IMagicEffect magicEffect)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    magicEffect.ImageSpaceModifier = new FormLink<IImageSpaceAdapterGetter>(value.FormKey);
                }
                else
                {
                    magicEffect.ImageSpaceModifier.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override IFormLinkGetter<IImageSpaceAdapterGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.ImageSpaceModifier;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return null;
        }
    }
}

using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class CastingArtHandler : AbstractPropertyHandler<IFormLinkGetter<IArtObjectGetter>>
    {
        public override string PropertyName => "CastingArt";

        public override void SetValue(IMajorRecord record, IFormLinkGetter<IArtObjectGetter>? value)
        {
            if (record is IMagicEffect magicEffect)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    magicEffect.CastingArt = new FormLink<IArtObjectGetter>(value.FormKey);
                }
                else
                {
                    magicEffect.CastingArt.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override IFormLinkGetter<IArtObjectGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.CastingArt;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return null;
        }
    }
}

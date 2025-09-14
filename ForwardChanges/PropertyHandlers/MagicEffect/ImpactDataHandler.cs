using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class ImpactDataHandler : AbstractPropertyHandler<IFormLinkGetter<IImpactDataSetGetter>>
    {
        public override string PropertyName => "ImpactData";

        public override void SetValue(IMajorRecord record, IFormLinkGetter<IImpactDataSetGetter>? value)
        {
            if (record is IMagicEffect magicEffect)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    magicEffect.ImpactData = new FormLink<IImpactDataSetGetter>(value.FormKey);
                }
                else
                {
                    magicEffect.ImpactData.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override IFormLinkGetter<IImpactDataSetGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.ImpactData;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return null;
        }
    }
}

using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class SpellmakingAreaHandler : AbstractPropertyHandler<uint>
    {
        public override string PropertyName => "SpellmakingArea";

        public override void SetValue(IMajorRecord record, uint value)
        {
            if (record is IMagicEffect magicEffect)
            {
                magicEffect.SpellmakingArea = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override uint GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.SpellmakingArea;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return 0;
        }
    }
}

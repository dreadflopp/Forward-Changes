using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class CastingSoundLevelHandler : AbstractPropertyHandler<SoundLevel>
    {
        public override string PropertyName => "CastingSoundLevel";

        public override void SetValue(IMajorRecord record, SoundLevel value)
        {
            if (record is IMagicEffect magicEffect)
            {
                magicEffect.CastingSoundLevel = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override SoundLevel GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.CastingSoundLevel;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return SoundLevel.Normal;
        }
    }
}

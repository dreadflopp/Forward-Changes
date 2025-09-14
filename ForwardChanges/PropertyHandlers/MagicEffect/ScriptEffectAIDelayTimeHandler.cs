using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class ScriptEffectAIDelayTimeHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "ScriptEffectAIDelayTime";

        public override void SetValue(IMajorRecord record, float value)
        {
            if (record is IMagicEffect magicEffect)
            {
                magicEffect.ScriptEffectAIDelayTime = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override float GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.ScriptEffectAIDelayTime;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return 0f;
        }
    }
}


using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class ScriptEffectAIScoreHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "ScriptEffectAIScore";

        public override void SetValue(IMajorRecord record, float value)
        {
            if (record is IMagicEffect magicEffect)
            {
                magicEffect.ScriptEffectAIScore = value;
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
                return magicEffect.ScriptEffectAIScore;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return 0f;
        }
    }
}


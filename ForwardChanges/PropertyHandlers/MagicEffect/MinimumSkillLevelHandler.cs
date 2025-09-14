using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class MinimumSkillLevelHandler : AbstractPropertyHandler<uint>
    {
        public override string PropertyName => "MinimumSkillLevel";

        public override void SetValue(IMajorRecord record, uint value)
        {
            if (record is IMagicEffect magicEffect)
            {
                magicEffect.MinimumSkillLevel = value;
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
                return magicEffect.MinimumSkillLevel;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return 0;
        }
    }
}

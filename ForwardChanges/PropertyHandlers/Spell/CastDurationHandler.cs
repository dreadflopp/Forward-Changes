using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Spell
{
    public class CastDurationHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "CastDuration";

        public override void SetValue(IMajorRecord record, float value)
        {
            var spellRecord = TryCastRecord<ISpell>(record, PropertyName);
            if (spellRecord != null)
            {
                spellRecord.CastDuration = value;
            }
        }

        public override float GetValue(IMajorRecordGetter record)
        {
            var spellRecord = TryCastRecord<ISpellGetter>(record, PropertyName);
            if (spellRecord != null)
            {
                return spellRecord.CastDuration;
            }
            return 0.0f;
        }
    }
}
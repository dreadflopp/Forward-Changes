using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Spell
{
    public class ChargeTimeHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "ChargeTime";

        public override void SetValue(IMajorRecord record, float value)
        {
            var spellRecord = TryCastRecord<ISpell>(record, PropertyName);
            if (spellRecord != null)
            {
                spellRecord.ChargeTime = value;
            }
        }

        public override float GetValue(IMajorRecordGetter record)
        {
            var spellRecord = TryCastRecord<ISpellGetter>(record, PropertyName);
            if (spellRecord != null)
            {
                return spellRecord.ChargeTime;
            }
            return 0.0f;
        }
    }
}
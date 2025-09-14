using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Spell
{
    public class TargetTypeHandler : AbstractPropertyHandler<TargetType>
    {
        public override string PropertyName => "TargetType";

        public override void SetValue(IMajorRecord record, TargetType value)
        {
            var spellRecord = TryCastRecord<ISpell>(record, PropertyName);
            if (spellRecord != null)
            {
                spellRecord.TargetType = value;
            }
        }

        public override TargetType GetValue(IMajorRecordGetter record)
        {
            var spellRecord = TryCastRecord<ISpellGetter>(record, PropertyName);
            if (spellRecord != null)
            {
                return spellRecord.TargetType;
            }
            return default(TargetType);
        }
    }
}
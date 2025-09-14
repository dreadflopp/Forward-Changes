using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Spell
{
    public class TypeHandler : AbstractPropertyHandler<SpellType>
    {
        public override string PropertyName => "Type";

        public override void SetValue(IMajorRecord record, SpellType value)
        {
            var spellRecord = TryCastRecord<ISpell>(record, PropertyName);
            if (spellRecord != null)
            {
                spellRecord.Type = value;
            }
        }

        public override SpellType GetValue(IMajorRecordGetter record)
        {
            var spellRecord = TryCastRecord<ISpellGetter>(record, PropertyName);
            if (spellRecord != null)
            {
                return spellRecord.Type;
            }
            return default(SpellType);
        }
    }
}
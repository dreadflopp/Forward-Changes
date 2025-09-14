using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Spell
{
    public class CastTypeHandler : AbstractPropertyHandler<CastType>
    {
        public override string PropertyName => "CastType";

        public override void SetValue(IMajorRecord record, CastType value)
        {
            var spellRecord = TryCastRecord<ISpell>(record, PropertyName);
            if (spellRecord != null)
            {
                spellRecord.CastType = value;
            }
        }

        public override CastType GetValue(IMajorRecordGetter record)
        {
            var spellRecord = TryCastRecord<ISpellGetter>(record, PropertyName);
            if (spellRecord != null)
            {
                return spellRecord.CastType;
            }
            return default(CastType);
        }
    }
}
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Spell
{
    public class BaseCostHandler : AbstractPropertyHandler<uint>
    {
        public override string PropertyName => "BaseCost";

        public override void SetValue(IMajorRecord record, uint value)
        {
            var spellRecord = TryCastRecord<ISpell>(record, PropertyName);
            if (spellRecord != null)
            {
                spellRecord.BaseCost = value;
            }
        }

        public override uint GetValue(IMajorRecordGetter record)
        {
            var spellRecord = TryCastRecord<ISpellGetter>(record, PropertyName);
            if (spellRecord != null)
            {
                return spellRecord.BaseCost;
            }
            return 0;
        }
    }
}
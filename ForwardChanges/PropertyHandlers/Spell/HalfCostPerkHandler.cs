using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Spell
{
    public class HalfCostPerkHandler : AbstractPropertyHandler<IFormLinkGetter<IPerkGetter>>
    {
        public override string PropertyName => "HalfCostPerk";

        public override void SetValue(IMajorRecord record, IFormLinkGetter<IPerkGetter>? value)
        {
            var spellRecord = TryCastRecord<ISpell>(record, PropertyName);
            if (spellRecord != null)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    spellRecord.HalfCostPerk = new FormLink<IPerkGetter>(value.FormKey);
                }
                else
                {
                    spellRecord.HalfCostPerk.Clear();
                }
            }
        }

        public override IFormLinkGetter<IPerkGetter>? GetValue(IMajorRecordGetter record)
        {
            var spellRecord = TryCastRecord<ISpellGetter>(record, PropertyName);
            if (spellRecord != null)
            {
                return spellRecord.HalfCostPerk;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkGetter<IPerkGetter>? value1, IFormLinkGetter<IPerkGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}
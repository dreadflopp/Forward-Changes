using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Spell
{
    public class EquipmentTypeHandler : AbstractFormLinkPropertyHandler<ISpell, ISpellGetter, IEquipTypeGetter>
    {
        public override string PropertyName => "EquipmentType";

        protected override IFormLinkNullableGetter<IEquipTypeGetter>? GetFormLinkValue(ISpellGetter record)
        {
            return record.EquipmentType;
        }

        protected override void SetFormLinkValue(ISpell record, IFormLinkNullableGetter<IEquipTypeGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.EquipmentType = new FormLinkNullable<IEquipTypeGetter>(value.FormKey);
            }
            else
            {
                record.EquipmentType.Clear();
            }
        }
    }
}
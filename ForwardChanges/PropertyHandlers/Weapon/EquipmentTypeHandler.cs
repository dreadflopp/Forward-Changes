using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class EquipmentTypeHandler : AbstractFormLinkPropertyHandler<IWeapon, IWeaponGetter, IEquipTypeGetter>
    {
        public override string PropertyName => "EquipmentType";

        protected override IFormLinkNullableGetter<IEquipTypeGetter>? GetFormLinkValue(IWeaponGetter record)
        {
            return record.EquipmentType;
        }

        protected override void SetFormLinkValue(IWeapon record, IFormLinkNullableGetter<IEquipTypeGetter>? value)
        {
            if (value == null)
            {
                record.EquipmentType.Clear();
            }
            else
            {
                record.EquipmentType = new FormLinkNullable<IEquipTypeGetter>(value.FormKey);
            }
        }
    }
}
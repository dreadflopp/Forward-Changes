using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class EquipSoundHandler : AbstractFormLinkPropertyHandler<IWeapon, IWeaponGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "EquipSound";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetFormLinkValue(IWeaponGetter record)
        {
            return record.EquipSound;
        }

        protected override void SetFormLinkValue(IWeapon record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value == null)
            {
                record.EquipSound.Clear();
            }
            else
            {
                record.EquipSound = new FormLinkNullable<ISoundDescriptorGetter>(value.FormKey);
            }
        }
    }
}
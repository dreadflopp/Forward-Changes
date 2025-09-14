using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class UnequipSoundHandler : AbstractFormLinkPropertyHandler<IWeapon, IWeaponGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "UnequipSound";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetFormLinkValue(IWeaponGetter record)
        {
            return record.UnequipSound;
        }

        protected override void SetFormLinkValue(IWeapon record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value == null)
            {
                record.UnequipSound.Clear();
            }
            else
            {
                record.UnequipSound = new FormLinkNullable<ISoundDescriptorGetter>(value.FormKey);
            }
        }
    }
}
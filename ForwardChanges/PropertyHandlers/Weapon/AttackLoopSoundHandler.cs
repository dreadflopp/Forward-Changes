using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class AttackLoopSoundHandler : AbstractFormLinkPropertyHandler<IWeapon, IWeaponGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "AttackLoopSound";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetFormLinkValue(IWeaponGetter record)
        {
            return record.AttackLoopSound;
        }

        protected override void SetFormLinkValue(IWeapon record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value == null)
            {
                record.AttackLoopSound.Clear();
            }
            else
            {
                record.AttackLoopSound = new FormLinkNullable<ISoundDescriptorGetter>(value.FormKey);
            }
        }
    }
}
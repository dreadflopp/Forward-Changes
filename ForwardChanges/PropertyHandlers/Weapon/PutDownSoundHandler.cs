using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class PutDownSoundHandler : AbstractFormLinkPropertyHandler<IWeapon, IWeaponGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "PutDownSound";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetFormLinkValue(IWeaponGetter record)
        {
            return record.PutDownSound;
        }

        protected override void SetFormLinkValue(IWeapon record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value == null)
            {
                record.PutDownSound.Clear();
            }
            else
            {
                record.PutDownSound = new FormLinkNullable<ISoundDescriptorGetter>(value.FormKey);
            }
        }
    }
}
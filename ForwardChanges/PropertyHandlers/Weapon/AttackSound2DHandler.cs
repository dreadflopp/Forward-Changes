using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class AttackSound2DHandler : AbstractFormLinkPropertyHandler<IWeapon, IWeaponGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "AttackSound2D";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetFormLinkValue(IWeaponGetter record)
        {
            return record.AttackSound2D;
        }

        protected override void SetFormLinkValue(IWeapon record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value == null)
            {
                record.AttackSound2D.Clear();
            }
            else
            {
                record.AttackSound2D = new FormLinkNullable<ISoundDescriptorGetter>(value.FormKey);
            }
        }
    }
}
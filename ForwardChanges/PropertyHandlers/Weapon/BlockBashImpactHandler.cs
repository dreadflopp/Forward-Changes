using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class BlockBashImpactHandler : AbstractFormLinkPropertyHandler<IWeapon, IWeaponGetter, IImpactDataSetGetter>
    {
        public override string PropertyName => "BlockBashImpact";

        protected override IFormLinkNullableGetter<IImpactDataSetGetter>? GetFormLinkValue(IWeaponGetter record)
        {
            return record.BlockBashImpact;
        }

        protected override void SetFormLinkValue(IWeapon record, IFormLinkNullableGetter<IImpactDataSetGetter>? value)
        {
            if (value == null)
            {
                record.BlockBashImpact.Clear();
            }
            else
            {
                record.BlockBashImpact = new FormLinkNullable<IImpactDataSetGetter>(value.FormKey);
            }
        }
    }
}
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class ImpactDataSetHandler : AbstractFormLinkPropertyHandler<IWeapon, IWeaponGetter, IImpactDataSetGetter>
    {
        public override string PropertyName => "ImpactDataSet";

        protected override IFormLinkNullableGetter<IImpactDataSetGetter>? GetFormLinkValue(IWeaponGetter record)
        {
            return record.ImpactDataSet;
        }

        protected override void SetFormLinkValue(IWeapon record, IFormLinkNullableGetter<IImpactDataSetGetter>? value)
        {
            if (value == null)
            {
                record.ImpactDataSet.Clear();
            }
            else
            {
                record.ImpactDataSet = new FormLinkNullable<IImpactDataSetGetter>(value.FormKey);
            }
        }
    }
}
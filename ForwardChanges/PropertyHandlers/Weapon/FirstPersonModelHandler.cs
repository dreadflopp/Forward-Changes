using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class FirstPersonModelHandler : AbstractFormLinkPropertyHandler<IWeapon, IWeaponGetter, IStaticGetter>
    {
        public override string PropertyName => "FirstPersonModel";

        protected override IFormLinkNullableGetter<IStaticGetter>? GetFormLinkValue(IWeaponGetter record)
        {
            return record.FirstPersonModel;
        }

        protected override void SetFormLinkValue(IWeapon record, IFormLinkNullableGetter<IStaticGetter>? value)
        {
            if (value == null)
            {
                record.FirstPersonModel.Clear();
            }
            else
            {
                record.FirstPersonModel = new FormLinkNullable<IStaticGetter>(value.FormKey);
            }
        }
    }
}
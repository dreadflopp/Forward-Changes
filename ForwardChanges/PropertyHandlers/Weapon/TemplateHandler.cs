using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class TemplateHandler : AbstractFormLinkPropertyHandler<IWeapon, IWeaponGetter, IWeaponGetter>
    {
        public override string PropertyName => "Template";

        protected override IFormLinkNullableGetter<IWeaponGetter>? GetFormLinkValue(IWeaponGetter record)
        {
            return record.Template;
        }

        protected override void SetFormLinkValue(IWeapon record, IFormLinkNullableGetter<IWeaponGetter>? value)
        {
            if (value == null)
            {
                record.Template.Clear();
            }
            else
            {
                record.Template = new FormLinkNullable<IWeaponGetter>(value.FormKey);
            }
        }
    }
}
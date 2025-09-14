using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class AlternateBlockMaterialHandler : AbstractFormLinkPropertyHandler<IWeapon, IWeaponGetter, IMaterialTypeGetter>
    {
        public override string PropertyName => "AlternateBlockMaterial";

        protected override IFormLinkNullableGetter<IMaterialTypeGetter>? GetFormLinkValue(IWeaponGetter record)
        {
            return record.AlternateBlockMaterial;
        }

        protected override void SetFormLinkValue(IWeapon record, IFormLinkNullableGetter<IMaterialTypeGetter>? value)
        {
            if (value == null)
            {
                record.AlternateBlockMaterial.Clear();
            }
            else
            {
                record.AlternateBlockMaterial = new FormLinkNullable<IMaterialTypeGetter>(value.FormKey);
            }
        }
    }
}
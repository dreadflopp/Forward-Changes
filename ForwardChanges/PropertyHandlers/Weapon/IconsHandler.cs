using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class IconsHandler : AbstractIconsHandler<IWeaponGetter, IWeapon>
    {
        protected override IIconsGetter? GetIcons(IWeaponGetter record)
        {
            return record.Icons;
        }

        protected override void SetIcons(IWeapon record, Icons? value)
        {
            record.Icons = value;
        }
    }
}
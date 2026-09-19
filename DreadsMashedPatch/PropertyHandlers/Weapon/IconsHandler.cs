using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.Weapon
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
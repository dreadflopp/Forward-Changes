using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ammunition
{
    public class IconsHandler : AbstractIconsHandler<IAmmunitionGetter, IAmmunition>
    {
        protected override IIconsGetter? GetIcons(IAmmunitionGetter record)
        {
            return record.Icons;
        }

        protected override void SetIcons(IAmmunition record, Icons? value)
        {
            record.Icons = value;
        }
    }
}

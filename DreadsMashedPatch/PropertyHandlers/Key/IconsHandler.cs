using Mutagen.Bethesda.Skyrim;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.Key
{
    public class IconsHandler : AbstractIconsHandler<IKeyGetter, IKey>
    {
        protected override IIconsGetter? GetIcons(IKeyGetter record)
        {
            return record.Icons;
        }

        protected override void SetIcons(IKey record, Icons? value)
        {
            record.Icons = value;
        }
    }
}

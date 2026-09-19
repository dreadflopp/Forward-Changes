using Mutagen.Bethesda.Skyrim;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.AlchemicalApparatus
{
    public class IconsHandler : AbstractIconsHandler<IAlchemicalApparatusGetter, IAlchemicalApparatus>
    {
        protected override IIconsGetter? GetIcons(IAlchemicalApparatusGetter record)
        {
            return record.Icons;
        }

        protected override void SetIcons(IAlchemicalApparatus record, Icons? value)
        {
            record.Icons = value;
        }
    }
}

using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.AlchemicalApparatus
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

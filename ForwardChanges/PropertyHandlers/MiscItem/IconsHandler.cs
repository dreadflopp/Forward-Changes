using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.MiscItem
{
    public class IconsHandler : AbstractIconsHandler<IMiscItemGetter, IMiscItem>
    {
        protected override IIconsGetter? GetIcons(IMiscItemGetter record)
        {
            return record.Icons;
        }

        protected override void SetIcons(IMiscItem record, Icons? value)
        {
            record.Icons = value;
        }
    }
}

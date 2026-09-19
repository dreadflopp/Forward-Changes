using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Book
{
    public class IconsHandler : AbstractIconsHandler<IBookGetter, IBook>
    {
        protected override IIconsGetter? GetIcons(IBookGetter record)
        {
            return record.Icons;
        }

        protected override void SetIcons(IBook record, Icons? value)
        {
            record.Icons = value;
        }
    }
}
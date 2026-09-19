using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.Light
{
    public class IconsHandler : AbstractIconsHandler<ILightGetter, Mutagen.Bethesda.Skyrim.Light>
    {
        protected override IIconsGetter? GetIcons(ILightGetter record)
        {
            return record.Icons;
        }

        protected override void SetIcons(Mutagen.Bethesda.Skyrim.Light record, Icons? value)
        {
            record.Icons = value;
        }
    }
}


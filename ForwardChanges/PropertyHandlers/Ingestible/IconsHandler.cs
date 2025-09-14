using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingestible
{
    public class IconsHandler : AbstractIconsHandler<IIngestibleGetter, IIngestible>
    {
        protected override IIconsGetter? GetIcons(IIngestibleGetter record)
        {
            return record.Icons;
        }

        protected override void SetIcons(IIngestible record, Icons? value)
        {
            record.Icons = value;
        }
    }
}
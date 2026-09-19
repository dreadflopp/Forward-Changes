using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingestible
{
    public class DestructibleHandler : AbstractDestructibleHandler<IIngestibleGetter, IIngestible>
    {
        protected override IDestructibleGetter? GetDestructible(IIngestibleGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(IIngestible record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}
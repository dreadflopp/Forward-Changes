using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.MiscItem
{
    public class DestructibleHandler : AbstractDestructibleHandler<IMiscItemGetter, IMiscItem>
    {
        protected override IDestructibleGetter? GetDestructible(IMiscItemGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(IMiscItem record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}

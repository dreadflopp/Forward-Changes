using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.Npc
{
    public class DestructibleHandler : AbstractDestructibleHandler<INpcGetter, INpc>
    {
        protected override IDestructibleGetter? GetDestructible(INpcGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(INpc record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}
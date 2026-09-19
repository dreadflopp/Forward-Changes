using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.Light
{
    public class DestructibleHandler : AbstractDestructibleHandler<ILightGetter, Mutagen.Bethesda.Skyrim.Light>
    {
        protected override IDestructibleGetter? GetDestructible(ILightGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(Mutagen.Bethesda.Skyrim.Light record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}


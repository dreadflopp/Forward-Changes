using Mutagen.Bethesda.Skyrim;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.Ammunition
{
    public class DestructibleHandler : AbstractDestructibleHandler<IAmmunitionGetter, IAmmunition>
    {
        protected override IDestructibleGetter? GetDestructible(IAmmunitionGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(IAmmunition record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}

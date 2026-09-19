using Mutagen.Bethesda.Skyrim;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.AlchemicalApparatus
{
    public class DestructibleHandler : AbstractDestructibleHandler<IAlchemicalApparatusGetter, IAlchemicalApparatus>
    {
        protected override IDestructibleGetter? GetDestructible(IAlchemicalApparatusGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(IAlchemicalApparatus record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}

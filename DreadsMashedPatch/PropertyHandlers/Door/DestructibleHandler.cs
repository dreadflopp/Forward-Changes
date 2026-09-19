using Mutagen.Bethesda.Skyrim;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.Door
{
    public class DestructibleHandler : AbstractDestructibleHandler<IDoorGetter, IDoor>
    {
        protected override IDestructibleGetter? GetDestructible(IDoorGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(IDoor record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}

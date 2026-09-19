using Mutagen.Bethesda.Skyrim;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.Container
{
    public class DestructibleHandler : AbstractDestructibleHandler<IContainerGetter, IContainer>
    {
        protected override IDestructibleGetter? GetDestructible(IContainerGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(IContainer record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}
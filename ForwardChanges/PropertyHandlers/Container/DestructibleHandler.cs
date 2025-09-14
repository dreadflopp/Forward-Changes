using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Container
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
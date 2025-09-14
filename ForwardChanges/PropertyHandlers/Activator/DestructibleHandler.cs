using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Activator
{
    public class DestructibleHandler : AbstractDestructibleHandler<IActivatorGetter, IActivator>
    {
        protected override IDestructibleGetter? GetDestructible(IActivatorGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(IActivator record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}

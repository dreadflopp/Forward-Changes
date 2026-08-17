using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Key
{
    public class DestructibleHandler : AbstractDestructibleHandler<IKeyGetter, IKey>
    {
        protected override IDestructibleGetter? GetDestructible(IKeyGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(IKey record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}

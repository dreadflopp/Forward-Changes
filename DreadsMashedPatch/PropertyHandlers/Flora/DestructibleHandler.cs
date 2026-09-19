using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Flora
{
    public class DestructibleHandler : AbstractDestructibleHandler<IFloraGetter, IFlora>
    {
        protected override IDestructibleGetter? GetDestructible(IFloraGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(IFlora record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}

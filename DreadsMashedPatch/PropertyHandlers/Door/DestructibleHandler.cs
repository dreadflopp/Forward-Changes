using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Door
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

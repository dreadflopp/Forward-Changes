using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.AlchemicalApparatus
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

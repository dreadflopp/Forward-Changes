using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Armor
{
    public class DestructibleHandler : AbstractDestructibleHandler<IArmorGetter, IArmor>
    {
        protected override IDestructibleGetter? GetDestructible(IArmorGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(IArmor record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}

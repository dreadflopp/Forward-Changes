using Mutagen.Bethesda.Skyrim;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.Armor
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

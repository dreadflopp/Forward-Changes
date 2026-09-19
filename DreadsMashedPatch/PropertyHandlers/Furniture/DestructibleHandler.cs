using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Furniture
{
    public class DestructibleHandler : AbstractDestructibleHandler<IFurnitureGetter, IFurniture>
    {
        protected override IDestructibleGetter? GetDestructible(IFurnitureGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(IFurniture record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}

using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Book
{
    public class DestructibleHandler : AbstractDestructibleHandler<IBookGetter, IBook>
    {
        protected override IDestructibleGetter? GetDestructible(IBookGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(IBook record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}
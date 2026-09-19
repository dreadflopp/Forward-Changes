using Mutagen.Bethesda.Skyrim;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.Book
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
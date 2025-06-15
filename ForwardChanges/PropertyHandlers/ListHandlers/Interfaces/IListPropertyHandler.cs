using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyStates;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ListHandlers.Interfaces
{
    public interface IListPropertyHandler<TRecord, TItem>
        where TRecord : class, IMajorRecordGetter
        where TItem : class
    {
        /// <summary>
        /// Gets the current state of the list property from a record context
        /// </summary>
        object? GetItemStateCollection(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context);

        void SortItemsToMatchContextOrder(object contextItems, PropertyState propertyState);

        /*
                void AddItem(IMajorRecord record, TValue item);
                void RemoveItem(IMajorRecord record, TValue item);
                void ClearItems(IMajorRecord record);
        */
    }
}
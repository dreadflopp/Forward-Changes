using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyStates
{
    /// <summary>
    /// Represents a collection of items in a list property.
    /// </summary>
    /// <typeparam name="TItem">The type of item in the collection.</typeparam>
    public class ItemStateCollection<TItem>
    {
        public List<ItemState<TItem>> Items { get; set; } = [];

        public override string ToString()
        {
            if (Items.Count == 0)
                return "No items";

            return string.Join(", ", Items.Where(i => !i.IsRemoved).Select(i =>
            {
                if (i.Item is IRankPlacementGetter rankPlacement)
                {
                    return $"{rankPlacement.Faction.FormKey}(Rank {rankPlacement.Rank})";
                }
                return i.Item?.ToString() ?? "null";
            }));
        }
    }
}
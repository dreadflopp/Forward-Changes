using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.ListHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System.Collections.Generic;

namespace ForwardChanges.PropertyHandlers.ListHandlers
{
    public class FactionListHandler : AbstractListPropertyHandler<INpcGetter, IRankPlacementGetter>, IPropertyHandler<object>
    {
        public override string PropertyName => "Factions";

        public FactionListHandler()
            : base(
                npc => npc.Factions.Select(f => new ListItemContext<IRankPlacementGetter>(f, npc.FormKey.ModKey.ToString())).ToList())
        {
        }

        /// <summary>
        /// Formats a list of factions into a string.
        /// </summary>
        /// <param name="factions">The list of factions to format.</param>
        /// <returns>A string representation of the factions.</returns>
        public string FormatFactionList(IReadOnlyList<IRankPlacementGetter>? factions)
        {
            if (factions == null || factions.Count == 0)
                return "No factions";

            return string.Join(", ", factions.Select(f => FormatItem(f)));
        }

        /// <summary>
        /// Checks if two factions are equal.
        /// </summary>
        /// <param name="item1">The first faction to compare.</param>
        /// <param name="item2">The second faction to compare.</param>
        /// <returns>True if the factions are equal, false otherwise.</returns>
        protected override bool IsItemEqual(IRankPlacementGetter? item1, IRankPlacementGetter? item2)
        {
            if (ReferenceEquals(item1, item2)) return true;
            if (item1 is null || item2 is null) return false;
            return IsFactionReferenceEqual(item1, item2) && IsRankEqual(item1, item2);
        }

        /// <summary>
        /// Checks if two factions are equal by their reference.
        /// </summary>
        /// <param name="item1">The first faction to compare.</param>
        /// <param name="item2">The second faction to compare.</param>
        /// <returns>True if the factions are equal, false otherwise.</returns>
        protected bool IsFactionReferenceEqual(IRankPlacementGetter? item1, IRankPlacementGetter? item2)
        {
            if (ReferenceEquals(item1, item2)) return true;
            if (item1 is null || item2 is null) return false;
            return item1.Faction.FormKey.Equals(item2.Faction.FormKey);
        }

        /// <summary>
        /// Checks if two factions are equal by their rank.
        /// </summary>
        /// <param name="item1">The first faction to compare.</param>
        /// <param name="item2">The second faction to compare.</param>
        /// <returns>True if the factions are equal, false otherwise.</returns>
        protected bool IsRankEqual(IRankPlacementGetter? item1, IRankPlacementGetter? item2)
        {
            if (ReferenceEquals(item1, item2)) return true;
            if (item1 is null || item2 is null) return false;
            return item1.Rank == item2.Rank;
        }

        /// <summary>
        /// Formats a faction into a string.
        /// </summary>
        /// <param name="item">The faction to format.</param>
        /// <returns>A string representation of the faction.</returns>
        protected override string FormatItem(IRankPlacementGetter item)
        {
            return $"{item.Faction.FormKey}(Rank {item.Rank})";
        }

        /// <summary>
        /// Sets the factions of an NPC.
        /// </summary>
        /// <param name="record">The NPC to set the factions of.</param>
        /// <param name="value">The factions to set.</param>
        public override void SetValue(IMajorRecord record, List<ListItemContext<IRankPlacementGetter>>? value)
        {
            if (record is not INpc npc)
                return;

            // Clear existing factions and add new ones
            var oldFactions = npc.Factions;
            npc.Factions.Clear();

            if (value != null)
            {
                foreach (var faction in value.Where(i => !i.IsRemoved))
                {
                    var rankPlacement = new RankPlacement
                    {
                        Faction = new FormLink<IFactionGetter>(faction.Item.Faction.FormKey),
                        Rank = faction.Item.Rank
                    };
                    npc.Factions.Add(rankPlacement);
                }
            }
            Console.WriteLine($"Forwarded factions: {this.FormatFactionList(oldFactions)} -> {this.FormatFactionList(npc.Factions)}");
        }

        /// <summary>
        /// Checks if two item state collections of factions are equal.
        /// </summary>
        /// <param name="value1">The first list of factions to compare.</param>
        /// <param name="value2">The second list of factions to compare.</param>
        /// <returns>True if the lists are equal, false otherwise.</returns>
        public override bool AreValuesEqual(List<ListItemContext<IRankPlacementGetter>>? value1, List<ListItemContext<IRankPlacementGetter>>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            var items1 = value1.Where(i => !i.IsRemoved).Select(i => i.Item).ToList();
            var items2 = value2.Where(i => !i.IsRemoved).Select(i => i.Item).ToList();

            if (items1.Count != items2.Count) return false;

            // Check if every item in the first list exists in the second list
            return items1.All(item1 => items2.Any(item2 => IsItemEqual(item1, item2)));
        }

        public override void UpdatePropertyContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            PropertyContext propertyContext)
        {
            if (context.Record is not INpcGetter record)
                return;

            var recordItems = ListAccessor(record).ToList();
            var currentFinalItems = (List<ListItemContext<IRankPlacementGetter>>)propertyContext.ForwardValue!;
            var recordMod = state.LoadOrder[context.ModKey].Mod;

            // Process removals
            foreach (var item in currentFinalItems.Where(i => !i.IsRemoved).ToList())
            {
                var matchingItemInRecord = recordItems.FirstOrDefault(c => IsItemEqual(c.Item, item.Item));

                if (matchingItemInRecord == null)
                {
                    // Item is being removed
                    var canModify = recordMod?.MasterReferences.Any(m => m.Master.ToString() == item.OwnerMod) == true;

                    if (canModify)
                    {
                        var oldOwner = item.OwnerMod;
                        item.IsRemoved = true;
                        item.OwnerMod = context.ModKey.ToString();
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Removing item {FormatItem(item.Item)} (was owned by {oldOwner}) Success");
                    }
                    else
                    {
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Removing item {FormatItem(item.Item)} (was owned by {item.OwnerMod}) Permission denied");
                    }
                }
            }

            // Process additions            
            foreach (var item in recordItems)
            {
                // Check if this faction exists in the final items with a different rank
                var matchingItemInFinalItems = currentFinalItems.FirstOrDefault(e =>
                    !e.IsRemoved && IsFactionReferenceEqual(e.Item, item.Item));

                if (matchingItemInFinalItems == null)
                {
                    // Check if this specific item was previously removed
                    var previouslyRemovedItem = currentFinalItems.FirstOrDefault(e =>
                        e.IsRemoved && IsFactionReferenceEqual(e.Item, item.Item));

                    if (previouslyRemovedItem == null)
                    {
                        // Add as new item
                        var newItem = new ListItemContext<IRankPlacementGetter>(item.Item, context.ModKey.ToString());
                        InsertItemAtCorrectPosition(newItem, recordItems.Select(i => i.Item).ToList(), currentFinalItems);
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding new faction {FormatItem(item.Item)} Success");
                    }
                    else
                    {
                        // Check if we can add it back
                        var canAddBack = recordMod?.MasterReferences.Any(m => m.Master.ToString() == previouslyRemovedItem.OwnerMod) == true;

                        if (canAddBack)
                        {
                            // Update the removed item's state and re-insert at correct position
                            previouslyRemovedItem.IsRemoved = false;
                            previouslyRemovedItem.OwnerMod = context.ModKey.ToString();
                            currentFinalItems.Remove(previouslyRemovedItem);
                            InsertItemAtCorrectPosition(previouslyRemovedItem, recordItems.Select(i => i.Item).ToList(), currentFinalItems);
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding back faction {FormatItem(item.Item)} Success");
                        }
                        else
                        {
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding new faction {FormatItem(item.Item)} Permission denied. Previously removed by {previouslyRemovedItem.OwnerMod}");
                        }
                    }
                }
                else
                {
                    // Faction exists with different rank - check if we can modify it
                    var canModify = recordMod?.MasterReferences.Any(m => m.Master.ToString() == matchingItemInFinalItems.OwnerMod) == true;

                    if (canModify)
                    {
                        // Replace the existing faction
                        var oldOwner = matchingItemInFinalItems.OwnerMod;
                        matchingItemInFinalItems.IsRemoved = true;
                        matchingItemInFinalItems.OwnerMod = context.ModKey.ToString();
                        var newItem = new ListItemContext<IRankPlacementGetter>(item.Item, context.ModKey.ToString());
                        InsertItemAtCorrectPosition(newItem, recordItems.Select(i => i.Item).ToList(), currentFinalItems);
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Replacing faction {FormatItem(matchingItemInFinalItems.Item)} with {FormatItem(item.Item)} (was owned by {oldOwner})");
                    }
                    else
                    {
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Cannot modify faction {FormatItem(item.Item)} (owned by {matchingItemInFinalItems.OwnerMod}) Permission denied");
                    }
                }
            }

            // Update the state
            propertyContext.ForwardValue.Item = currentFinalItems;
        }

        // IPropertyHandler<object> implementation
        void IPropertyHandler<object>.SetValue(IMajorRecord record, object? value) => SetValue(record, (List<ListItemContext<IRankPlacementGetter>>?)value);
        object? IPropertyHandler<object>.GetValue(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context) => GetValue(context);
        bool IPropertyHandler<object>.AreValuesEqual(object? value1, object? value2) => AreValuesEqual((List<ListItemContext<IRankPlacementGetter>>?)value1, (List<ListItemContext<IRankPlacementGetter>>?)value2);
    }
}
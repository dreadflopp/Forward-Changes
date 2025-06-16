using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyStates;
using ForwardChanges.PropertyHandlers.ListHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System.Collections.Generic;

namespace ForwardChanges.PropertyHandlers.ListHandlers
{
    public class FactionListHandler : AbstractListPropertyHandler<INpcGetter, IRankPlacementGetter>, IPropertyHandler
    {
        public override string PropertyName => "Factions";

        public FactionListHandler()
            : base(
                "Factions",
                npc => npc.Factions)
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

        public override void SetValue(IMajorRecord record, object? value)
        {
            if (record is not INpc npc || value is not ItemStateCollection<IRankPlacementGetter> listState)
                return;

            // Clear existing factions and add new ones
            var oldFactions = npc.Factions;
            npc.Factions.Clear();
            foreach (var item in listState.Items.Where(i => !i.IsRemoved))
            {
                var rankPlacement = new RankPlacement
                {
                    Faction = new FormLink<IFactionGetter>(item.Item.Faction.FormKey),
                    Rank = item.Item.Rank
                };
                npc.Factions.Add(rankPlacement);
            }
            Console.WriteLine($"Forwarded factions: {this.FormatFactionList(oldFactions)} -> {this.FormatFactionList(npc.Factions)}");
        }

        public override bool AreValuesEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1 is ItemStateCollection<IRankPlacementGetter> list1 &&
                value2 is ItemStateCollection<IRankPlacementGetter> list2)
            {
                var items1 = list1.Items.Select(i => i.Item).ToList();
                var items2 = list2.Items.Select(i => i.Item).ToList();

                if (items1.Count != items2.Count) return false;

                for (int i = 0; i < items1.Count; i++)
                {
                    if (!IsItemEqual(items1[i], items2[i])) return false;
                }
                return true;
            }
            return false;
        }

        public override void UpdatePropertyState(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            PropertyState propertyState)
        {
            if (context.Record is not INpcGetter record)
                return;

            var recordItems = ListAccessor(record).ToList();
            var currentFinalItems = (ItemStateCollection<IRankPlacementGetter>)propertyState.FinalValue!;
            var recordMod = state.LoadOrder[context.ModKey].Mod;

            // Process removals
            foreach (var item in currentFinalItems.Items.Where(i => !i.IsRemoved).ToList())
            {
                var matchingItemInRecord = recordItems.FirstOrDefault(c => IsItemEqual(c, item.Item));

                if (matchingItemInRecord == null)
                {
                    // Item is being removed
                    var canModify = recordMod?.MasterReferences.Any(m => m.Master.ToString() == item.OwnerMod) == true;

                    if (canModify)
                    {
                        var oldOwner = item.OwnerMod;
                        item.IsRemoved = true;
                        item.OwnerMod = context.ModKey.ToString();
                        LogCollector.Add(_propertyName, $"[{_propertyName}] {context.ModKey}: Removing item {FormatItem(item.Item)} (was owned by {oldOwner}) Success");
                    }
                    else
                    {
                        LogCollector.Add(_propertyName, $"[{_propertyName}] {context.ModKey}: Removing item {FormatItem(item.Item)} (was owned by {item.OwnerMod}) Permission denied");
                    }
                }
            }

            // Process additions            
            foreach (var item in recordItems)
            {
                // Check if this faction exists in the final items with a different rank
                var matchingItemInFinalItems = currentFinalItems.Items.FirstOrDefault(e =>
                    !e.IsRemoved && IsFactionReferenceEqual(e.Item, item));

                if (matchingItemInFinalItems == null)
                {
                    // Check if this specific item was previously removed
                    var previouslyRemovedItem = currentFinalItems.Items.FirstOrDefault(e =>
                        e.IsRemoved && IsFactionReferenceEqual(e.Item, item));

                    if (previouslyRemovedItem == null)
                    {
                        // Add as new item
                        var newItem = new ItemState<IRankPlacementGetter>(item, context.ModKey.ToString());
                        InsertItemAtCorrectPosition(newItem, recordItems, currentFinalItems);
                        LogCollector.Add(_propertyName, $"[{_propertyName}] {context.ModKey}: Adding new faction {FormatItem(item)} Success");
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
                            currentFinalItems.Items.Remove(previouslyRemovedItem);
                            InsertItemAtCorrectPosition(previouslyRemovedItem, recordItems, currentFinalItems);
                            LogCollector.Add(_propertyName, $"[{_propertyName}] {context.ModKey}: Adding back faction {FormatItem(item)} Success");
                        }
                        else
                        {
                            LogCollector.Add(_propertyName, $"[{_propertyName}] {context.ModKey}: Adding new faction {FormatItem(item)} Permission denied. Previously removed by {previouslyRemovedItem.OwnerMod}");
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
                        var newItem = new ItemState<IRankPlacementGetter>(item, context.ModKey.ToString());
                        InsertItemAtCorrectPosition(newItem, recordItems, currentFinalItems);
                        LogCollector.Add(_propertyName, $"[{_propertyName}] {context.ModKey}: Replacing faction {FormatItem(matchingItemInFinalItems.Item)} with {FormatItem(item)} (was owned by {oldOwner})");
                    }
                    else
                    {
                        LogCollector.Add(_propertyName, $"[{_propertyName}] {context.ModKey}: Cannot modify faction {FormatItem(item)} (owned by {matchingItemInFinalItems.OwnerMod}) Permission denied");
                    }
                }
            }

            // Update the state
            propertyState.FinalValue = currentFinalItems;
        }
    }
}
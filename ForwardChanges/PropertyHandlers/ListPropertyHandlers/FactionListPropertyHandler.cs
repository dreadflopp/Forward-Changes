using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class FactionListPropertyHandler : AbstractListPropertyHandler<IRankPlacementGetter>
    {
        public override string PropertyName => "Factions";

        public FactionListPropertyHandler()
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
            /*
            if (ReferenceEquals(item1, item2)) return true;
            if (item1 is null || item2 is null) return false;
            return IsFactionReferenceEqual(item1, item2) && IsRankEqual(item1, item2);
            */
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
        protected override string FormatItem(IRankPlacementGetter? item)
        {
            if (item == null) return "null";
            return $"{item.Faction.FormKey}(Rank {item.Rank})";
        }

        /// <summary>
        /// Sets the factions of an NPC.
        /// </summary>
        /// <param name="record">The NPC to set the factions of.</param>
        /// <param name="value">The factions to set.</param>
        public override void SetValue(IMajorRecord record, IReadOnlyList<IRankPlacementGetter>? value)
        {
            if (record is INpc npc)
            {
                npc.Factions.Clear();
                if (value != null)
                {
                    foreach (var item in value)
                    {
                        var rankPlacement = new RankPlacement
                        {
                            Faction = new FormLink<IFactionGetter>(item.Faction.FormKey),
                            Rank = item.Rank
                        };
                        npc.Factions.Add(rankPlacement);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the factions of an NPC.
        /// </summary>
        /// <param name="record">The context of the NPC.</param>
        /// <returns>The factions of the NPC.</returns>
        public override IReadOnlyList<IRankPlacementGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
            {
                return npc.Factions.ToList();
            }
            Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            return null;
        }

        protected override void ProcessHandlerSpecificLogic(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            ListPropertyContext<IRankPlacementGetter> listPropertyContext,
            IReadOnlyList<IRankPlacementGetter> recordItems,
            List<ListPropertyValueContext<IRankPlacementGetter>> currentForwardItems)
        {
            var recordMod = state.LoadOrder[context.ModKey].Mod;
            if (recordMod == null) return;

            // Check each current faction for rank changes
            foreach (var forwardItem in currentForwardItems.Where(i => !i.IsRemoved))
            {
                var matchingRecordItem = recordItems.FirstOrDefault(r => IsFactionReferenceEqual(r, forwardItem.Value));
                if (matchingRecordItem != null && !IsRankEqual(matchingRecordItem, forwardItem.Value))
                {
                    // Rank has changed, check if we can update it
                    var canModify = recordMod.MasterReferences.Any(m => m.Master.ToString() == forwardItem.OwnerMod);
                    if (canModify)
                    {
                        // Create new item with updated rank
                        var newItem = new ListPropertyValueContext<IRankPlacementGetter>(matchingRecordItem, context.ModKey.ToString());
                        // Preserve ordering information
                        newItem.ItemsBefore.AddRange(forwardItem.ItemsBefore);
                        newItem.ItemsAfter.AddRange(forwardItem.ItemsAfter);
                        // Replace old item with new one
                        var index = currentForwardItems.IndexOf(forwardItem);
                        currentForwardItems[index] = newItem;
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Updating rank for faction {FormatItem(matchingRecordItem)} Success");
                    }
                    else
                    {
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Updating rank for faction {FormatItem(matchingRecordItem)} Permission denied. Owned by {forwardItem.OwnerMod}");
                    }
                }
            }
        }
    }
}
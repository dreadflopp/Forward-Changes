using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class PerksHandler : AbstractListPropertyHandler<IPerkPlacementGetter>
    {
        public override string PropertyName => "Perks";

        public PerksHandler()
        {
        }

        /// <summary>
        /// Formats a list of perks into a string.
        /// </summary>
        /// <param name="perks">The list of perks to format.</param>
        /// <returns>A string representation of the perks.</returns>
        public string FormatPerkList(List<IPerkPlacementGetter>? perks)
        {
            if (perks == null || perks.Count == 0)
                return "No perks";

            return string.Join(", ", perks.Select(p => FormatItem(p)));
        }

        /// <summary>
        /// Checks if two perks are equal.
        /// </summary>
        /// <param name="item1">The first perk to compare.</param>
        /// <param name="item2">The second perk to compare.</param>
        /// <returns>True if the perks are equal, false otherwise.</returns>
        protected override bool IsItemEqual(IPerkPlacementGetter? item1, IPerkPlacementGetter? item2)
        {
            // Only check perk reference equality, not rank
            // This prevents duplicate perks with different ranks
            return IsPerkReferenceEqual(item1, item2);
        }

        /// <summary>
        /// Checks if two perks are equal by their reference.
        /// </summary>
        /// <param name="item1">The first perk to compare.</param>
        /// <param name="item2">The second perk to compare.</param>
        /// <returns>True if the perks are equal, false otherwise.</returns>
        protected bool IsPerkReferenceEqual(IPerkPlacementGetter? item1, IPerkPlacementGetter? item2)
        {
            if (ReferenceEquals(item1, item2)) return true;
            if (item1 is null || item2 is null) return false;
            return item1.Perk.FormKey.Equals(item2.Perk.FormKey);
        }

        /// <summary>
        /// Checks if two perks are equal by their rank.
        /// </summary>
        /// <param name="item1">The first perk to compare.</param>
        /// <param name="item2">The second perk to compare.</param>
        /// <returns>True if the perks are equal, false otherwise.</returns>
        protected bool IsRankEqual(IPerkPlacementGetter? item1, IPerkPlacementGetter? item2)
        {
            if (ReferenceEquals(item1, item2)) return true;
            if (item1 is null || item2 is null) return false;
            return item1.Rank == item2.Rank;
        }

        /// <summary>
        /// Formats a perk into a string.
        /// </summary>
        /// <param name="item">The perk to format.</param>
        /// <returns>A string representation of the perk.</returns>
        protected override string FormatItem(IPerkPlacementGetter? item)
        {
            if (item == null) return "null";
            return $"{item.Perk.FormKey}(Rank {item.Rank})";
        }

        /// <summary>
        /// Sets the perks of an NPC.
        /// </summary>
        /// <param name="record">The NPC to set the perks of.</param>
        /// <param name="value">The perks to set.</param>
        public override void SetValue(IMajorRecord record, List<IPerkPlacementGetter>? value)
        {
            if (record is INpc npc)
            {
                npc.Perks?.Clear();
                if (value != null)
                {
                    foreach (var item in value)
                    {
                        var perkPlacement = new PerkPlacement
                        {
                            Perk = new FormLink<IPerkGetter>(item.Perk.FormKey),
                            Rank = item.Rank
                        };
                        npc.Perks?.Add(perkPlacement);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the perks of an NPC.
        /// </summary>
        /// <param name="record">The context of the NPC.</param>
        /// <returns>The perks of the NPC.</returns>
        public override List<IPerkPlacementGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
            {
                return npc.Perks?.ToList();
            }
            Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            return null;
        }

        protected override void ProcessHandlerSpecificLogic(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            ListPropertyContext<IPerkPlacementGetter> listPropertyContext,
            List<IPerkPlacementGetter> recordItems,
            List<ListPropertyValueContext<IPerkPlacementGetter>> currentForwardItems)
        {

            var recordMod = state.LoadOrder[context.ModKey].Mod;
            if (recordMod == null) return;

            // Collect items to modify to avoid collection modification during enumeration
            var itemsToModify = new List<(ListPropertyValueContext<IPerkPlacementGetter> oldItem, ListPropertyValueContext<IPerkPlacementGetter> newItem)>();

            // Check each current perk for rank changes
            foreach (var forwardItem in currentForwardItems.Where(i => !i.IsRemoved))
            {
                var matchingRecordItem = recordItems.FirstOrDefault(r => IsPerkReferenceEqual(r, forwardItem.Value));
                if (matchingRecordItem != null && !IsRankEqual(matchingRecordItem, forwardItem.Value))
                {
                    // Rank has changed, check if we can update it
                    var canModify = recordMod.MasterReferences.Any(m => m.Master.ToString() == forwardItem.OwnerMod);
                    if (canModify)
                    {
                        // Create new item with updated rank
                        var newItem = new ListPropertyValueContext<IPerkPlacementGetter>(matchingRecordItem!, context.ModKey.ToString());
                        
                        // Collect for later modification
                        itemsToModify.Add((forwardItem, newItem));
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Updating rank for perk {FormatItem(matchingRecordItem)} (new owner: {newItem.OwnerMod}) Success");
                    }
                    else
                    {
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Updating rank for perk {FormatItem(matchingRecordItem)} Permission denied. Owned by {forwardItem.OwnerMod}");
                    }
                }
            }

            // Apply modifications after iteration is complete
            foreach (var (oldItem, newItem) in itemsToModify)
            {
                var index = currentForwardItems.IndexOf(oldItem);
                if (index != -1)
                {
                    currentForwardItems[index] = newItem;
                }
            }

        }
    }
}
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyStates;
using ForwardChanges.PropertyHandlers.ListHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ListHandlers
{
    public class FactionListHandler : AbstractListPropertyHandler<INpcGetter, IRankPlacementGetter>, IPropertyHandler
    {
        public override string PropertyName => "Factions";

        public FactionListHandler()
            : base(
                "Factions",
                npc => npc.Factions,
                new FactionComparer())
        {
        }

        public static string FormatFactionList(IReadOnlyList<IRankPlacementGetter>? factions)
        {
            if (factions == null || factions.Count == 0)
                return "No factions";

            return string.Join(", ", factions.Select(f => $"{f.Faction.FormKey}(Rank {f.Rank})"));
        }

        private class FactionComparer : IEqualityComparer<IRankPlacementGetter>
        {
            public bool Equals(IRankPlacementGetter? x, IRankPlacementGetter? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return x.Faction.FormKey.Equals(y.Faction.FormKey) && x.Rank == y.Rank;
            }

            public int GetHashCode(IRankPlacementGetter obj)
            {
                return HashCode.Combine(obj.Faction.FormKey, obj.Rank);
            }
        }

        private class FactionFormKeyComparer : IEqualityComparer<IRankPlacementGetter>
        {
            public bool Equals(IRankPlacementGetter? x, IRankPlacementGetter? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return x.Faction.FormKey.Equals(y.Faction.FormKey);
            }

            public int GetHashCode(IRankPlacementGetter obj)
            {
                return obj.Faction.FormKey.GetHashCode();
            }
        }

        protected override string FormatItem(IRankPlacementGetter item)
        {
            return $"{item.Faction.FormKey}(Rank {item.Rank})";
        }

        public override object? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
            {
                var listState = new ItemStateCollection<IRankPlacementGetter>();
                listState.Items = npc.Factions.Select(f => new ItemState<IRankPlacementGetter>(f, "")).ToList();
                return listState;
            }
            return null;
        }

        public override void UpdatePropertyState(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            PropertyState propertyState)
        {
            this.context = context;
            if (context.Record is not INpcGetter record)
                return;

            var recordItems = ListAccessor(record).ToList();
            var currentFinalItems = (ItemStateCollection<IRankPlacementGetter>)propertyState.FinalValue!;
            var recordMod = state.LoadOrder[context.ModKey].Mod;
            var formKeyComparer = new FactionFormKeyComparer();

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
                    !e.IsRemoved && formKeyComparer.Equals(e.Item, item));

                if (matchingItemInFinalItems == null)
                {
                    // Check if this specific item was previously removed
                    var previouslyRemovedItem = currentFinalItems.Items.FirstOrDefault(e =>
                        e.IsRemoved && formKeyComparer.Equals(e.Item, item));

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

        public override PropertyState CreateState(string lastChangedByMod, object? originalValue = null)
        {
            var listState = new ItemStateCollection<IRankPlacementGetter>();
            try
            {
                if (originalValue == null)
                {
                    Console.WriteLine($"Warning: Null faction list value encountered");
                    return new PropertyState
                    {
                        OriginalValue = originalValue,
                        FinalValue = listState,
                        LastChangedByMod = lastChangedByMod
                    };
                }

                if (originalValue is ItemStateCollection<IRankPlacementGetter> originalListState)
                {
                    listState.Items = originalListState.Items.Select(item => new ItemState<IRankPlacementGetter>(item.Item, lastChangedByMod)).ToList();
                }
                else
                {
                    Console.WriteLine($"Error: Expected ListPropertyState but got {originalValue.GetType().Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing faction list: {ex.Message}");
            }

            var state = new PropertyState
            {
                OriginalValue = originalValue,
                FinalValue = listState,
                LastChangedByMod = lastChangedByMod
            };

            // Debug output
            if (originalValue is ItemStateCollection<IRankPlacementGetter> debugListState)
            {
                LogCollector.Add("Factions", $"[Factions] CreateState. LastChangedByMod: {lastChangedByMod} Original factions: {FormatFactionList(debugListState.Items.Select(i => i.Item).ToList())}");
            }
            else
            {
                LogCollector.Add("Factions", "[Factions] No original factions");
            }

            return state;
        }

        public override object? GetValueFromContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is not INpcGetter npc)
                return null;

            // Create a ListPropertyState with the current factions
            var listState = new ItemStateCollection<IRankPlacementGetter>();
            listState.Items = npc.Factions.Select(f => new ItemState<IRankPlacementGetter>(f, context.ModKey.ToString())).ToList();
            return listState;
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
            Console.WriteLine($"Forwarded factions: {FormatFactionList(oldFactions)} -> {FormatFactionList(npc.Factions)}");
        }

        public override bool AreValuesEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1 is ItemStateCollection<IRankPlacementGetter> list1 &&
                value2 is ItemStateCollection<IRankPlacementGetter> list2)
            {
                return list1.Items.SequenceEqual(list2.Items, new FactionComparer());
            }
            return false;
        }
    }
}
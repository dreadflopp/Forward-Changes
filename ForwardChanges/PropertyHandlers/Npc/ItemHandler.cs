using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Noggog;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class ItemHandler : AbstractListPropertyHandler<ContainerEntry>
    {
        public override string PropertyName => "Items";

        public override void SetValue(IMajorRecord record, List<ContainerEntry>? value)
        {
            if (record is INpc npc)
            {
                npc.Items = value != null ? new ExtendedList<ContainerEntry>(value) : null;
            }
            else
            {
                LogCollector.Add(PropertyName, $"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override List<ContainerEntry>? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
            {
                return npc.Items?.Select((item, index) =>
                {
                    return new ContainerEntry
                    {
                        Item = new ContainerItem
                        {
                            Item = new FormLink<IItemGetter>(item.Item.Item.FormKey),
                            Count = item.Item.Count
                        },
                        Data = item.Data != null ? new ExtraData
                        {
                            Owner = DeepCopyOwner(item.Data.Owner),
                            ItemCondition = item.Data.ItemCondition
                        } : null
                    };
                }).ToList();
            }

            LogCollector.Add(PropertyName, $"Error: Record does not implement INpcGetter for {PropertyName}");
            return null;
        }

        private OwnerTarget DeepCopyOwner(IOwnerTargetGetter? owner)
        {
            return OwnerTargetUtility.DeepCopy(owner);
        }

        protected override bool IsItemEqual(ContainerEntry? item1, ContainerEntry? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Only compare by FormKey - other properties are handled in ProcessHandlerSpecificLogic
            return item1.Item.Item.FormKey == item2.Item.Item.FormKey;
        }

        protected override string FormatItem(ContainerEntry? item)
        {
            if (item == null) return "null";
            var result = $"{item.Item.Item.FormKey} (Count: {item.Item.Count}";
            if (item.Data != null)
            {
                result += $", Condition: {item.Data.ItemCondition}";
                if (item.Data.Owner != null)
                {
                    result += $", Owner: {FormatOwner(item.Data.Owner)}";
                }
            }
            result += ")";
            return result;
        }

        protected override void ProcessHandlerSpecificLogic(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            ListPropertyContext<ContainerEntry> listPropertyContext,
            List<ContainerEntry> recordItems,
            List<ListPropertyValueContext<ContainerEntry>> currentForwardItems)
        {
            var recordMod = state.LoadOrder[context.ModKey].Mod;
            if (recordMod == null) return;

            // Group items by FormKey for optimal matching
            var forwardItemsByFormKey = currentForwardItems
                .Where(i => !i.IsRemoved)
                .GroupBy(i => i.Value.Item.Item.FormKey)
                .ToDictionary(g => g.Key, g => g.ToList());

            var recordItemsByFormKey = recordItems
                .Select((item, index) => (Item: item, Index: index))
                .GroupBy(x => x.Item.Item.Item.FormKey)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Process each FormKey group
            foreach (var formKeyGroup in forwardItemsByFormKey)
            {
                var formKey = formKeyGroup.Key;
                var forwardItems = formKeyGroup.Value;

                if (!recordItemsByFormKey.TryGetValue(formKey, out var recordItemsForFormKey))
                {
                    continue;
                }

                // Find optimal matching using Hungarian algorithm approach
                var optimalMatches = FindOptimalMatches(forwardItems, recordItemsForFormKey, recordMod);

                // Apply the optimal matches
                foreach (var match in optimalMatches)
                {
                    var forwardItem = match.ForwardItem;
                    var recordItem = match.RecordItem;

                    bool hasChanges = false;
                    var changes = new List<string>();

                    // Update count if it's different and we have permissions
                    if (recordItem.Item.Count != forwardItem.Value.Item.Count)
                    {
                        if (HasPermissionsToModify(recordMod, forwardItem.OwnerMod))
                        {
                            var oldCount = forwardItem.Value.Item.Count;
                            forwardItem.Value.Item.Count = recordItem.Item.Count;
                            changes.Add($"count {oldCount} -> {recordItem.Item.Count}");
                            hasChanges = true;
                        }
                        else
                        {
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Cannot update count for {forwardItem.Value.Item.Item.FormKey} - no permission (owned by {forwardItem.OwnerMod})");
                        }
                    }

                    // Update condition if it's different and we have permissions
                    var recordCondition = recordItem.Data?.ItemCondition ?? 0f;
                    var forwardCondition = forwardItem.Value.Data?.ItemCondition ?? 0f;
                    if (Math.Abs(recordCondition - forwardCondition) > 0.001f) // Float comparison with tolerance
                    {
                        if (HasPermissionsToModify(recordMod, forwardItem.OwnerMod))
                        {
                            // Ensure Data exists
                            if (forwardItem.Value.Data == null)
                            {
                                forwardItem.Value.Data = new ExtraData();
                            }
                            var oldCondition = forwardItem.Value.Data.ItemCondition;
                            forwardItem.Value.Data.ItemCondition = recordCondition;
                            changes.Add($"condition {oldCondition} -> {recordCondition}");
                            hasChanges = true;
                        }
                        else
                        {
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Cannot update condition for {forwardItem.Value.Item.Item.FormKey} - no permission (owned by {forwardItem.OwnerMod})");
                        }
                    }

                    // Ensure Data exists for owner updates
                    if (forwardItem.Value.Data == null)
                    {
                        forwardItem.Value.Data = new ExtraData();
                    }

                    var forwardData = forwardItem.Value.Data; // Non-null after check above

                    // Update owner if it's different and we have permissions
                    var recordOwner = recordItem.Data?.Owner;
                    var forwardOwner = forwardData.Owner;
                    if (!OwnerTargetUtility.AreEqual(recordOwner, forwardOwner))
                    {
                        if (HasPermissionsToModify(recordMod, forwardItem.OwnerMod))
                        {
                            var oldOwner = forwardData.Owner;
                            forwardData.Owner = DeepCopyOwner(recordOwner);
                            changes.Add($"owner {FormatOwner(oldOwner)} -> {FormatOwner(recordOwner)}");
                            hasChanges = true;
                        }
                        else
                        {
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Cannot update owner for {forwardItem.Value.Item.Item.FormKey} - no permission (owned by {forwardItem.OwnerMod})");
                        }
                    }

                    // Log changes if any were made
                    if (hasChanges)
                    {
                        var oldOwner = forwardItem.OwnerMod;
                        forwardItem.OwnerMod = context.ModKey.ToString();
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Updated {string.Join(", ", changes)} for {forwardItem.Value.Item.Item.FormKey} (was owned by {oldOwner}) Success");
                    }
                }
            }
        }

        private List<(ListPropertyValueContext<ContainerEntry> ForwardItem, ContainerEntry RecordItem)> FindOptimalMatches(
            List<ListPropertyValueContext<ContainerEntry>> forwardItems,
            List<(ContainerEntry Item, int Index)> recordItemsForFormKey,
            ISkyrimModGetter recordMod)
        {
            var matches = new List<(ListPropertyValueContext<ContainerEntry>, ContainerEntry)>();
            var usedRecordIndices = new HashSet<int>();

            // For each forward item, find the best available record item
            foreach (var forwardItem in forwardItems)
            {
                var bestMatch = recordItemsForFormKey
                    .Where(x => !usedRecordIndices.Contains(x.Index))
                    .Select(x => new
                    {
                        RecordItem = x.Item,
                        Index = x.Index,
                        Cost = CalculateChangeCost(forwardItem.Value, x.Item, recordMod, forwardItem.OwnerMod)
                    })
                    .OrderBy(x => x.Cost)
                    .FirstOrDefault();

                if (bestMatch != null)
                {
                    matches.Add((forwardItem, bestMatch.RecordItem));
                    usedRecordIndices.Add(bestMatch.Index);
                }
            }

            return matches;
        }

        private int CalculateChangeCost(ContainerEntry forwardItem, ContainerEntry recordItem, ISkyrimModGetter recordMod, string ownerMod)
        {
            int cost = 0;
            bool hasPermission = HasPermissionsToModify(recordMod, ownerMod);

            // If no permission, cost is very high (but not infinite to allow fallback)
            if (!hasPermission)
            {
                cost += 1000;
            }

            // Count changes needed
            if (forwardItem.Item.Count != recordItem.Item.Count) cost += 1;

            var forwardCondition = forwardItem.Data?.ItemCondition ?? 0f;
            var recordCondition = recordItem.Data?.ItemCondition ?? 0f;
            if (Math.Abs(forwardCondition - recordCondition) > 0.001f) cost += 1;

            if (!OwnerTargetUtility.AreEqual(forwardItem.Data?.Owner, recordItem.Data?.Owner)) cost += 1;

            return cost;
        }

        private string FormatOwner(IOwnerTargetGetter? owner)
        {
            if (owner == null) return "null";

            switch (owner)
            {
                case IUntypedOwnerGetter untypedOwner:
                    return $"UntypedOwner(OwnerData:{untypedOwner.OwnerData.FormKey}, VariableData:{untypedOwner.VariableData.FormKey})";

                case IFactionOwnerGetter factionOwner:
                    return $"FactionOwner(Faction:{factionOwner.Faction.FormKey}, Rank:{factionOwner.RequiredRank})";

                case INpcOwnerGetter npcOwner:
                    return $"NpcOwner(NPC:{npcOwner.Npc.FormKey}, Global:{npcOwner.Global.FormKey})";

                default:
                    return $"{owner.GetType().Name}({owner.GetHashCode():X8})";
            }
        }

    }
}

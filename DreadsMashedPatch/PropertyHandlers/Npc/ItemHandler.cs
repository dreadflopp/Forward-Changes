using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using DreadsMashedPatch.Contexts;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Noggog;

namespace DreadsMashedPatch.PropertyHandlers.Npc
{
    public class ItemHandler : AbstractListPropertyHandler<ContainerEntry>
    {
        public override string PropertyName => "Items";
        public override ListSemantics Semantics => ListSemantics.SortedKeyed;

        protected override IReadOnlyList<object?> GetSortKey(ContainerEntry item) => [item.Item.Item.FormKey];

        protected override bool IsItemIdentityEqual(ContainerEntry? left, ContainerEntry? right) =>
            left?.Item.Item.FormKey == right?.Item.Item.FormKey;

        protected override ContainerEntry CopyItemForForwardContext(ContainerEntry item) => CopyItem(item);

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
                return npc.Items?.Select(CopyItem).ToList();
            }

            LogCollector.Add(PropertyName, $"Error: Record does not implement INpcGetter for {PropertyName}");
            return null;
        }

        private OwnerTarget DeepCopyOwner(IOwnerTargetGetter owner)
        {
            return OwnerTargetUtility.DeepCopy(owner);
        }

        private ExtraData DeepCopyExtraData(IExtraDataGetter data)
        {
            return new ExtraData
            {
                Owner = DeepCopyOwner(data.Owner),
                ItemCondition = data.ItemCondition
            };
        }

        private ContainerEntry CopyItem(IContainerEntryGetter item) => new()
        {
            Item = new ContainerItem
            {
                Item = new FormLink<IItemGetter>(item.Item.Item.FormKey),
                Count = item.Item.Count
            },
            Data = item.Data == null ? null : DeepCopyExtraData(item.Data)
        };

        public override bool AreValuesEqual(List<ContainerEntry>? value1, List<ContainerEntry>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null || value1.Count != value2.Count) return false;

            // Inventory order is not semantic, but duplicate FormKeys are valid. Match
            // each complete entry once instead of grouping by FormKey and losing duplicates.
            var unmatched = value2.ToList();
            foreach (var item1 in value1)
            {
                var matchIndex = unmatched.FindIndex(item2 => AreItemContentsEqual(item1, item2));
                if (matchIndex < 0) return false;
                unmatched.RemoveAt(matchIndex);
            }

            return true;
        }

        private static bool AreItemContentsEqual(ContainerEntry? item1, ContainerEntry? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;
            if (item1.Item.Item.FormKey != item2.Item.Item.FormKey) return false;
            if (item1.Item.Count != item2.Item.Count) return false;

            var data1 = item1.Data;
            var data2 = item2.Data;
            if (data1 == null || data2 == null)
            {
                return data1 == null && data2 == null;
            }

            return Math.Abs(data1.ItemCondition - data2.ItemCondition) <= 0.001f
                   && OwnerTargetUtility.AreEqual(data1.Owner, data2.Owner);
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

                    hasChanges |= ReconcileExtraData(
                        context.ModKey.ToString(),
                        recordMod,
                        forwardItem,
                        recordItem.Data,
                        changes);

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

        internal bool ReconcileExtraData(
            string modName,
            ISkyrimModGetter recordMod,
            ListPropertyValueContext<ContainerEntry> forwardItem,
            IExtraDataGetter? recordData,
            List<string> changes)
        {
            var forwardData = forwardItem.Value.Data;

            // COED presence is meaningful. Do not collapse an absent group into
            // a present group containing Mutagen's default UntypedOwner and zero
            // condition values.
            if (recordData == null || forwardData == null)
            {
                if (recordData == null && forwardData == null)
                {
                    return false;
                }

                if (!HasPermissionsToModify(recordMod, forwardItem.OwnerMod))
                {
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {modName}: Cannot update extra data for {forwardItem.Value.Item.Item.FormKey} - no permission (owned by {forwardItem.OwnerMod})");
                    return false;
                }

                forwardItem.Value.Data = recordData == null
                    ? null
                    : DeepCopyExtraData(recordData);
                changes.Add(recordData == null
                    ? "extra data present -> null"
                    : "extra data null -> present");
                return true;
            }

            var hasChanges = false;

            if (Math.Abs(recordData.ItemCondition - forwardData.ItemCondition) > 0.001f)
            {
                if (HasPermissionsToModify(recordMod, forwardItem.OwnerMod))
                {
                    var oldCondition = forwardData.ItemCondition;
                    forwardData.ItemCondition = recordData.ItemCondition;
                    changes.Add($"condition {oldCondition} -> {recordData.ItemCondition}");
                    hasChanges = true;
                }
                else
                {
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {modName}: Cannot update condition for {forwardItem.Value.Item.Item.FormKey} - no permission (owned by {forwardItem.OwnerMod})");
                }
            }

            if (!OwnerTargetUtility.AreEqual(recordData.Owner, forwardData.Owner))
            {
                if (HasPermissionsToModify(recordMod, forwardItem.OwnerMod))
                {
                    var oldOwner = forwardData.Owner;
                    forwardData.Owner = DeepCopyOwner(recordData.Owner);
                    changes.Add($"owner {FormatOwner(oldOwner)} -> {FormatOwner(recordData.Owner)}");
                    hasChanges = true;
                }
                else
                {
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {modName}: Cannot update owner for {forwardItem.Value.Item.Item.FormKey} - no permission (owned by {forwardItem.OwnerMod})");
                }
            }

            return hasChanges;
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

            if (forwardItem.Data == null || recordItem.Data == null)
            {
                if (forwardItem.Data != null || recordItem.Data != null) cost += 1;
                return cost;
            }

            if (Math.Abs(forwardItem.Data.ItemCondition - recordItem.Data.ItemCondition) > 0.001f) cost += 1;

            if (!OwnerTargetUtility.AreEqual(forwardItem.Data.Owner, recordItem.Data.Owner)) cost += 1;

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

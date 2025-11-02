using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Abstracts;
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

        /// <summary>
        /// Deep copies an OwnerTarget object, preserving all nested properties.
        /// Uses interface-based detection first (more reliable), then falls back to type name.
        /// Also preserves raw data fields (RawOwnerData and RawVariableData) if they exist on any owner type.
        /// </summary>
        private OwnerTarget DeepCopyOwner(IOwnerTargetGetter? owner)
        {
            if (owner == null)
            {
                return new NoOwner();
            }

            OwnerTarget? copiedOwner = null;

            // Use interface-based detection (Mutagen ensures all types implement their getter interfaces)
            if (owner is IFactionOwnerGetter factionOwner)
            {
                copiedOwner = new FactionOwner
                {
                    Faction = (IFormLink<IFactionGetter>)factionOwner.Faction,
                    RequiredRank = factionOwner.RequiredRank
                };
            }
            else if (owner is INpcOwnerGetter npcOwner)
            {
                copiedOwner = new NpcOwner
                {
                    Npc = (IFormLink<INpcGetter>)npcOwner.Npc,
                    Global = (IFormLink<IGlobalGetter>)npcOwner.Global
                };
            }
            else if (owner is INoOwnerGetter)
            {
                copiedOwner = new NoOwner();
            }
            else
            {
                // Unknown type - log warning and return NoOwner as safe default
                LogCollector.Add(PropertyName, $"WARNING: Unknown owner type '{owner.GetType().Name}' - using NoOwner as default");
                copiedOwner = new NoOwner();
            }

            // Preserve raw data fields if they exist on the original owner (works for any owner type)
            if (copiedOwner != null)
            {
                var originalOwnerType = owner.GetType();
                var rawOwnerDataProp = originalOwnerType.GetProperty("RawOwnerData");
                var rawVariableDataProp = originalOwnerType.GetProperty("RawVariableData");

                if (rawOwnerDataProp != null && rawVariableDataProp != null)
                {
                    var copiedOwnerType = copiedOwner.GetType();
                    var copiedRawOwnerDataProp = copiedOwnerType.GetProperty("RawOwnerData");
                    var copiedRawVariableDataProp = copiedOwnerType.GetProperty("RawVariableData");

                    if (copiedRawOwnerDataProp != null && copiedRawVariableDataProp != null &&
                        copiedRawOwnerDataProp.CanWrite && copiedRawVariableDataProp.CanWrite)
                    {
                        // Copy raw data fields if both original and copy have them
                        uint rawOwnerData = (uint)(rawOwnerDataProp.GetValue(owner) ?? 0);
                        uint rawVariableData = (uint)(rawVariableDataProp.GetValue(owner) ?? 0);

                        copiedRawOwnerDataProp.SetValue(copiedOwner, rawOwnerData);
                        copiedRawVariableDataProp.SetValue(copiedOwner, rawVariableData);
                    }
                }
            }

            return copiedOwner ?? new NoOwner();
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
                    if (!AreOwnersEqual(recordOwner, forwardOwner))
                    {
                        if (HasPermissionsToModify(recordMod, forwardItem.OwnerMod))
                        {
                            // Try to update individual owner properties first
                            if (TryUpdateOwnerProperties(forwardData, recordOwner, changes))
                            {
                                hasChanges = true;
                            }
                            else
                            {
                                // Fallback: replace entire owner object
                                var oldOwner = forwardData.Owner;
                                forwardData.Owner = DeepCopyOwner(recordOwner!);
                                changes.Add($"owner {FormatOwner(oldOwner)} -> {FormatOwner(recordOwner)}");
                                hasChanges = true;
                            }
                        }
                        else
                        {
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Cannot update owner for {forwardItem.Value.Item.Item.FormKey} - no permission (owned by {forwardItem.OwnerMod})");
                        }
                    }

                    // Always check and update raw data fields independently (even if owner types match)
                    // RawOwnerData and RawVariableData are separate properties that should be compared independently
                    if (HasPermissionsToModify(recordMod, forwardItem.OwnerMod))
                    {
                        if (TryUpdateRawDataFields(forwardData, recordItem.Data, changes))
                        {
                            hasChanges = true;
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

            if (!AreOwnersEqual(forwardItem.Data?.Owner, recordItem.Data?.Owner)) cost += 1;

            return cost;
        }

        private bool AreOwnersEqual(IOwnerTargetGetter? owner1, IOwnerTargetGetter? owner2)
        {
            if (owner1 == null && owner2 == null) return true;
            if (owner1 == null || owner2 == null) return false;

            // Get the actual types
            var type1 = owner1.GetType();
            var type2 = owner2.GetType();

            // Different owner types are never equal
            if (type1 != type2)
            {
                return false;
            }

            // Handle NoOwner - compare raw data fields
            if (type1.Name == "NoOwner")
            {
                var noOwner1 = owner1 as INoOwnerGetter;
                var noOwner2 = owner2 as INoOwnerGetter;

                if (noOwner1 != null && noOwner2 != null)
                {
                    // Compare raw data fields using reflection
                    var rawOwnerData1Prop = type1.GetProperty("RawOwnerData");
                    var rawOwnerData2Prop = type2.GetProperty("RawOwnerData");
                    var rawVariableData1Prop = type1.GetProperty("RawVariableData");
                    var rawVariableData2Prop = type2.GetProperty("RawVariableData");

                    uint rawOwnerData1 = 0, rawOwnerData2 = 0;
                    uint rawVariableData1 = 0, rawVariableData2 = 0;

                    if (rawOwnerData1Prop != null) rawOwnerData1 = (uint)(rawOwnerData1Prop.GetValue(owner1) ?? 0);
                    if (rawOwnerData2Prop != null) rawOwnerData2 = (uint)(rawOwnerData2Prop.GetValue(owner2) ?? 0);
                    if (rawVariableData1Prop != null) rawVariableData1 = (uint)(rawVariableData1Prop.GetValue(owner1) ?? 0);
                    if (rawVariableData2Prop != null) rawVariableData2 = (uint)(rawVariableData2Prop.GetValue(owner2) ?? 0);

                    return rawOwnerData1 == rawOwnerData2 && rawVariableData1 == rawVariableData2;
                }

                // Fallback: if reflection fails, compare by reference (not ideal but safe)
                return owner1.Equals(owner2);
            }

            // Handle FactionOwner - compare both Faction and RequiredRank
            if (type1.Name == "FactionOwner")
            {
                var faction1 = (IFactionOwnerGetter)owner1;
                var faction2 = (IFactionOwnerGetter)owner2;

                return faction1.Faction.FormKey == faction2.Faction.FormKey &&
                       faction1.RequiredRank == faction2.RequiredRank;
            }

            // Handle NpcOwner - compare both Npc and Global properties
            if (type1.Name == "NpcOwner")
            {
                var npc1 = (INpcOwnerGetter)owner1;
                var npc2 = (INpcOwnerGetter)owner2;

                return npc1.Npc.FormKey == npc2.Npc.FormKey &&
                       npc1.Global.FormKey == npc2.Global.FormKey;
            }

            // For unknown owner types, log a warning and return false (conservative approach)
            LogCollector.Add(PropertyName, $"WARNING: Unknown owner type '{type1.Name}' - treating as different");
            return false;
        }


        private string FormatOwner(IOwnerTargetGetter? owner)
        {
            if (owner == null) return "null";

            var typeName = owner.GetType().Name;

            switch (typeName)
            {
                case "NoOwner":
                    // Check if NoOwner has raw data (FactionOwner data)
                    var noOwnerType = owner.GetType();
                    var rawOwnerDataProp = noOwnerType.GetProperty("RawOwnerData");
                    var rawVariableDataProp = noOwnerType.GetProperty("RawVariableData");

                    if (rawOwnerDataProp != null && rawVariableDataProp != null)
                    {
                        uint rawOwnerData = (uint)(rawOwnerDataProp.GetValue(owner) ?? 0);
                        uint rawVariableData = (uint)(rawVariableDataProp.GetValue(owner) ?? 0);

                        if (rawOwnerData != 0 || rawVariableData != 0)
                        {
                            return $"NoOwner(RawOwnerData:0x{rawOwnerData:X8}, RawVariableData:{rawVariableData})";
                        }
                    }
                    return "NoOwner";

                case "FactionOwner":
                    var factionOwner = (IFactionOwnerGetter)owner;
                    return $"FactionOwner(Faction:{factionOwner.Faction.FormKey}, Rank:{factionOwner.RequiredRank})";

                case "NpcOwner":
                    var npcOwner = (INpcOwnerGetter)owner;
                    return $"NpcOwner(NPC:{npcOwner.Npc.FormKey}, Global:{npcOwner.Global.FormKey})";

                default:
                    return $"{typeName}({owner.GetHashCode():X8})";
            }
        }

        /// <summary>
        /// Tries to update individual properties within the owner object.
        /// Returns true if any properties were updated, false if the entire owner needs to be replaced.
        /// </summary>
        private bool TryUpdateOwnerProperties(ExtraData forwardData, IOwnerTargetGetter? recordOwner, List<string> changes)
        {
            if (recordOwner == null || forwardData.Owner == null) return false;

            var forwardOwnerType = forwardData.Owner.GetType().Name;
            var recordOwnerType = recordOwner.GetType().Name;

            // Only try to update properties if both owners are the same type
            if (forwardOwnerType != recordOwnerType) return false;

            try
            {
                switch (forwardOwnerType)
                {
                    case "FactionOwner":
                        return TryUpdateFactionOwnerProperties(forwardData, recordOwner, changes);

                    case "NpcOwner":
                        return TryUpdateNpcOwnerProperties(forwardData, recordOwner, changes);

                    case "NoOwner":
                        // Raw data for NoOwner is handled separately by TryUpdateRawDataFields
                        return false;

                    default:
                        // Unknown owner type - can't update individual properties
                        return false;
                }
            }
            catch
            {
                // If anything fails, fall back to replacing the entire owner
                return false;
            }
        }

        private bool TryUpdateFactionOwnerProperties(ExtraData forwardData, IOwnerTargetGetter recordOwner, List<string> changes)
        {
            var forwardFactionOwner = (IFactionOwnerGetter)forwardData.Owner;
            var recordFactionOwner = (IFactionOwnerGetter)recordOwner;

            bool updated = false;

            // Update Faction if different
            if (forwardFactionOwner.Faction.FormKey != recordFactionOwner.Faction.FormKey)
            {
                // We need the mutable version to update properties
                // For now, we'll fall back to replacing the entire owner
                return false;
            }

            // Update RequiredRank if different
            if (forwardFactionOwner.RequiredRank != recordFactionOwner.RequiredRank)
            {
                // We need the mutable version to update properties
                // For now, we'll fall back to replacing the entire owner
                return false;
            }

            return updated;
        }

        private bool TryUpdateNpcOwnerProperties(ExtraData forwardData, IOwnerTargetGetter recordOwner, List<string> changes)
        {
            var forwardNpcOwner = (INpcOwnerGetter)forwardData.Owner;
            var recordNpcOwner = (INpcOwnerGetter)recordOwner;

            bool updated = false;

            // Update Npc if different
            if (forwardNpcOwner.Npc.FormKey != recordNpcOwner.Npc.FormKey)
            {
                // We need the mutable version to update properties
                // For now, we'll fall back to replacing the entire owner
                return false;
            }

            // Update Global if different
            if (forwardNpcOwner.Global.FormKey != recordNpcOwner.Global.FormKey)
            {
                // We need the mutable version to update properties
                // For now, we'll fall back to replacing the entire owner
                return false;
            }

            return updated;
        }

        /// <summary>
        /// Compares and updates RawOwnerData and RawVariableData fields independently.
        /// These are separate properties that should be compared and updated even if owner types match.
        /// </summary>
        private bool TryUpdateRawDataFields(ExtraData forwardData, ExtraData? recordData, List<string> changes)
        {
            if (recordData == null || forwardData.Owner == null) return false;

            bool updated = false;

            // Get raw data from record owner
            var recordOwnerType = recordData.Owner?.GetType();
            var recordRawOwnerDataProp = recordOwnerType?.GetProperty("RawOwnerData");
            var recordRawVariableDataProp = recordOwnerType?.GetProperty("RawVariableData");

            if (recordRawOwnerDataProp == null || recordRawVariableDataProp == null) return false;

            uint recordRawOwnerData = (uint)(recordRawOwnerDataProp.GetValue(recordData.Owner) ?? 0);
            uint recordRawVariableData = (uint)(recordRawVariableDataProp.GetValue(recordData.Owner) ?? 0);

            // Get raw data from forward owner
            var forwardOwnerType = forwardData.Owner.GetType();
            var forwardRawOwnerDataProp = forwardOwnerType.GetProperty("RawOwnerData");
            var forwardRawVariableDataProp = forwardOwnerType.GetProperty("RawVariableData");

            if (forwardRawOwnerDataProp == null || forwardRawVariableDataProp == null) return false;

            uint forwardRawOwnerData = (uint)(forwardRawOwnerDataProp.GetValue(forwardData.Owner) ?? 0);
            uint forwardRawVariableData = (uint)(forwardRawVariableDataProp.GetValue(forwardData.Owner) ?? 0);

            // Update RawOwnerData if different - use reflection to set on any owner type that has this property
            if (recordRawOwnerData != forwardRawOwnerData)
            {
                // Check if property is writable
                if (forwardRawOwnerDataProp.CanWrite)
                {
                    try
                    {
                        forwardRawOwnerDataProp.SetValue(forwardData.Owner, recordRawOwnerData);
                        changes.Add($"RawOwnerData 0x{forwardRawOwnerData:X8} -> 0x{recordRawOwnerData:X8}");
                        updated = true;
                    }
                    catch (Exception ex)
                    {
                        LogCollector.Add(PropertyName, $"WARNING: Failed to update RawOwnerData: {ex.Message}");
                    }
                }
                else
                {
                    LogCollector.Add(PropertyName, $"WARNING: RawOwnerData property is read-only on {forwardOwnerType.Name}");
                }
            }

            // Update RawVariableData if different (this includes RequiredRank) - use reflection to set on any owner type
            if (recordRawVariableData != forwardRawVariableData)
            {
                // Check if property is writable
                if (forwardRawVariableDataProp.CanWrite)
                {
                    try
                    {
                        forwardRawVariableDataProp.SetValue(forwardData.Owner, recordRawVariableData);
                        changes.Add($"RawVariableData {forwardRawVariableData} -> {recordRawVariableData}");
                        updated = true;
                    }
                    catch (Exception ex)
                    {
                        LogCollector.Add(PropertyName, $"WARNING: Failed to update RawVariableData: {ex.Message}");
                    }
                }
                else
                {
                    LogCollector.Add(PropertyName, $"WARNING: RawVariableData property is read-only on {forwardOwnerType.Name}");
                }
            }

            return updated;
        }

    }
}


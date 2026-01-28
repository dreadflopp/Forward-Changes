using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ForwardChanges.PropertyHandlers.LeveledItem
{
    public class EntriesHandler : AbstractListPropertyHandler<ILeveledItemEntryGetter>
    {
        public override string PropertyName => "Entries";

        protected override ListOrdering Ordering => ListOrdering.None; // Order doesn't matter, but we sort for consistency

        // Helper method to cast records (not available in AbstractListPropertyHandler)
        private static TRecord? TryCastRecord<TRecord>(IMajorRecord record, string propertyName) where TRecord : class
        {
            if (record is TRecord typedRecord)
            {
                return typedRecord;
            }
            Console.WriteLine($"Error: Record does not implement {typeof(TRecord).Name} for {propertyName}");
            return null;
        }

        private static TRecord? TryCastRecord<TRecord>(IMajorRecordGetter record, string propertyName) where TRecord : class
        {
            if (record is TRecord typedRecord)
            {
                return typedRecord;
            }
            Console.WriteLine($"Error: Record does not implement {typeof(TRecord).Name} for {propertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<ILeveledItemEntryGetter>? value)
        {
            var leveledItem = TryCastRecord<ILeveledItem>(record, PropertyName);
            if (leveledItem != null)
            {
                leveledItem.Entries?.Clear();
                if (value != null && value.Count > 0)
                {
                    if (leveledItem.Entries == null)
                    {
                        leveledItem.Entries = new ExtendedList<LeveledItemEntry>();
                    }

                    // Sort entries by Level first, then Reference before setting
                    var sortedEntries = value.OrderBy(e => GetSortKey(e)).ToList();

                    foreach (var entryGetter in sortedEntries)
                    {
                        var newEntry = new LeveledItemEntry();
                        
                        // Copy Data
                        if (entryGetter.Data != null)
                        {
                            newEntry.Data = new LeveledItemEntryData
                            {
                                Level = entryGetter.Data.Level,
                                Unknown = entryGetter.Data.Unknown,
                                Reference = new FormLink<IItemGetter>(entryGetter.Data.Reference.FormKey),
                                Count = entryGetter.Data.Count,
                                Unknown2 = entryGetter.Data.Unknown2
                            };
                        }

                        // Copy ExtraData
                        if (entryGetter.ExtraData != null)
                        {
                            newEntry.ExtraData = DeepCopyExtraData(entryGetter.ExtraData);
                        }

                        leveledItem.Entries.Add(newEntry);
                    }
                }
            }
        }

        public override List<ILeveledItemEntryGetter>? GetValue(IMajorRecordGetter record)
        {
            var leveledItem = TryCastRecord<ILeveledItemGetter>(record, PropertyName);
            if (leveledItem != null && leveledItem.Entries != null)
            {
                // Sort entries by Level first, then Reference before returning
                return leveledItem.Entries
                    .OrderBy(e => GetSortKey(e))
                    .ToList();
            }
            return null;
        }

        protected override bool IsItemEqual(ILeveledItemEntryGetter? item1, ILeveledItemEntryGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare Data
            if (!AreDataEqual(item1.Data, item2.Data))
            {
                return false;
            }

            // Compare ExtraData (both might be null)
            if (!AreExtraDataEqual(item1.ExtraData, item2.ExtraData))
            {
                return false;
            }

            return true;
        }

        private bool AreDataEqual(ILeveledItemEntryDataGetter? data1, ILeveledItemEntryDataGetter? data2)
        {
            if (data1 == null && data2 == null) return true;
            if (data1 == null || data2 == null) return false;

            return data1.Level == data2.Level &&
                   data1.Unknown == data2.Unknown &&
                   data1.Reference.FormKey == data2.Reference.FormKey &&
                   data1.Count == data2.Count &&
                   data1.Unknown2 == data2.Unknown2;
        }

        private bool AreExtraDataEqual(IExtraDataGetter? extraData1, IExtraDataGetter? extraData2)
        {
            if (extraData1 == null && extraData2 == null) return true;
            if (extraData1 == null || extraData2 == null) return false;

            // Compare ItemCondition
            if (Math.Abs(extraData1.ItemCondition - extraData2.ItemCondition) > 0.0001f)
            {
                return false;
            }

            // Compare Owner
            if (!AreOwnersEqual(extraData1.Owner, extraData2.Owner))
            {
                return false;
            }

            return true;
        }

        private bool AreOwnersEqual(IOwnerTargetGetter? owner1, IOwnerTargetGetter? owner2)
        {
            if (owner1 == null && owner2 == null) return true;
            if (owner1 == null || owner2 == null) return false;

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

            // Handle FactionOwner
            if (type1.Name == "FactionOwner")
            {
                var faction1 = (IFactionOwnerGetter)owner1;
                var faction2 = (IFactionOwnerGetter)owner2;

                return faction1.Faction.FormKey == faction2.Faction.FormKey &&
                       faction1.RequiredRank == faction2.RequiredRank;
            }

            // Handle NpcOwner
            if (type1.Name == "NpcOwner")
            {
                var npc1 = (INpcOwnerGetter)owner1;
                var npc2 = (INpcOwnerGetter)owner2;

                return npc1.Npc.FormKey == npc2.Npc.FormKey &&
                       npc1.Global.FormKey == npc2.Global.FormKey;
            }

            // For unknown owner types, return false (conservative approach)
            return false;
        }

        private ExtraData DeepCopyExtraData(IExtraDataGetter extraData)
        {
            var newExtraData = new ExtraData
            {
                ItemCondition = extraData.ItemCondition,
                Owner = DeepCopyOwner(extraData.Owner)
            };
            return newExtraData;
        }

        private OwnerTarget DeepCopyOwner(IOwnerTargetGetter owner)
        {
            OwnerTarget? copiedOwner = null;

            if (owner is IFactionOwnerGetter factionOwner)
            {
                copiedOwner = new FactionOwner
                {
                    Faction = new FormLink<IFactionGetter>(factionOwner.Faction.FormKey),
                    RequiredRank = factionOwner.RequiredRank
                };
            }
            else if (owner is INpcOwnerGetter npcOwner)
            {
                copiedOwner = new NpcOwner
                {
                    Npc = new FormLink<INpcGetter>(npcOwner.Npc.FormKey),
                    Global = new FormLink<IGlobalGetter>(npcOwner.Global.FormKey)
                };
            }
            else if (owner is INoOwnerGetter)
            {
                copiedOwner = new NoOwner();
            }
            else
            {
                // Unknown type - use NoOwner as safe default
                copiedOwner = new NoOwner();
            }

            // Preserve raw data fields if they exist
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
                        uint rawOwnerData = (uint)(rawOwnerDataProp.GetValue(owner) ?? 0);
                        uint rawVariableData = (uint)(rawVariableDataProp.GetValue(owner) ?? 0);

                        copiedRawOwnerDataProp.SetValue(copiedOwner, rawOwnerData);
                        copiedRawVariableDataProp.SetValue(copiedOwner, rawVariableData);
                    }
                }
            }

            return copiedOwner ?? new NoOwner();
        }

        /// <summary>
        /// Get a sort key for an entry: Level first, then Reference FormKey
        /// </summary>
        private (short Level, FormKey Reference) GetSortKey(ILeveledItemEntryGetter entry)
        {
            var level = entry.Data?.Level ?? 0;
            var reference = entry.Data?.Reference.FormKey ?? FormKey.Null;
            return (level, reference);
        }

        protected override string FormatItem(ILeveledItemEntryGetter? item)
        {
            if (item == null) return "null";
            var level = item.Data?.Level ?? 0;
            var reference = item.Data?.Reference.FormKey ?? FormKey.Null;
            return $"Entry(Level={level}, Reference={reference})";
        }
    }
}

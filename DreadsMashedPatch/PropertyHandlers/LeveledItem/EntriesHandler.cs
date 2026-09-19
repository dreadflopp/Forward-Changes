using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Noggog;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DreadsMashedPatch.PropertyHandlers.LeveledItem
{
    public class EntriesHandler : AbstractListPropertyHandler<ILeveledItemEntryGetter>
    {
        public override string PropertyName => "Entries";

        public override ListSemantics Semantics => ListSemantics.SortedKeyed;

        protected override IReadOnlyList<object?> GetSortKey(ILeveledItemEntryGetter item)
            => [item.Data?.Level ?? 0, item.Data?.Reference.FormKey ?? FormKey.Null];

        protected override bool IsItemIdentityEqual(ILeveledItemEntryGetter? left, ILeveledItemEntryGetter? right) =>
            left?.Data?.Level == right?.Data?.Level
            && left?.Data?.Reference.FormKey == right?.Data?.Reference.FormKey;

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

                    // Sort entries by Level first, then Reference string key for deterministic ordering.
                    var sortedEntries = value
                        .OrderBy(e => e.Data?.Level ?? 0)
                        .ThenBy(e => GetReferenceSortKey(e), StringComparer.OrdinalIgnoreCase)
                        .ToList();

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
                // Sort entries by Level first, then Reference string key for deterministic ordering.
                return leveledItem.Entries
                    .OrderBy(e => e.Data?.Level ?? 0)
                    .ThenBy(e => GetReferenceSortKey(e), StringComparer.OrdinalIgnoreCase)
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
            if (!OwnerTargetUtility.AreEqual(extraData1.Owner, extraData2.Owner))
            {
                return false;
            }

            return true;
        }

        private ExtraData DeepCopyExtraData(IExtraDataGetter extraData)
        {
            var newExtraData = new ExtraData
            {
                ItemCondition = extraData.ItemCondition,
                Owner = OwnerTargetUtility.DeepCopy(extraData.Owner)
            };
            return newExtraData;
        }

        private string GetReferenceSortKey(ILeveledItemEntryGetter entry)
        {
            var reference = entry.Data?.Reference.FormKey ?? FormKey.Null;
            return reference.ToString();
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

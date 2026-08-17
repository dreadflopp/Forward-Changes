using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;

namespace ForwardChanges.PropertyHandlers.LeveledNpc
{
    public class EntriesHandler : AbstractListPropertyHandler<ILeveledNpcEntryGetter>
    {
        public override string PropertyName => "Entries";

        protected override ListOrdering Ordering => ListOrdering.None;

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

        public override void SetValue(IMajorRecord record, List<ILeveledNpcEntryGetter>? value)
        {
            var leveledNpc = TryCastRecord<ILeveledNpc>(record, PropertyName);
            if (leveledNpc == null)
            {
                return;
            }

            leveledNpc.Entries?.Clear();
            if (value == null || value.Count == 0)
            {
                return;
            }

            if (leveledNpc.Entries == null)
            {
                leveledNpc.Entries = new ExtendedList<LeveledNpcEntry>();
            }

            var sortedEntries = value
                .OrderBy(e => e.Data?.Level ?? 0)
                .ThenBy(e => GetReferenceSortKey(e), StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var entryGetter in sortedEntries)
            {
                var newEntry = new LeveledNpcEntry();

                if (entryGetter.Data != null)
                {
                    newEntry.Data = new LeveledNpcEntryData
                    {
                        Level = entryGetter.Data.Level,
                        Unknown = entryGetter.Data.Unknown,
                        Reference = new FormLink<INpcSpawnGetter>(entryGetter.Data.Reference.FormKey),
                        Count = entryGetter.Data.Count,
                        Unknown2 = entryGetter.Data.Unknown2
                    };
                }

                if (entryGetter.ExtraData != null)
                {
                    newEntry.ExtraData = DeepCopyExtraData(entryGetter.ExtraData);
                }

                leveledNpc.Entries.Add(newEntry);
            }
        }

        public override List<ILeveledNpcEntryGetter>? GetValue(IMajorRecordGetter record)
        {
            var leveledNpc = TryCastRecord<ILeveledNpcGetter>(record, PropertyName);
            if (leveledNpc?.Entries == null)
            {
                return null;
            }

            return leveledNpc.Entries
                .OrderBy(e => e.Data?.Level ?? 0)
                .ThenBy(e => GetReferenceSortKey(e), StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        protected override bool IsItemEqual(ILeveledNpcEntryGetter? item1, ILeveledNpcEntryGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            if (!AreDataEqual(item1.Data, item2.Data)) return false;
            if (!AreExtraDataEqual(item1.ExtraData, item2.ExtraData)) return false;
            return true;
        }

        private bool AreDataEqual(ILeveledNpcEntryDataGetter? data1, ILeveledNpcEntryDataGetter? data2)
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
            return Math.Abs(extraData1.ItemCondition - extraData2.ItemCondition) <= 0.0001f &&
                   OwnerTargetUtility.AreEqual(extraData1.Owner, extraData2.Owner);
        }

        private ExtraData DeepCopyExtraData(IExtraDataGetter extraData)
        {
            return new ExtraData
            {
                ItemCondition = extraData.ItemCondition,
                Owner = OwnerTargetUtility.DeepCopy(extraData.Owner)
            };
        }

        private string GetReferenceSortKey(ILeveledNpcEntryGetter entry)
        {
            return entry.Data?.Reference.FormKey.ToString() ?? FormKey.Null.ToString();
        }
    }
}

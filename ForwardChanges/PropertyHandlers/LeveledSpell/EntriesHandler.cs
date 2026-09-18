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

namespace ForwardChanges.PropertyHandlers.LeveledSpell
{
    public class EntriesHandler : AbstractListPropertyHandler<ILeveledSpellEntryGetter>
    {
        public override string PropertyName => "Entries";

        public override ListSemantics Semantics => ListSemantics.SortedKeyed;

        protected override IReadOnlyList<object?> GetSortKey(ILeveledSpellEntryGetter item)
            => [item.Data?.Level ?? 0, item.Data?.Reference.FormKey ?? FormKey.Null];

        protected override bool IsItemIdentityEqual(ILeveledSpellEntryGetter? left, ILeveledSpellEntryGetter? right) =>
            left?.Data?.Level == right?.Data?.Level
            && left?.Data?.Reference.FormKey == right?.Data?.Reference.FormKey;

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

        public override void SetValue(IMajorRecord record, List<ILeveledSpellEntryGetter>? value)
        {
            var leveledSpell = TryCastRecord<ILeveledSpell>(record, PropertyName);
            if (leveledSpell == null)
            {
                return;
            }

            leveledSpell.Entries?.Clear();
            if (value == null || value.Count == 0)
            {
                return;
            }

            if (leveledSpell.Entries == null)
            {
                leveledSpell.Entries = new ExtendedList<LeveledSpellEntry>();
            }

            var sortedEntries = value
                .OrderBy(e => e.Data?.Level ?? 0)
                .ThenBy(e => GetReferenceSortKey(e), StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var entryGetter in sortedEntries)
            {
                var newEntry = new LeveledSpellEntry();

                if (entryGetter.Data != null)
                {
                    newEntry.Data = new LeveledSpellEntryData
                    {
                        Level = entryGetter.Data.Level,
                        Unknown = entryGetter.Data.Unknown,
                        Reference = new FormLink<ISpellRecordGetter>(entryGetter.Data.Reference.FormKey),
                        Count = entryGetter.Data.Count,
                        Unknown2 = entryGetter.Data.Unknown2
                    };
                }

                if (entryGetter.ExtraData != null)
                {
                    newEntry.ExtraData = DeepCopyExtraData(entryGetter.ExtraData);
                }

                leveledSpell.Entries.Add(newEntry);
            }
        }

        public override List<ILeveledSpellEntryGetter>? GetValue(IMajorRecordGetter record)
        {
            var leveledSpell = TryCastRecord<ILeveledSpellGetter>(record, PropertyName);
            if (leveledSpell?.Entries == null)
            {
                return null;
            }

            return leveledSpell.Entries
                .OrderBy(e => e.Data?.Level ?? 0)
                .ThenBy(e => GetReferenceSortKey(e), StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        protected override bool IsItemEqual(ILeveledSpellEntryGetter? item1, ILeveledSpellEntryGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            if (!AreDataEqual(item1.Data, item2.Data)) return false;
            if (!AreExtraDataEqual(item1.ExtraData, item2.ExtraData)) return false;
            return true;
        }

        private bool AreDataEqual(ILeveledSpellEntryDataGetter? data1, ILeveledSpellEntryDataGetter? data2)
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

        private string GetReferenceSortKey(ILeveledSpellEntryGetter entry)
        {
            return entry.Data?.Reference.FormKey.ToString() ?? FormKey.Null.ToString();
        }
    }
}

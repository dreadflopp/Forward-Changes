using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using ForwardChanges;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;
using System.Linq;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class StagesHandler : AbstractListPropertyHandler<IQuestStageGetter>
    {
        public override string PropertyName => "Stages";
        public override ListSemantics Semantics => ListSemantics.SortedKeyed;

        protected override bool IsItemIdentityEqual(IQuestStageGetter? left, IQuestStageGetter? right) =>
            left?.Index == right?.Index;

        protected override IReadOnlyList<object?> GetSortKey(IQuestStageGetter item) => [item.Index];

        public override List<IQuestStageGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IQuestGetter questRecord)
            {
                return questRecord.Stages?.ToList();
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IQuestStageGetter>? value)
        {
            if (record is IQuest questRecord && value != null)
            {
                if (questRecord.Stages != null)
                {
                    questRecord.Stages.Clear();
                    foreach (var stage in value)
                    {
                        if (stage is QuestStage concreteStage)
                        {
                            questRecord.Stages.Add(concreteStage);
                        }
                        else
                        {
                            // Convert IQuestStageGetter to QuestStage
                            var newStage = stage.DeepCopy();
                            questRecord.Stages.Add(newStage);
                        }
                    }
                }
            }
        }

        public override bool AreValuesEqual(List<IQuestStageGetter>? value1, List<IQuestStageGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            // Group stages by Index for efficient matching (order-independent)
            var stages1ByIndex = value1
                .Where(s => s != null)
                .GroupBy(s => s.Index)
                .ToDictionary(g => g.Key, g => g.First());

            var stages2ByIndex = value2
                .Where(s => s != null)
                .GroupBy(s => s.Index)
                .ToDictionary(g => g.Key, g => g.First());

            // Check that all stages in value1 have matching stages in value2 with same properties
            foreach (var stage1 in stages1ByIndex.Values)
            {
                if (!stages2ByIndex.TryGetValue(stage1.Index, out var stage2))
                {
                    return false; // Stage Index not found in value2
                }

                // Compare stages by all properties (for forwarding decision)
                if (!AreStagesEqual(stage1, stage2))
                {
                    return false; // Stages differ
                }
            }

            return true;
        }

        protected override bool IsItemEqual(IQuestStageGetter? item1, IQuestStageGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Match stages by Index only - properties are compared separately in AreValuesEqual
            return item1.Index == item2.Index;
        }

        private bool AreStagesEqual(IQuestStageGetter stage1, IQuestStageGetter stage2)
        {
            if (stage1.Index != stage2.Index) return false;
            if (stage1.Flags != stage2.Flags) return false;
            if (stage1.Unknown != stage2.Unknown) return false;
            if (!AreLogEntriesEqual(stage1.LogEntries, stage2.LogEntries)) return false;
            return true;
        }

        protected override string FormatItem(IQuestStageGetter? item)
        {
            return item != null ? $"Index:{item.Index}, Flags:{item.Flags}, LogEntries:{item.LogEntries.Count}" : "null";
        }

        protected override void ProcessHandlerSpecificLogic(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            ListPropertyContext<IQuestStageGetter> listPropertyContext,
            List<IQuestStageGetter> recordItems,
            List<ListPropertyValueContext<IQuestStageGetter>> currentForwardItems)
        {
            var recordMod = state.LoadOrder[context.ModKey].Mod;
            if (recordMod == null) return;

            var forwardStagesByIndex = currentForwardItems
                .Where(i => !i.IsRemoved)
                .GroupBy(i => i.Value.Index)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var recordStage in recordItems)
            {
                if (recordStage == null) continue;

                if (!forwardStagesByIndex.TryGetValue(recordStage.Index, out var forwardContext))
                    continue;

                if (!AreStagesEqual(recordStage, forwardContext.Value))
                {
                    var canTakeOwnership = HasPermissionsToModify(recordMod, forwardContext.OwnerMod);
                    var previousOwner = forwardContext.OwnerMod;

                    if (canTakeOwnership)
                    {
                        forwardContext.Value = recordStage.DeepCopy();
                        forwardContext.OwnerMod = context.ModKey.ToString();
                        LogCollector.Add(PropertyName, $"[{PropertyName}] Updating stage Index {recordStage.Index} (taking ownership as '{context.ModKey}')");
                    }
                    else
                    {
                        LogCollector.Add(PropertyName, $"[{PropertyName}] Not updating stage Index {recordStage.Index} (owned by '{previousOwner}'; permission denied)");
                    }
                }
            }
        }

        private bool AreLogEntriesEqual(IReadOnlyList<IQuestLogEntryGetter> logEntries1, IReadOnlyList<IQuestLogEntryGetter> logEntries2)
        {
            if (logEntries1.Count != logEntries2.Count) return false;

            for (int i = 0; i < logEntries1.Count; i++)
            {
                if (!AreLogEntriesEqual(logEntries1[i], logEntries2[i])) return false;
            }
            return true;
        }

        private bool AreLogEntriesEqual(IQuestLogEntryGetter logEntry1, IQuestLogEntryGetter logEntry2)
        {
            if (logEntry1.Flags != logEntry2.Flags) return false;
            if (!AreConditionsEqual(logEntry1.Conditions, logEntry2.Conditions)) return false;
            if (!AreTranslatedStringsEqual(logEntry1.Entry, logEntry2.Entry)) return false;
            if (logEntry1.NextQuest.FormKey != logEntry2.NextQuest.FormKey) return false;
            if (!AreByteSlicesEqual(logEntry1.SCHR, logEntry2.SCHR)) return false;
            if (!AreByteSlicesEqual(logEntry1.SCTX, logEntry2.SCTX)) return false;
            if (!AreByteSlicesEqual(logEntry1.QNAM, logEntry2.QNAM)) return false;
            return true;
        }

        private bool AreTranslatedStringsEqual(ITranslatedStringGetter? text1, ITranslatedStringGetter? text2)
        {
            if (text1 == null && text2 == null) return true;
            if (text1 == null || text2 == null) return false;
            return StringComparisonHelper.EqualsNormalized(text1.String, text2.String);
        }

        private bool AreByteSlicesEqual(ReadOnlyMemorySlice<byte>? slice1, ReadOnlyMemorySlice<byte>? slice2)
        {
            if (!slice1.HasValue && !slice2.HasValue) return true;
            if (!slice1.HasValue || !slice2.HasValue) return false;
            if (slice1.Value.Length != slice2.Value.Length) return false;
            return slice1.Value.Span.SequenceEqual(slice2.Value.Span);
        }

        private bool AreConditionsEqual(IReadOnlyList<IConditionGetter> conditions1, IReadOnlyList<IConditionGetter> conditions2)
        {
            if (conditions1.Count != conditions2.Count) return false;

            // xEdit represents CTDA as an ordered array. Reordering can change OR-group semantics.
            for (int i = 0; i < conditions1.Count; i++)
            {
                if (!AreConditionsEqual(conditions1[i], conditions2[i])) return false;
            }
            return true;
        }

        private bool AreConditionsEqual(IConditionGetter condition1, IConditionGetter condition2)
        {
            return ConditionMixIn.Equals(condition1, condition2);
        }
    }
}

using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;
using System.Linq;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class StagesHandler : AbstractListPropertyHandler<IQuestStageGetter>
    {
        public override string PropertyName => "Stages";

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

                if (!HasPermissionsToModify(recordMod, forwardContext.OwnerMod))
                    continue;

                if (!AreStagesEqual(recordStage, forwardContext.Value))
                {
                    forwardContext.Value = recordStage.DeepCopy();
                    forwardContext.OwnerMod = context.ModKey.ToString();
                    LogCollector.Add(PropertyName, $"[{PropertyName}] Updating stage Index {recordStage.Index} (taking ownership as '{context.ModKey}')");
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
            return text1.String == text2.String;
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

            // Sort conditions by a consistent key for comparison (order-independent)
            var sorted1 = conditions1.OrderBy(c =>
            {
                try
                {
                    var formKey = c.Data.Reference.FormKey.IsNull ? FormKey.Null : c.Data.Reference.FormKey;
                    return $"{c.Data.Function}_{c.CompareOperator}_{formKey}";
                }
                catch
                {
                    return $"{c.Data.Function}_{c.CompareOperator}_{FormKey.Null}";
                }
            }).ToList();
            var sorted2 = conditions2.OrderBy(c =>
            {
                try
                {
                    var formKey = c.Data.Reference.FormKey.IsNull ? FormKey.Null : c.Data.Reference.FormKey;
                    return $"{c.Data.Function}_{c.CompareOperator}_{formKey}";
                }
                catch
                {
                    return $"{c.Data.Function}_{c.CompareOperator}_{FormKey.Null}";
                }
            }).ToList();

            for (int i = 0; i < sorted1.Count; i++)
            {
                if (!AreConditionsEqual(sorted1[i], sorted2[i])) return false;
            }
            return true;
        }

        private bool AreConditionsEqual(IConditionGetter condition1, IConditionGetter condition2)
        {
            // Compare all condition properties directly
            if (condition1.CompareOperator != condition2.CompareOperator) return false;
            if (condition1.Flags != condition2.Flags) return false;
            if (!condition1.Unknown1.Span.SequenceEqual(condition2.Unknown1.Span)) return false;
            if (condition1.Unknown2 != condition2.Unknown2) return false;

            // Compare condition data properties
            var data1 = condition1.Data;
            var data2 = condition2.Data;

            if (data1.Function != data2.Function) return false;
            if (data1.RunOnType != data2.RunOnType) return false;
            if (data1.RunOnTypeIndex != data2.RunOnTypeIndex) return false;
            if (data1.UseAliases != data2.UseAliases) return false;
            if (data1.UsePackageData != data2.UsePackageData) return false;

            // Compare the Reference property
            if (data1.Reference.FormKey != data2.Reference.FormKey) return false;

            // For specific condition types that need special comparison logic
            if (data1 is IGetStageDoneConditionDataGetter stageData1 && data2 is IGetStageDoneConditionDataGetter stageData2)
            {
                // For GetStageDone conditions, compare quest and stage
                if (stageData1.Quest.Link.FormKey != stageData2.Quest.Link.FormKey) return false;
                if (stageData1.Stage != stageData2.Stage) return false;
            }
            else
            {
                // For all other condition types, compare using reflection-based approach
                if (!CompareConditionDataProperties(data1, data2)) return false;
            }

            return true;
        }

        /// <summary>
        /// Compare condition data properties using reflection-based comparison like AbstractConditionsHandler
        /// </summary>
        private bool CompareConditionDataProperties(IConditionDataGetter data1, IConditionDataGetter data2)
        {
            // Use the same reflection-based approach as AbstractConditionsHandler
            return CompareConditionReferences(data1, data2);
        }

        /// <summary>
        /// Generic method to compare reference information between two condition data objects using reflection.
        /// This is copied from AbstractConditionsHandler to ensure consistency.
        /// </summary>
        private bool CompareConditionReferences(IConditionDataGetter data1, IConditionDataGetter data2)
        {
            // Get all properties from both objects
            var props1 = data1.GetType().GetProperties();
            var props2 = data2.GetType().GetProperties();

            // Compare properties that are not already compared above
            foreach (var prop1 in props1)
            {
                if (prop1.Name == "Function" || prop1.Name == "RunOnType" || prop1.Name == "RunOnTypeIndex" ||
                    prop1.Name == "UseAliases" || prop1.Name == "UsePackageData" || prop1.Name == "Reference")
                {
                    continue; // Already compared
                }

                var prop2 = props2.FirstOrDefault(p => p.Name == prop1.Name);
                if (prop2 == null) continue;

                var val1 = prop1.GetValue(data1);
                var val2 = prop2.GetValue(data2);

                if (!Equals(val1, val2)) return false;
            }

            return true;
        }
    }
}

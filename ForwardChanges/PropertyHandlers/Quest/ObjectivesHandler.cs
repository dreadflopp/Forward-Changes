using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;
using System.Linq;
using System.Reflection;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class ObjectivesHandler : AbstractListPropertyHandler<IQuestObjectiveGetter>
    {
        public override string PropertyName => "Objectives";

        public override List<IQuestObjectiveGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IQuestGetter questRecord)
            {
                return questRecord.Objectives?.ToList();
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IQuestObjectiveGetter>? value)
        {
            if (record is IQuest questRecord && value != null)
            {
                if (questRecord.Objectives != null)
                {
                    questRecord.Objectives.Clear();
                    foreach (var objective in value)
                    {
                        if (objective is QuestObjective concreteObjective)
                        {
                            questRecord.Objectives.Add(concreteObjective);
                        }
                        else
                        {
                            // Convert IQuestObjectiveGetter to QuestObjective
                            var newObjective = objective.DeepCopy();
                            questRecord.Objectives.Add(newObjective);
                        }
                    }
                }
            }
        }

        public override bool AreValuesEqual(List<IQuestObjectiveGetter>? value1, List<IQuestObjectiveGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            // Group objectives by Index for efficient matching (order-independent)
            var objectives1ByIndex = value1
                .Where(o => o != null)
                .GroupBy(o => o.Index)
                .ToDictionary(g => g.Key, g => g.First());

            var objectives2ByIndex = value2
                .Where(o => o != null)
                .GroupBy(o => o.Index)
                .ToDictionary(g => g.Key, g => g.First());

            // Check that all objectives in value1 have matching objectives in value2 with same properties
            foreach (var objective1 in objectives1ByIndex.Values)
            {
                if (!objectives2ByIndex.TryGetValue(objective1.Index, out var objective2))
                {
                    return false; // Objective Index not found in value2
                }

                // Compare objectives by all properties (for forwarding decision)
                if (!AreObjectivesEqual(objective1, objective2))
                {
                    return false; // Objectives differ
                }
            }

            return true;
        }

        protected override bool IsItemEqual(IQuestObjectiveGetter? item1, IQuestObjectiveGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Match objectives by Index only - properties are compared separately in AreValuesEqual
            return item1.Index == item2.Index;
        }

        private bool AreObjectivesEqual(IQuestObjectiveGetter objective1, IQuestObjectiveGetter objective2)
        {
            // Compare all quest objective properties for content-based equality
            if (objective1.Index != objective2.Index) return false;
            if (objective1.Flags != objective2.Flags) return false;
            if (!AreTranslatedStringsEqual(objective1.DisplayText, objective2.DisplayText)) return false;

            // Compare targets by content
            if (!AreObjectiveTargetsEqual(objective1.Targets, objective2.Targets)) return false;

            return true;
        }

        protected override string FormatItem(IQuestObjectiveGetter? item)
        {
            return item != null ? $"Index:{item.Index}, Flags:{item.Flags}, DisplayText:{item.DisplayText?.String ?? "null"}, Targets:{item.Targets.Count}" : "null";
        }

        protected override void ProcessHandlerSpecificLogic(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            ListPropertyContext<IQuestObjectiveGetter> listPropertyContext,
            List<IQuestObjectiveGetter> recordItems,
            List<ListPropertyValueContext<IQuestObjectiveGetter>> currentForwardItems)
        {
            var recordMod = state.LoadOrder[context.ModKey].Mod;
            if (recordMod == null) return;

            var forwardByIndex = currentForwardItems
                .Where(i => !i.IsRemoved)
                .GroupBy(i => i.Value.Index)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var recordObj in recordItems)
            {
                if (recordObj == null) continue;

                if (!forwardByIndex.TryGetValue(recordObj.Index, out var forwardContext))
                    continue;

                if (!HasPermissionsToModify(recordMod, forwardContext.OwnerMod))
                    continue;

                if (!AreObjectivesEqual(recordObj, forwardContext.Value))
                {
                    forwardContext.Value = recordObj.DeepCopy();
                    forwardContext.OwnerMod = context.ModKey.ToString();
                    LogCollector.Add(PropertyName, $"[{PropertyName}] Updating objective Index {recordObj.Index} (taking ownership as '{context.ModKey}')");
                }
            }
        }

        private bool AreTranslatedStringsEqual(ITranslatedStringGetter? text1, ITranslatedStringGetter? text2)
        {
            if (text1 == null && text2 == null) return true;
            if (text1 == null || text2 == null) return false;
            return text1.String == text2.String;
        }

        private bool AreObjectiveTargetsEqual(IReadOnlyList<IQuestObjectiveTargetGetter> targets1, IReadOnlyList<IQuestObjectiveTargetGetter> targets2)
        {
            if (targets1.Count != targets2.Count) return false;

            for (int i = 0; i < targets1.Count; i++)
            {
                if (!AreObjectiveTargetsEqual(targets1[i], targets2[i])) return false;
            }
            return true;
        }

        private bool AreObjectiveTargetsEqual(IQuestObjectiveTargetGetter target1, IQuestObjectiveTargetGetter target2)
        {
            // Compare all quest objective target properties
            if (target1.AliasID != target2.AliasID) return false;
            if (target1.Flags != target2.Flags) return false;

            // Compare conditions by content
            if (!AreConditionsEqual(target1.Conditions, target2.Conditions)) return false;

            return true;
        }

        private bool AreConditionsEqual(IReadOnlyList<IConditionGetter> conditions1, IReadOnlyList<IConditionGetter> conditions2)
        {
            if (conditions1.Count != conditions2.Count) return false;

            var sorted1 = conditions1.OrderBy(c =>
            {
                try
                {
                    var formKey = c.Data.Reference.FormKey.IsNull ? FormKey.Null : c.Data.Reference.FormKey;
                    return $"{c.Data.Function}_{c.CompareOperator}_{formKey}";
                }
                catch { return $"{c.Data.Function}_{c.CompareOperator}_{FormKey.Null}"; }
            }).ToList();
            var sorted2 = conditions2.OrderBy(c =>
            {
                try
                {
                    var formKey = c.Data.Reference.FormKey.IsNull ? FormKey.Null : c.Data.Reference.FormKey;
                    return $"{c.Data.Function}_{c.CompareOperator}_{formKey}";
                }
                catch { return $"{c.Data.Function}_{c.CompareOperator}_{FormKey.Null}"; }
            }).ToList();

            for (int i = 0; i < sorted1.Count; i++)
            {
                if (!AreConditionsEqual(sorted1[i], sorted2[i])) return false;
            }
            return true;
        }

        private bool AreConditionsEqual(IConditionGetter condition1, IConditionGetter condition2)
        {
            if (condition1.CompareOperator != condition2.CompareOperator) return false;
            if (condition1.Flags != condition2.Flags) return false;
            if (!condition1.Unknown1.Span.SequenceEqual(condition2.Unknown1.Span)) return false;
            if (condition1.Unknown2 != condition2.Unknown2) return false;

            var data1 = condition1.Data;
            var data2 = condition2.Data;

            if (data1.Function != data2.Function) return false;
            if (data1.RunOnType != data2.RunOnType) return false;
            if (data1.RunOnTypeIndex != data2.RunOnTypeIndex) return false;
            if (data1.UseAliases != data2.UseAliases) return false;
            if (data1.UsePackageData != data2.UsePackageData) return false;
            if (data1.Reference.FormKey != data2.Reference.FormKey) return false;

            if (data1 is IGetStageDoneConditionDataGetter stageData1 && data2 is IGetStageDoneConditionDataGetter stageData2)
            {
                if (stageData1.Quest.Link.FormKey != stageData2.Quest.Link.FormKey) return false;
                if (stageData1.Stage != stageData2.Stage) return false;
            }
            else
            {
                if (!CompareConditionDataProperties(data1, data2)) return false;
            }

            return true;
        }

        private bool CompareConditionDataProperties(IConditionDataGetter data1, IConditionDataGetter data2)
        {
            return CompareConditionReferences(data1, data2);
        }

        private bool CompareConditionReferences(IConditionDataGetter data1, IConditionDataGetter data2)
        {
            var props1 = data1.GetType().GetProperties();
            var props2 = data2.GetType().GetProperties();

            foreach (var prop1 in props1)
            {
                if (prop1.Name == "Function" || prop1.Name == "RunOnType" || prop1.Name == "RunOnTypeIndex" ||
                    prop1.Name == "UseAliases" || prop1.Name == "UsePackageData" || prop1.Name == "Reference")
                    continue;

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

using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Synthesis;
using DreadsMashedPatch;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.Contexts;
using System.Linq;

namespace DreadsMashedPatch.PropertyHandlers.Quest
{
    public class ObjectivesHandler : AbstractListPropertyHandler<IQuestObjectiveGetter>
    {
        public override string PropertyName => "Objectives";
        public override ListSemantics Semantics => ListSemantics.SortedKeyed;

        protected override bool IsItemIdentityEqual(IQuestObjectiveGetter? left, IQuestObjectiveGetter? right) =>
            left?.Index == right?.Index;

        protected override IReadOnlyList<object?> GetSortKey(IQuestObjectiveGetter item) => [item.Index];

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
            return StringComparisonHelper.EqualsNormalized(text1.String, text2.String);
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

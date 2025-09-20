using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.Abstracts;

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

            for (int i = 0; i < value1.Count; i++)
            {
                if (!IsItemEqual(value1[i], value2[i])) return false;
            }
            return true;
        }

        protected override bool IsItemEqual(IQuestObjectiveGetter? item1, IQuestObjectiveGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare all quest objective properties for content-based equality
            if (item1.Index != item2.Index) return false;
            if (item1.Flags != item2.Flags) return false;
            if (!AreTranslatedStringsEqual(item1.DisplayText, item2.DisplayText)) return false;

            // Compare targets by content
            if (!AreObjectiveTargetsEqual(item1.Targets, item2.Targets)) return false;

            return true;
        }

        protected override string FormatItem(IQuestObjectiveGetter? item)
        {
            return item != null ? $"Index:{item.Index}, Flags:{item.Flags}, DisplayText:{item.DisplayText?.String ?? "null"}, Targets:{item.Targets.Count}" : "null";
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

            for (int i = 0; i < conditions1.Count; i++)
            {
                if (!AreConditionsEqual(conditions1[i], conditions2[i])) return false;
            }
            return true;
        }

        private bool AreConditionsEqual(IConditionGetter condition1, IConditionGetter condition2)
        {
            // For now, use a simple approach - in practice you might want more detailed comparison
            // This could be enhanced based on the specific properties of IConditionGetter
            return condition1.Equals(condition2);
        }
    }
}

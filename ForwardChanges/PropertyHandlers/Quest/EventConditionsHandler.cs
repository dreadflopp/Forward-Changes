using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class EventConditionsHandler : AbstractConditionsHandler<IQuestGetter, IQuest>
    {
        protected override IEnumerable<IConditionGetter>? GetConditions(IQuestGetter record)
        {
            return record.EventConditions;
        }

        protected override IEnumerable<IConditionGetter>? GetConditions(IQuest record)
        {
            return record.EventConditions;
        }

        protected override void UpdateConditionsCollection(IQuest record, List<IConditionGetter> conditions)
        {
            // Clear the existing conditions and add the new ones
            if (record.EventConditions != null)
            {
                record.EventConditions.Clear();
                foreach (var condition in conditions)
                {
                    if (condition == null) continue;

                    if (condition is Condition concreteCondition)
                    {
                        record.EventConditions.Add(concreteCondition);
                    }
                    else
                    {
                        // Convert IConditionGetter to Condition
                        var newCondition = condition.DeepCopy();
                        record.EventConditions.Add(newCondition);
                    }
                }
            }
        }
    }
}

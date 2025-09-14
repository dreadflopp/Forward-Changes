using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class ConditionsHandler : AbstractConditionsHandler<IDialogResponsesGetter, IDialogResponses>
    {
        protected override IEnumerable<IConditionGetter>? GetConditions(IDialogResponsesGetter record)
        {
            return record.Conditions;
        }

        protected override IEnumerable<IConditionGetter>? GetConditions(IDialogResponses record)
        {
            return record.Conditions;
        }

        protected override void UpdateConditionsCollection(IDialogResponses record, List<IConditionGetter> conditions)
        {
            // Clear the existing conditions and add the new ones
            if (record.Conditions != null)
            {
                record.Conditions.Clear();
                foreach (var condition in conditions)
                {
                    if (condition == null) continue;

                    if (condition is Condition concreteCondition)
                    {
                        record.Conditions.Add(concreteCondition);
                    }
                    else
                    {
                        // Convert IConditionGetter to Condition
                        var newCondition = condition.DeepCopy();
                        record.Conditions.Add(newCondition);
                    }
                }
            }
        }
    }
}


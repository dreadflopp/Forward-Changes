using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class ConditionsHandler : AbstractConditionsHandler<IMagicEffectGetter, IMagicEffect>
    {
        protected override IEnumerable<IConditionGetter>? GetConditions(IMagicEffectGetter record)
        {
            return record.Conditions;
        }

        protected override IEnumerable<IConditionGetter>? GetConditions(IMagicEffect record)
        {
            return record.Conditions;
        }

        protected override void UpdateConditionsCollection(IMagicEffect record, List<IConditionGetter> conditions)
        {
            // Clear the existing conditions and add the new ones
            if (record.Conditions != null)
            {
                record.Conditions.Clear();
                for (int i = 0; i < conditions.Count; i++)
                {
                    var condition = conditions[i];
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

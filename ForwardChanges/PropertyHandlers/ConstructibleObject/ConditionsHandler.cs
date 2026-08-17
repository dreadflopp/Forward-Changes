using System.Collections.Generic;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.ConstructibleObject
{
    public class ConditionsHandler : AbstractConditionsHandler<IConstructibleObjectGetter, IConstructibleObject>
    {
        protected override IEnumerable<IConditionGetter>? GetConditions(IConstructibleObjectGetter record)
        {
            return record.Conditions;
        }

        protected override IEnumerable<IConditionGetter>? GetConditions(IConstructibleObject record)
        {
            return record.Conditions;
        }

        protected override void UpdateConditionsCollection(IConstructibleObject record, List<IConditionGetter> conditions)
        {
            record.Conditions.Clear();
            foreach (var condition in conditions)
            {
                if (condition == null) continue;
                record.Conditions.Add(condition.DeepCopy());
            }
        }
    }
}

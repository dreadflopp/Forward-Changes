using System.Collections.Generic;
using Mutagen.Bethesda.Skyrim;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.CameraPath
{
    public class ConditionsHandler : AbstractConditionsHandler<ICameraPathGetter, ICameraPath>
    {
        protected override IEnumerable<IConditionGetter>? GetConditions(ICameraPathGetter record)
        {
            return record.Conditions;
        }

        protected override IEnumerable<IConditionGetter>? GetConditions(ICameraPath record)
        {
            return record.Conditions;
        }

        protected override void UpdateConditionsCollection(ICameraPath record, List<IConditionGetter> conditions)
        {
            if (record.Conditions == null)
            {
                return;
            }

            record.Conditions.Clear();
            foreach (var condition in conditions)
            {
                if (condition == null) continue;
                record.Conditions.Add(condition.DeepCopy());
            }
        }
    }
}

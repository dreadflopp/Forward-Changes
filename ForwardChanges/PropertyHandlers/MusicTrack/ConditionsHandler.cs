using System.Collections.Generic;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.MusicTrack
{
    public class ConditionsHandler : AbstractConditionsHandler<IMusicTrackGetter, IMusicTrack>
    {
        protected override IEnumerable<IConditionGetter>? GetConditions(IMusicTrackGetter record)
        {
            return record.Conditions;
        }

        protected override IEnumerable<IConditionGetter>? GetConditions(IMusicTrack record)
        {
            return record.Conditions;
        }

        protected override void UpdateConditionsCollection(IMusicTrack record, List<IConditionGetter> conditions)
        {
            if (record.Conditions == null)
            {
                return;
            }

            record.Conditions.Clear();
            foreach (var condition in conditions)
            {
                if (condition == null)
                {
                    continue;
                }

                record.Conditions.Add(condition.DeepCopy());
            }
        }
    }
}
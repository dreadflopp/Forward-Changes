using System.Collections.Generic;
using Mutagen.Bethesda.Skyrim;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.MusicTrack
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
            record.Conditions ??= [];
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

        protected override void SetConditionsNull(IMusicTrack record)
        {
            record.Conditions = null;
        }
    }
}

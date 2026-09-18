using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class ConditionsHandler : AbstractConditionsHandler<IFactionGetter, IFaction>
    {
        protected override IEnumerable<IConditionGetter>? GetConditions(IFactionGetter record)
        {
            return record.Conditions;
        }

        protected override IEnumerable<IConditionGetter>? GetConditions(IFaction record)
        {
            return record.Conditions;
        }

        protected override void UpdateConditionsCollection(IFaction record, List<IConditionGetter> conditions)
        {
            record.Conditions ??= [];
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
                    record.Conditions.Add(condition.DeepCopy());
                }
            }
        }

        protected override void SetConditionsNull(IFaction record)
        {
            record.Conditions = null;
        }
    }
}

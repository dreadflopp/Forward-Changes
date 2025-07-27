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

        protected override ICollection<IConditionGetter>? GetConditions(IFaction record)
        {
            return record.Conditions as ICollection<IConditionGetter>;
        }
    }
}


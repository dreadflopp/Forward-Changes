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

        protected override ICollection<IConditionGetter>? GetConditions(IDialogResponses record)
        {
            return record.Conditions as ICollection<IConditionGetter>;
        }
    }
}


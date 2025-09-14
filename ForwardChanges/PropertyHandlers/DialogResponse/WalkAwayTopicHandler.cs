using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class WalkAwayTopicHandler : AbstractFormLinkPropertyHandler<IDialogResponses, IDialogResponsesGetter, IDialogTopicGetter>
    {
        public override string PropertyName => "WalkAwayTopic";

        protected override IFormLinkNullableGetter<IDialogTopicGetter> GetFormLinkValue(IDialogResponsesGetter record)
        {
            return record.WalkAwayTopic;
        }

        protected override void SetFormLinkValue(IDialogResponses record, IFormLinkNullableGetter<IDialogTopicGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.WalkAwayTopic = new FormLinkNullable<IDialogTopicGetter>(value.FormKey);
            }
            else
            {
                record.WalkAwayTopic.Clear();
            }
        }
    }
}
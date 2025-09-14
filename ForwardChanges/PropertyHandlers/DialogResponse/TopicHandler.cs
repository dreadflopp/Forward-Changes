using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class TopicHandler : AbstractFormLinkPropertyHandler<IDialogResponses, IDialogResponsesGetter, IDialogTopicGetter>
    {
        public override string PropertyName => "Topic";

        protected override IFormLinkNullableGetter<IDialogTopicGetter> GetFormLinkValue(IDialogResponsesGetter record)
        {
            return record.Topic;
        }

        protected override void SetFormLinkValue(IDialogResponses record, IFormLinkNullableGetter<IDialogTopicGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Topic = new FormLinkNullable<IDialogTopicGetter>(value.FormKey);
            }
            else
            {
                record.Topic.Clear();
            }
        }
    }
}
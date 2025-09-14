using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogTopic
{
    public class QuestHandler : AbstractFormLinkPropertyHandler<IDialogTopic, IDialogTopicGetter, IQuestGetter>
    {
        public override string PropertyName => "Quest";

        protected override IFormLinkNullableGetter<IQuestGetter> GetFormLinkValue(IDialogTopicGetter record)
        {
            return record.Quest;
        }

        protected override void SetFormLinkValue(IDialogTopic record, IFormLinkNullableGetter<IQuestGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Quest = new FormLinkNullable<IQuestGetter>(value.FormKey);
            }
            else
            {
                record.Quest.Clear();
            }
        }
    }
}
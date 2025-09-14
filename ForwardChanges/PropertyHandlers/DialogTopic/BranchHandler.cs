using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogTopic
{
    public class BranchHandler : AbstractFormLinkPropertyHandler<IDialogTopic, IDialogTopicGetter, IDialogBranchGetter>
    {
        public override string PropertyName => "Branch";

        protected override IFormLinkNullableGetter<IDialogBranchGetter> GetFormLinkValue(IDialogTopicGetter record)
        {
            return record.Branch;
        }

        protected override void SetFormLinkValue(IDialogTopic record, IFormLinkNullableGetter<IDialogBranchGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Branch = new FormLinkNullable<IDialogBranchGetter>(value.FormKey);
            }
            else
            {
                record.Branch.Clear();
            }
        }
    }
}
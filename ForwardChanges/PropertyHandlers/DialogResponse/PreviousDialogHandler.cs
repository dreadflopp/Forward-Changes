using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class PreviousDialogHandler : AbstractFormLinkPropertyHandler<IDialogResponses, IDialogResponsesGetter, IDialogResponsesGetter>
    {
        public override string PropertyName => "PreviousDialog";

        protected override IFormLinkNullableGetter<IDialogResponsesGetter> GetFormLinkValue(IDialogResponsesGetter record)
        {
            return record.PreviousDialog;
        }

        protected override void SetFormLinkValue(IDialogResponses record, IFormLinkNullableGetter<IDialogResponsesGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.PreviousDialog = new FormLinkNullable<IDialogResponsesGetter>(value.FormKey);
            }
            else
            {
                record.PreviousDialog.Clear();
            }
        }
    }
}
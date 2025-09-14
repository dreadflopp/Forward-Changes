using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class ResponseDataHandler : AbstractFormLinkPropertyHandler<IDialogResponses, IDialogResponsesGetter, IDialogResponsesGetter>
    {
        public override string PropertyName => "ResponseData";

        protected override IFormLinkNullableGetter<IDialogResponsesGetter> GetFormLinkValue(IDialogResponsesGetter record)
        {
            return record.ResponseData;
        }

        protected override void SetFormLinkValue(IDialogResponses record, IFormLinkNullableGetter<IDialogResponsesGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.ResponseData = new FormLinkNullable<IDialogResponsesGetter>(value.FormKey);
            }
            else
            {
                record.ResponseData.Clear();
            }
        }
    }
}
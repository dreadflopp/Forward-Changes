using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class SpeakerHandler : AbstractFormLinkPropertyHandler<IDialogResponses, IDialogResponsesGetter, INpcGetter>
    {
        public override string PropertyName => "Speaker";

        protected override IFormLinkNullableGetter<INpcGetter> GetFormLinkValue(IDialogResponsesGetter record)
        {
            return record.Speaker;
        }

        protected override void SetFormLinkValue(IDialogResponses record, IFormLinkNullableGetter<INpcGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Speaker = new FormLinkNullable<INpcGetter>(value.FormKey);
            }
            else
            {
                record.Speaker.Clear();
            }
        }
    }
}
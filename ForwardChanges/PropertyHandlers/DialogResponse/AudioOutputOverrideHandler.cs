using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class AudioOutputOverrideHandler : AbstractFormLinkPropertyHandler<IDialogResponses, IDialogResponsesGetter, ISoundOutputModelGetter>
    {
        public override string PropertyName => "AudioOutputOverride";

        protected override IFormLinkNullableGetter<ISoundOutputModelGetter> GetFormLinkValue(IDialogResponsesGetter record)
        {
            return record.AudioOutputOverride;
        }

        protected override void SetFormLinkValue(IDialogResponses record, IFormLinkNullableGetter<ISoundOutputModelGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.AudioOutputOverride = new FormLinkNullable<ISoundOutputModelGetter>(value.FormKey);
            }
            else
            {
                record.AudioOutputOverride.Clear();
            }
        }
    }
}
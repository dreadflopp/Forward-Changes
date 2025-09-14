using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class VoiceHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, IVoiceTypeGetter>
    {
        public override string PropertyName => "Voice";

        protected override IFormLinkNullableGetter<IVoiceTypeGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.Voice;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<IVoiceTypeGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Voice = new FormLinkNullable<IVoiceTypeGetter>(value.FormKey);
            }
            else
            {
                record.Voice.Clear();
            }
        }
    }
}
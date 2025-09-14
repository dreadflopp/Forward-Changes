using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Ingestible
{
    public class PutDownSoundHandler : AbstractFormLinkPropertyHandler<IIngestible, IIngestibleGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "PutDownSound";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetFormLinkValue(IIngestibleGetter record)
        {
            return record.PutDownSound;
        }

        protected override void SetFormLinkValue(IIngestible record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.PutDownSound = new FormLinkNullable<ISoundDescriptorGetter>(value.FormKey);
            }
            else
            {
                record.PutDownSound.Clear();
            }
        }
    }
}


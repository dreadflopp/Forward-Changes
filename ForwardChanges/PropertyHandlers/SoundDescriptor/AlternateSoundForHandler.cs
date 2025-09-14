using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class AlternateSoundForHandler : AbstractFormLinkPropertyHandler<ISoundDescriptor, ISoundDescriptorGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "AlternateSoundFor";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetFormLinkValue(ISoundDescriptorGetter record)
        {
            return record.AlternateSoundFor;
        }

        protected override void SetFormLinkValue(ISoundDescriptor record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.AlternateSoundFor = new FormLinkNullable<ISoundDescriptorGetter>(value.FormKey);
            }
            else
            {
                record.AlternateSoundFor.Clear();
            }
        }
    }
}
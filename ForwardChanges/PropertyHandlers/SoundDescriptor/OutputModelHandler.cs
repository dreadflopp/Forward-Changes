using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class OutputModelHandler : AbstractFormLinkPropertyHandler<ISoundDescriptor, ISoundDescriptorGetter, ISoundOutputModelGetter>
    {
        public override string PropertyName => "OutputModel";

        protected override IFormLinkNullableGetter<ISoundOutputModelGetter>? GetFormLinkValue(ISoundDescriptorGetter record)
        {
            return record.OutputModel;
        }

        protected override void SetFormLinkValue(ISoundDescriptor record, IFormLinkNullableGetter<ISoundOutputModelGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.OutputModel = new FormLinkNullable<ISoundOutputModelGetter>(value.FormKey);
            }
            else
            {
                record.OutputModel.Clear();
            }
        }
    }
}
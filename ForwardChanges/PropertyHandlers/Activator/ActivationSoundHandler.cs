using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Activator
{
    public class ActivationSoundHandler : AbstractFormLinkPropertyHandler<IActivator, IActivatorGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "ActivationSound";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetFormLinkValue(IActivatorGetter record)
        {
            return record.ActivationSound;
        }

        protected override void SetFormLinkValue(IActivator record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value != null)
            {
                record.ActivationSound.SetTo(value.FormKey);
            }
            else
            {
                record.ActivationSound.Clear();
            }
        }
    }
}

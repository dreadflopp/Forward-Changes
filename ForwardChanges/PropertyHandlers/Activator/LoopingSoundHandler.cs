using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Activator
{
    public class LoopingSoundHandler : AbstractFormLinkPropertyHandler<IActivator, IActivatorGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "LoopingSound";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetFormLinkValue(IActivatorGetter record)
        {
            return record.LoopingSound;
        }

        protected override void SetFormLinkValue(IActivator record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value != null)
            {
                record.LoopingSound.SetTo(value.FormKey);
            }
            else
            {
                record.LoopingSound.Clear();
            }
        }
    }
}

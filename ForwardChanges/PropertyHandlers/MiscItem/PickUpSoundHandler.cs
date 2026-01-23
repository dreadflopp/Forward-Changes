using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MiscItem
{
    public class PickUpSoundHandler : AbstractFormLinkPropertyHandler<IMiscItem, IMiscItemGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "PickUpSound";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetFormLinkValue(IMiscItemGetter record)
        {
            return record.PickUpSound;
        }

        protected override void SetFormLinkValue(IMiscItem record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.PickUpSound = new FormLinkNullable<ISoundDescriptorGetter>(value.FormKey);
            }
            else
            {
                record.PickUpSound.Clear();
            }
        }
    }
}

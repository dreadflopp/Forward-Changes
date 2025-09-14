using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    public class FootstepSoundHandler : AbstractFormLinkPropertyHandler<IArmorAddon, IArmorAddonGetter, IFootstepSetGetter>
    {
        public override string PropertyName => "FootstepSound";

        protected override IFormLinkNullableGetter<IFootstepSetGetter>? GetFormLinkValue(IArmorAddonGetter record)
        {
            return record.FootstepSound;
        }

        protected override void SetFormLinkValue(IArmorAddon record, IFormLinkNullableGetter<IFootstepSetGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.FootstepSound = new FormLinkNullable<IFootstepSetGetter>(value.FormKey);
            }
            else
            {
                record.FootstepSound.Clear();
            }
        }
    }
}
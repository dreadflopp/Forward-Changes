using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    public class ArtObjectHandler : AbstractFormLinkPropertyHandler<IArmorAddon, IArmorAddonGetter, IArtObjectGetter>
    {
        public override string PropertyName => "ArtObject";

        protected override IFormLinkNullableGetter<IArtObjectGetter>? GetFormLinkValue(IArmorAddonGetter record)
        {
            return record.ArtObject;
        }

        protected override void SetFormLinkValue(IArmorAddon record, IFormLinkNullableGetter<IArtObjectGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.ArtObject = new FormLinkNullable<IArtObjectGetter>(value.FormKey);
            }
            else
            {
                record.ArtObject.Clear();
            }
        }
    }
}
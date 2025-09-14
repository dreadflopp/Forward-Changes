using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    public class RaceHandler : AbstractFormLinkPropertyHandler<IArmorAddon, IArmorAddonGetter, IRaceGetter>
    {
        public override string PropertyName => "Race";

        protected override IFormLinkNullableGetter<IRaceGetter>? GetFormLinkValue(IArmorAddonGetter record)
        {
            return record.Race;
        }

        protected override void SetFormLinkValue(IArmorAddon record, IFormLinkNullableGetter<IRaceGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Race = new FormLinkNullable<IRaceGetter>(value.FormKey);
            }
            else
            {
                record.Race.Clear();
            }
        }
    }
}
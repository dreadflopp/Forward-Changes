using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class WornArmorHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, IArmorGetter>
    {
        public override string PropertyName => "WornArmor";

        protected override IFormLinkNullableGetter<IArmorGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.WornArmor;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<IArmorGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.WornArmor = new FormLinkNullable<IArmorGetter>(value.FormKey);
            }
            else
            {
                record.WornArmor.Clear();
            }
        }
    }
}
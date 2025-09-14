using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Plugins;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class CombatOverridePackageListHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, IFormListGetter>
    {
        public override string PropertyName => "CombatOverridePackageList";

        protected override IFormLinkNullableGetter<IFormListGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.CombatOverridePackageList;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<IFormListGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.CombatOverridePackageList = new FormLinkNullable<IFormListGetter>(value.FormKey);
            }
            else
            {
                record.CombatOverridePackageList.Clear();
            }
        }
    }
}

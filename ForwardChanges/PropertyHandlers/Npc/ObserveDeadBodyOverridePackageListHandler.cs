using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Plugins;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class ObserveDeadBodyOverridePackageListHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, IFormListGetter>
    {
        public override string PropertyName => "ObserveDeadBodyOverridePackageList";

        protected override IFormLinkNullableGetter<IFormListGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.ObserveDeadBodyOverridePackageList;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<IFormListGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.ObserveDeadBodyOverridePackageList = new FormLinkNullable<IFormListGetter>(value.FormKey);
            }
            else
            {
                record.ObserveDeadBodyOverridePackageList.Clear();
            }
        }
    }
}

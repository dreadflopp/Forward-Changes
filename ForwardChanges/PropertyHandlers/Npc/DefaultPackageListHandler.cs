using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class DefaultPackageListHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, IFormListGetter>
    {
        public override string PropertyName => "DefaultPackageList";

        protected override IFormLinkNullableGetter<IFormListGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.DefaultPackageList;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<IFormListGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.DefaultPackageList = new FormLinkNullable<IFormListGetter>(value.FormKey);
            }
            else
            {
                record.DefaultPackageList.Clear();
            }
        }
    }
}
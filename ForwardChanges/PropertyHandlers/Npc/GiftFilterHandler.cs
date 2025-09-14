using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class GiftFilterHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, IFormListGetter>
    {
        public override string PropertyName => "GiftFilter";

        protected override IFormLinkNullableGetter<IFormListGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.GiftFilter;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<IFormListGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.GiftFilter = new FormLinkNullable<IFormListGetter>(value.FormKey);
            }
            else
            {
                record.GiftFilter.Clear();
            }
        }
    }
}
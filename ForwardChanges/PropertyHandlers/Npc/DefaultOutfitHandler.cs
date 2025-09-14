using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class DefaultOutfitHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, IOutfitGetter>
    {
        public override string PropertyName => "DefaultOutfit";

        protected override IFormLinkNullableGetter<IOutfitGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.DefaultOutfit;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<IOutfitGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.DefaultOutfit = new FormLinkNullable<IOutfitGetter>(value.FormKey);
            }
            else
            {
                record.DefaultOutfit.Clear();
            }
        }
    }
}
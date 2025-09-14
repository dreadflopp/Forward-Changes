using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class SleepingOutfitHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, IOutfitGetter>
    {
        public override string PropertyName => "SleepingOutfit";

        protected override IFormLinkNullableGetter<IOutfitGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.SleepingOutfit;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<IOutfitGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.SleepingOutfit = new FormLinkNullable<IOutfitGetter>(value.FormKey);
            }
            else
            {
                record.SleepingOutfit.Clear();
            }
        }
    }
}
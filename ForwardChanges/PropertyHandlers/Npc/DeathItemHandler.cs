using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class DeathItemHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, ILeveledItemGetter>
    {
        public override string PropertyName => "DeathItem";

        protected override IFormLinkNullableGetter<ILeveledItemGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.DeathItem as IFormLinkNullableGetter<ILeveledItemGetter>;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<ILeveledItemGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.DeathItem = new FormLinkNullable<ILeveledItemGetter>(value.FormKey);
            }
            else
            {
                record.DeathItem.Clear();
            }
        }
    }
}


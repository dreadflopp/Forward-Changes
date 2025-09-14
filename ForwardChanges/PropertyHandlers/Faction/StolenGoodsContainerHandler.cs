using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class StolenGoodsContainerHandler : AbstractFormLinkPropertyHandler<IFaction, IFactionGetter, IPlacedObjectGetter>
    {
        public override string PropertyName => "StolenGoodsContainer";

        protected override IFormLinkNullableGetter<IPlacedObjectGetter>? GetFormLinkValue(IFactionGetter record)
        {
            return record.StolenGoodsContainer as IFormLinkNullableGetter<IPlacedObjectGetter>;
        }

        protected override void SetFormLinkValue(IFaction record, IFormLinkNullableGetter<IPlacedObjectGetter>? value)
        {
            record.StolenGoodsContainer = value != null ? new FormLinkNullable<IPlacedObjectGetter>(value.FormKey) : new FormLinkNullable<IPlacedObjectGetter>();
        }
    }
}


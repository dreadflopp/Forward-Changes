using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class VendorBuySellHandler : AbstractFormLinkPropertyHandler<IFaction, IFactionGetter, IFormListGetter>
    {
        public override string PropertyName => "VendorBuySellList";

        protected override IFormLinkNullableGetter<IFormListGetter>? GetFormLinkValue(IFactionGetter record)
        {
            return record.VendorBuySellList as IFormLinkNullableGetter<IFormListGetter>;
        }

        protected override void SetFormLinkValue(IFaction record, IFormLinkNullableGetter<IFormListGetter>? value)
        {
            record.VendorBuySellList = value != null ? new FormLinkNullable<IFormListGetter>(value.FormKey) : new FormLinkNullable<IFormListGetter>();
        }
    }
}


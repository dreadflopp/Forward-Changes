using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class JailOutfitHandler : AbstractFormLinkPropertyHandler<IFaction, IFactionGetter, IOutfitGetter>
    {
        public override string PropertyName => "JailOutfit";

        protected override IFormLinkNullableGetter<IOutfitGetter>? GetFormLinkValue(IFactionGetter record)
        {
            return record.JailOutfit as IFormLinkNullableGetter<IOutfitGetter>;
        }

        protected override void SetFormLinkValue(IFaction record, IFormLinkNullableGetter<IOutfitGetter>? value)
        {
            record.JailOutfit = value != null ? new FormLinkNullable<IOutfitGetter>(value.FormKey) : new FormLinkNullable<IOutfitGetter>();
        }
    }
}


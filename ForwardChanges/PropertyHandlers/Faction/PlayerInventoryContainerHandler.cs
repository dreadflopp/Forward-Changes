using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class PlayerInventoryContainerHandler : AbstractFormLinkPropertyHandler<IFaction, IFactionGetter, IPlacedObjectGetter>
    {
        public override string PropertyName => "PlayerInventoryContainer";

        protected override IFormLinkNullableGetter<IPlacedObjectGetter>? GetFormLinkValue(IFactionGetter record)
        {
            return record.PlayerInventoryContainer as IFormLinkNullableGetter<IPlacedObjectGetter>;
        }

        protected override void SetFormLinkValue(IFaction record, IFormLinkNullableGetter<IPlacedObjectGetter>? value)
        {
            record.PlayerInventoryContainer = value != null ? new FormLinkNullable<IPlacedObjectGetter>(value.FormKey) : new FormLinkNullable<IPlacedObjectGetter>();
        }
    }
}


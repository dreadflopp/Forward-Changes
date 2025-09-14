using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class WorldLocationMarkerRefHandler : AbstractFormLinkPropertyHandler<ILocation, ILocationGetter, IPlacedSimpleGetter>
    {
        public override string PropertyName => "WorldLocationMarkerRef";

        protected override IFormLinkNullableGetter<IPlacedSimpleGetter>? GetFormLinkValue(ILocationGetter record)
        {
            return record.WorldLocationMarkerRef as IFormLinkNullableGetter<IPlacedSimpleGetter>;
        }

        protected override void SetFormLinkValue(ILocation record, IFormLinkNullableGetter<IPlacedSimpleGetter>? value)
        {
            record.WorldLocationMarkerRef = value != null ? new FormLinkNullable<IPlacedSimpleGetter>(value.FormKey) : new FormLinkNullable<IPlacedSimpleGetter>();
        }
    }
}


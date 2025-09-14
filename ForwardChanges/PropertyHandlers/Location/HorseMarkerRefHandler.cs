using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class HorseMarkerRefHandler : AbstractFormLinkPropertyHandler<ILocation, ILocationGetter, IPlacedObjectGetter>
    {
        public override string PropertyName => "HorseMarkerRef";

        protected override IFormLinkNullableGetter<IPlacedObjectGetter>? GetFormLinkValue(ILocationGetter record)
        {
            return record.HorseMarkerRef as IFormLinkNullableGetter<IPlacedObjectGetter>;
        }

        protected override void SetFormLinkValue(ILocation record, IFormLinkNullableGetter<IPlacedObjectGetter>? value)
        {
            record.HorseMarkerRef = value != null ? new FormLinkNullable<IPlacedObjectGetter>(value.FormKey) : new FormLinkNullable<IPlacedObjectGetter>();
        }
    }
}


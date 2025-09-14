using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.EncounterZone
{
    public class LocationHandler : AbstractFormLinkPropertyHandler<IEncounterZone, IEncounterZoneGetter, ILocationGetter>
    {
        public override string PropertyName => "Location";

        protected override IFormLinkNullableGetter<ILocationGetter>? GetFormLinkValue(IEncounterZoneGetter record)
        {
            return record.Location as IFormLinkNullableGetter<ILocationGetter>;
        }

        protected override void SetFormLinkValue(IEncounterZone record, IFormLinkNullableGetter<ILocationGetter>? value)
        {
            if (value != null)
            {
                record.Location.SetTo(value.FormKey);
            }
            else
            {
                record.Location.SetTo(null);
            }
        }
    }
}
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.EncounterZone
{
    public class OwnerHandler : AbstractFormLinkPropertyHandler<IEncounterZone, IEncounterZoneGetter, IOwnerGetter>
    {
        public override string PropertyName => "Owner";

        protected override IFormLinkNullableGetter<IOwnerGetter>? GetFormLinkValue(IEncounterZoneGetter record)
        {
            return record.Owner as IFormLinkNullableGetter<IOwnerGetter>;
        }

        protected override void SetFormLinkValue(IEncounterZone record, IFormLinkNullableGetter<IOwnerGetter>? value)
        {
            if (value != null)
            {
                record.Owner.SetTo(value.FormKey);
            }
            else
            {
                record.Owner.SetTo(null);
            }
        }
    }
}
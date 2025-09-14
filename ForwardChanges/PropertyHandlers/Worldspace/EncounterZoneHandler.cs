using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class EncounterZoneHandler : AbstractFormLinkPropertyHandler<IWorldspace, IWorldspaceGetter, IEncounterZoneGetter>
    {
        public override string PropertyName => "EncounterZone";

        protected override IFormLinkNullableGetter<IEncounterZoneGetter>? GetFormLinkValue(IWorldspaceGetter record)
        {
            return record.EncounterZone;
        }

        protected override void SetFormLinkValue(IWorldspace record, IFormLinkNullableGetter<IEncounterZoneGetter>? value)
        {
            if (value != null)
            {
                record.EncounterZone.SetTo(value.FormKey);
            }
            else
            {
                record.EncounterZone.SetTo(null);
            }
        }
    }
}
using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class EncounterZoneHandler : AbstractFormLinkPropertyHandler<IPlacedNpc, IPlacedNpcGetter, IEncounterZoneGetter>
    {
        public override string PropertyName => "EncounterZone";

        protected override IFormLinkNullableGetter<IEncounterZoneGetter> GetFormLinkValue(IPlacedNpcGetter record)
        {
            return record.EncounterZone;
        }

        protected override void SetFormLinkValue(IPlacedNpc record, IFormLinkNullableGetter<IEncounterZoneGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.EncounterZone = new FormLinkNullable<IEncounterZoneGetter>(value.FormKey);
            }
            else
            {
                record.EncounterZone.Clear();
            }
        }
    }
}
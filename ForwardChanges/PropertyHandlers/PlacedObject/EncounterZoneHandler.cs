using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class EncounterZoneHandler : AbstractFormLinkPropertyHandler<IPlacedObject, IPlacedObjectGetter, IEncounterZoneGetter>
    {
        public override string PropertyName => "EncounterZone";

        protected override IFormLinkNullableGetter<IEncounterZoneGetter> GetFormLinkValue(IPlacedObjectGetter record)
        {
            return record.EncounterZone;
        }

        protected override void SetFormLinkValue(IPlacedObject record, IFormLinkNullableGetter<IEncounterZoneGetter>? value)
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
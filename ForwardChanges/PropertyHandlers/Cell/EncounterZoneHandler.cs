using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class EncounterZoneHandler : AbstractFormLinkPropertyHandler<ICell, ICellGetter, IEncounterZoneGetter>
    {
        public override string PropertyName => "EncounterZone";

        protected override IFormLinkNullableGetter<IEncounterZoneGetter>? GetFormLinkValue(ICellGetter record)
        {
            return record.EncounterZone;
        }

        protected override void SetFormLinkValue(ICell record, IFormLinkNullableGetter<IEncounterZoneGetter>? value)
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


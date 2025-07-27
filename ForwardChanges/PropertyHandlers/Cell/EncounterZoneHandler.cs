using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class EncounterZoneHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IEncounterZoneGetter>>
    {
        public override string PropertyName => "EncounterZone";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IEncounterZoneGetter>? value)
        {
            if (record is ICell cell)
            {
                if (value != null)
                {
                    cell.EncounterZone.SetTo(value.FormKey);
                }
                else
                {
                    cell.EncounterZone.SetTo(null);
                }
            }
        }

        public override IFormLinkNullableGetter<IEncounterZoneGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cell)
            {
                return cell.EncounterZone;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IEncounterZoneGetter>? value1, IFormLinkNullableGetter<IEncounterZoneGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}


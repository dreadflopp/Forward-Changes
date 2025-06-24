using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class CellLocationPropertyHandler : AbstractPropertyHandler<IFormLinkNullableGetter<ILocationGetter>>
    {
        public override string PropertyName => "Location";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<ILocationGetter>? value)
        {
            if (record is ICell cell)
            {
                if (value != null)
                {
                    cell.Location.SetTo(value.FormKey);
                }
                else
                {
                    cell.Location.SetTo(null);
                }
            }
        }

        public override IFormLinkNullableGetter<ILocationGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cell)
            {
                return cell.Location;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<ILocationGetter>? value1, IFormLinkNullableGetter<ILocationGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}
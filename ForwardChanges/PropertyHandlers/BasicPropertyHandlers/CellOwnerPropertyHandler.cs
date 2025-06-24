using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class CellOwnerPropertyHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IOwnerGetter>>
    {
        public override string PropertyName => "Owner";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IOwnerGetter>? value)
        {
            if (record is ICell cell)
            {
                if (value != null)
                {
                    cell.Owner.SetTo(value.FormKey);
                }
                else
                {
                    cell.Owner.SetTo(null);
                }
            }
        }

        public override IFormLinkNullableGetter<IOwnerGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cell)
            {
                return cell.Owner;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IOwnerGetter>? value1, IFormLinkNullableGetter<IOwnerGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class AcousticSpaceHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IAcousticSpaceGetter>>
    {
        public override string PropertyName => "AcousticSpace";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IAcousticSpaceGetter>? value)
        {
            if (record is ICell cell)
            {
                if (value != null)
                {
                    cell.AcousticSpace.SetTo(value.FormKey);
                }
                else
                {
                    cell.AcousticSpace.SetTo(null);
                }
            }
        }

        public override IFormLinkNullableGetter<IAcousticSpaceGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cell)
            {
                return cell.AcousticSpace;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IAcousticSpaceGetter>? value1, IFormLinkNullableGetter<IAcousticSpaceGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}


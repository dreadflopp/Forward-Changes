using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class CellImageSpacePropertyHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IImageSpaceGetter>>
    {
        public override string PropertyName => "ImageSpace";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IImageSpaceGetter>? value)
        {
            if (record is ICell cell)
            {
                if (value != null)
                {
                    cell.ImageSpace.SetTo(value.FormKey);
                }
                else
                {
                    cell.ImageSpace.SetTo(null);
                }
            }
        }

        public override IFormLinkNullableGetter<IImageSpaceGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cell)
            {
                return cell.ImageSpace;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IImageSpaceGetter>? value1, IFormLinkNullableGetter<IImageSpaceGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}
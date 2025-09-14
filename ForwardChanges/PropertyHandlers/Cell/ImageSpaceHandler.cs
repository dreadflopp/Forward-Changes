using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class ImageSpaceHandler : AbstractFormLinkPropertyHandler<ICell, ICellGetter, IImageSpaceGetter>
    {
        public override string PropertyName => "ImageSpace";

        protected override IFormLinkNullableGetter<IImageSpaceGetter>? GetFormLinkValue(ICellGetter record)
        {
            return record.ImageSpace;
        }

        protected override void SetFormLinkValue(ICell record, IFormLinkNullableGetter<IImageSpaceGetter>? value)
        {
            if (value != null)
            {
                record.ImageSpace.SetTo(value.FormKey);
            }
            else
            {
                record.ImageSpace.SetTo(null);
            }
        }
    }
}


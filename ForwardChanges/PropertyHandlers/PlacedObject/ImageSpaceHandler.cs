using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class ImageSpaceHandler : AbstractFormLinkPropertyHandler<IPlacedObject, IPlacedObjectGetter, IImageSpaceGetter>
    {
        public override string PropertyName => "ImageSpace";

        protected override IFormLinkNullableGetter<IImageSpaceGetter>? GetFormLinkValue(IPlacedObjectGetter record)
        {
            return record.ImageSpace as IFormLinkNullableGetter<IImageSpaceGetter>;
        }

        protected override void SetFormLinkValue(IPlacedObject record, IFormLinkNullableGetter<IImageSpaceGetter>? value)
        {
            record.ImageSpace = value != null ? new FormLinkNullable<IImageSpaceGetter>(value.FormKey) : new FormLinkNullable<IImageSpaceGetter>();
        }
    }
}


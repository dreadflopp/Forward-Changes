using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class PlacedObjectImageSpacePropertyHandler : AbstractPropertyHandler<IFormLinkNullable<IImageSpaceGetter>>
    {
        public override string PropertyName => "ImageSpace";

        public override IFormLinkNullable<IImageSpaceGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.ImageSpace as IFormLinkNullable<IImageSpaceGetter>;
            }

            Console.WriteLine($"Error: Record does not implement IPlacedObjectGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkNullable<IImageSpaceGetter>? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.ImageSpace = value ?? new FormLinkNullable<IImageSpaceGetter>();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IPlacedObject for {PropertyName}");
            }
        }


    }
}
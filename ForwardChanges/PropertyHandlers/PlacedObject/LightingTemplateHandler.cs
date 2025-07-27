using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class LightingTemplateHandler : AbstractPropertyHandler<IFormLinkNullable<ILightingTemplateGetter>>
    {
        public override string PropertyName => "LightingTemplate";

                public override IFormLinkNullable<ILightingTemplateGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.LightingTemplate as IFormLinkNullable<ILightingTemplateGetter>;
            }
            
            Console.WriteLine($"Error: Record does not implement IPlacedObjectGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkNullable<ILightingTemplateGetter>? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.LightingTemplate = value ?? new FormLinkNullable<ILightingTemplateGetter>();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IPlacedObject for {PropertyName}");
            }
        }


    }
}


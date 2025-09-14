using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class LightingTemplateHandler : AbstractFormLinkPropertyHandler<IPlacedObject, IPlacedObjectGetter, ILightingTemplateGetter>
    {
        public override string PropertyName => "LightingTemplate";

        protected override IFormLinkNullableGetter<ILightingTemplateGetter>? GetFormLinkValue(IPlacedObjectGetter record)
        {
            return record.LightingTemplate as IFormLinkNullableGetter<ILightingTemplateGetter>;
        }

        protected override void SetFormLinkValue(IPlacedObject record, IFormLinkNullableGetter<ILightingTemplateGetter>? value)
        {
            record.LightingTemplate = value != null ? new FormLinkNullable<ILightingTemplateGetter>(value.FormKey) : new FormLinkNullable<ILightingTemplateGetter>();
        }
    }
}


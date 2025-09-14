using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class LightingTemplateHandler : AbstractFormLinkPropertyHandler<ICell, ICellGetter, ILightingTemplateGetter>
    {
        public override string PropertyName => "LightingTemplate";

        protected override IFormLinkNullableGetter<ILightingTemplateGetter>? GetFormLinkValue(ICellGetter record)
        {
            return record.LightingTemplate as IFormLinkNullableGetter<ILightingTemplateGetter>;
        }

        protected override void SetFormLinkValue(ICell record, IFormLinkNullableGetter<ILightingTemplateGetter>? value)
        {
            if (value != null)
            {
                record.LightingTemplate.SetTo(value.FormKey);
            }
            else
            {
                record.LightingTemplate.SetTo(null);
            }
        }
    }
}


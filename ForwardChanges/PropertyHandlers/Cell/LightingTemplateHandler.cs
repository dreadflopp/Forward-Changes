using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class LightingTemplateHandler : AbstractPropertyHandler<IFormLinkGetter<ILightingTemplateGetter>>
    {
        public override string PropertyName => "LightingTemplate";

        public override void SetValue(IMajorRecord record, IFormLinkGetter<ILightingTemplateGetter>? value)
        {
            if (record is ICell cell)
            {
                if (value != null)
                {
                    cell.LightingTemplate.SetTo(value.FormKey);
                }
                else
                {
                    cell.LightingTemplate.SetTo(null);
                }
            }
        }

        public override IFormLinkGetter<ILightingTemplateGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cell)
            {
                return cell.LightingTemplate;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkGetter<ILightingTemplateGetter>? value1, IFormLinkGetter<ILightingTemplateGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}


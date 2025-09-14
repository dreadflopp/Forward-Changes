using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class InteriorLightingHandler : AbstractFormLinkPropertyHandler<IWorldspace, IWorldspaceGetter, ILightingTemplateGetter>
    {
        public override string PropertyName => "InteriorLighting";

        protected override IFormLinkNullableGetter<ILightingTemplateGetter>? GetFormLinkValue(IWorldspaceGetter record)
        {
            return record.InteriorLighting;
        }

        protected override void SetFormLinkValue(IWorldspace record, IFormLinkNullableGetter<ILightingTemplateGetter>? value)
        {
            if (value != null)
            {
                record.InteriorLighting.SetTo(value.FormKey);
            }
            else
            {
                record.InteriorLighting.SetTo(null);
            }
        }
    }
}
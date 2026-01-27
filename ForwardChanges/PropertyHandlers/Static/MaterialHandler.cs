using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Static
{
    public class MaterialHandler : AbstractFormLinkPropertyHandler<IStatic, IStaticGetter, IMaterialObjectGetter>
    {
        public override string PropertyName => "Material";

        protected override IFormLinkNullableGetter<IMaterialObjectGetter>? GetFormLinkValue(IStaticGetter record)
        {
            // Material is IFormLink, but we can treat it as nullable for comparison
            var material = record.Material;
            if (material == null || material.FormKey.IsNull)
            {
                return null;
            }
            return new FormLinkNullable<IMaterialObjectGetter>(material.FormKey);
        }

        protected override void SetFormLinkValue(IStatic record, IFormLinkNullableGetter<IMaterialObjectGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Material = new FormLink<IMaterialObjectGetter>(value.FormKey);
            }
            else
            {
                record.Material.Clear();
            }
        }
    }
}

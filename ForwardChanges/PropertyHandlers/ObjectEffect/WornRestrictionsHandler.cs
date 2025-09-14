using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ObjectEffect
{
    public class WornRestrictionsHandler : AbstractFormLinkPropertyHandler<IObjectEffect, IObjectEffectGetter, IFormListGetter>
    {
        public override string PropertyName => "WornRestrictions";

        protected override IFormLinkNullableGetter<IFormListGetter>? GetFormLinkValue(IObjectEffectGetter record)
        {
            return record.WornRestrictions as IFormLinkNullableGetter<IFormListGetter>;
        }

        protected override void SetFormLinkValue(IObjectEffect record, IFormLinkNullableGetter<IFormListGetter>? value)
        {
            record.WornRestrictions = new FormLinkNullable<IFormListGetter>(value?.FormKey ?? FormKey.Null);
        }
    }
}


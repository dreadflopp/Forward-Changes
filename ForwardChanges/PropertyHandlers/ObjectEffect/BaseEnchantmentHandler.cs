using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ObjectEffect
{
    public class BaseEnchantmentHandler : AbstractFormLinkPropertyHandler<IObjectEffect, IObjectEffectGetter, IObjectEffectGetter>
    {
        public override string PropertyName => "BaseEnchantment";

        protected override IFormLinkNullableGetter<IObjectEffectGetter>? GetFormLinkValue(IObjectEffectGetter record)
        {
            return record.BaseEnchantment as IFormLinkNullableGetter<IObjectEffectGetter>;
        }

        protected override void SetFormLinkValue(IObjectEffect record, IFormLinkNullableGetter<IObjectEffectGetter>? value)
        {
            record.BaseEnchantment = new FormLinkNullable<IObjectEffectGetter>(value?.FormKey ?? FormKey.Null);
        }
    }
}


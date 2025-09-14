using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class BaseHandler : AbstractFormLinkPropertyHandler<IPlacedNpc, IPlacedNpcGetter, INpcGetter>
    {
        public override string PropertyName => "Base";

        protected override IFormLinkNullableGetter<INpcGetter>? GetFormLinkValue(IPlacedNpcGetter record)
        {
            return record.Base;
        }

        protected override void SetFormLinkValue(IPlacedNpc record, IFormLinkNullableGetter<INpcGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Base = new FormLinkNullable<INpcGetter>(value.FormKey);
            }
            else
            {
                record.Base.Clear();
            }
        }
    }
}
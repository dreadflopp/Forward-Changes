using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class MerchantContainerHandler : AbstractFormLinkPropertyHandler<IPlacedNpc, IPlacedNpcGetter, IPlacedObjectGetter>
    {
        public override string PropertyName => "MerchantContainer";

        protected override IFormLinkNullableGetter<IPlacedObjectGetter> GetFormLinkValue(IPlacedNpcGetter record)
        {
            return record.MerchantContainer;
        }

        protected override void SetFormLinkValue(IPlacedNpc record, IFormLinkNullableGetter<IPlacedObjectGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.MerchantContainer = new FormLinkNullable<IPlacedObjectGetter>(value.FormKey);
            }
            else
            {
                record.MerchantContainer.Clear();
            }
        }
    }
}
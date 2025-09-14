using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class OwnerHandler : AbstractFormLinkPropertyHandler<IPlacedNpc, IPlacedNpcGetter, IOwnerGetter>
    {
        public override string PropertyName => "Owner";

        protected override IFormLinkNullableGetter<IOwnerGetter> GetFormLinkValue(IPlacedNpcGetter record)
        {
            return record.Owner;
        }

        protected override void SetFormLinkValue(IPlacedNpc record, IFormLinkNullableGetter<IOwnerGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Owner = new FormLinkNullable<IOwnerGetter>(value.FormKey);
            }
            else
            {
                record.Owner.Clear();
            }
        }
    }
}
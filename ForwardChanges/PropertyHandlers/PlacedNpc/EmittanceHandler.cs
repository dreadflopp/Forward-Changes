using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class EmittanceHandler : AbstractFormLinkPropertyHandler<IPlacedNpc, IPlacedNpcGetter, IEmittanceGetter>
    {
        public override string PropertyName => "Emittance";

        protected override IFormLinkNullableGetter<IEmittanceGetter> GetFormLinkValue(IPlacedNpcGetter record)
        {
            return record.Emittance;
        }

        protected override void SetFormLinkValue(IPlacedNpc record, IFormLinkNullableGetter<IEmittanceGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Emittance = new FormLinkNullable<IEmittanceGetter>(value.FormKey);
            }
            else
            {
                record.Emittance.Clear();
            }
        }
    }
}
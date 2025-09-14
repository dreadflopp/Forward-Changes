using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class HorseHandler : AbstractFormLinkPropertyHandler<IPlacedNpc, IPlacedNpcGetter, IPlacedNpcGetter>
    {
        public override string PropertyName => "Horse";

        protected override IFormLinkNullableGetter<IPlacedNpcGetter> GetFormLinkValue(IPlacedNpcGetter record)
        {
            return record.Horse;
        }

        protected override void SetFormLinkValue(IPlacedNpc record, IFormLinkNullableGetter<IPlacedNpcGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Horse = new FormLinkNullable<IPlacedNpcGetter>(value.FormKey);
            }
            else
            {
                record.Horse.Clear();
            }
        }
    }
}
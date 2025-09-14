using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class WaterHandler : AbstractFormLinkPropertyHandler<ICell, ICellGetter, IWaterGetter>
    {
        public override string PropertyName => "Water";

        protected override IFormLinkNullableGetter<IWaterGetter>? GetFormLinkValue(ICellGetter record)
        {
            return record.Water;
        }

        protected override void SetFormLinkValue(ICell record, IFormLinkNullableGetter<IWaterGetter>? value)
        {
            if (value != null)
            {
                record.Water.SetTo(value.FormKey);
            }
            else
            {
                record.Water.SetTo(null);
            }
        }
    }
}


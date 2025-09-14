using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class LocationHandler : AbstractFormLinkPropertyHandler<ICell, ICellGetter, ILocationGetter>
    {
        public override string PropertyName => "Location";

        protected override IFormLinkNullableGetter<ILocationGetter>? GetFormLinkValue(ICellGetter record)
        {
            return record.Location;
        }

        protected override void SetFormLinkValue(ICell record, IFormLinkNullableGetter<ILocationGetter>? value)
        {
            if (value != null)
            {
                record.Location.SetTo(value.FormKey);
            }
            else
            {
                record.Location.SetTo(null);
            }
        }
    }
}


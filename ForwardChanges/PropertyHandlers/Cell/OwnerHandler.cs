using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class OwnerHandler : AbstractFormLinkPropertyHandler<ICell, ICellGetter, IOwnerGetter>
    {
        public override string PropertyName => "Owner";

        protected override IFormLinkNullableGetter<IOwnerGetter>? GetFormLinkValue(ICellGetter record)
        {
            return record.Owner;
        }

        protected override void SetFormLinkValue(ICell record, IFormLinkNullableGetter<IOwnerGetter>? value)
        {
            if (value != null)
            {
                record.Owner.SetTo(value.FormKey);
            }
            else
            {
                record.Owner.SetTo(null);
            }
        }
    }
}


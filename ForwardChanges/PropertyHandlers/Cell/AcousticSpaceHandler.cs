using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class AcousticSpaceHandler : AbstractFormLinkPropertyHandler<ICell, ICellGetter, IAcousticSpaceGetter>
    {
        public override string PropertyName => "AcousticSpace";

        protected override IFormLinkNullableGetter<IAcousticSpaceGetter>? GetFormLinkValue(ICellGetter record)
        {
            return record.AcousticSpace;
        }

        protected override void SetFormLinkValue(ICell record, IFormLinkNullableGetter<IAcousticSpaceGetter>? value)
        {
            if (value != null)
            {
                record.AcousticSpace.SetTo(value.FormKey);
            }
            else
            {
                record.AcousticSpace.SetTo(null);
            }
        }
    }
}


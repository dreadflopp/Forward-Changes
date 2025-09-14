using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class ClimateHandler : AbstractFormLinkPropertyHandler<IWorldspace, IWorldspaceGetter, IClimateGetter>
    {
        public override string PropertyName => "Climate";

        protected override IFormLinkNullableGetter<IClimateGetter>? GetFormLinkValue(IWorldspaceGetter record)
        {
            return record.Climate;
        }

        protected override void SetFormLinkValue(IWorldspace record, IFormLinkNullableGetter<IClimateGetter>? value)
        {
            if (value != null)
            {
                record.Climate.SetTo(value.FormKey);
            }
            else
            {
                record.Climate.SetTo(null);
            }
        }
    }
}
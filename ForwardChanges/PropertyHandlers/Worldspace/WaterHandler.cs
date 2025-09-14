using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class WaterHandler : AbstractFormLinkPropertyHandler<IWorldspace, IWorldspaceGetter, IWaterGetter>
    {
        public override string PropertyName => "Water";

        protected override IFormLinkNullableGetter<IWaterGetter>? GetFormLinkValue(IWorldspaceGetter record)
        {
            return record.Water;
        }

        protected override void SetFormLinkValue(IWorldspace record, IFormLinkNullableGetter<IWaterGetter>? value)
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


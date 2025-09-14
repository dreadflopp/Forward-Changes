using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class LodWaterHandler : AbstractFormLinkPropertyHandler<IWorldspace, IWorldspaceGetter, IWaterGetter>
    {
        public override string PropertyName => "LodWater";

        protected override IFormLinkNullableGetter<IWaterGetter>? GetFormLinkValue(IWorldspaceGetter record)
        {
            return record.LodWater;
        }

        protected override void SetFormLinkValue(IWorldspace record, IFormLinkNullableGetter<IWaterGetter>? value)
        {
            if (value != null)
            {
                record.LodWater.SetTo(value.FormKey);
            }
            else
            {
                record.LodWater.SetTo(null);
            }
        }
    }
}


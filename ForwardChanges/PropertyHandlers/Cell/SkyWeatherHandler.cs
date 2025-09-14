using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class SkyWeatherHandler : AbstractFormLinkPropertyHandler<ICell, ICellGetter, IRegionGetter>
    {
        public override string PropertyName => "SkyAndWeatherFromRegion";

        protected override IFormLinkNullableGetter<IRegionGetter>? GetFormLinkValue(ICellGetter record)
        {
            return record.SkyAndWeatherFromRegion;
        }

        protected override void SetFormLinkValue(ICell record, IFormLinkNullableGetter<IRegionGetter>? value)
        {
            if (value != null)
            {
                record.SkyAndWeatherFromRegion.SetTo(value.FormKey);
            }
            else
            {
                record.SkyAndWeatherFromRegion.SetTo(null);
            }
        }
    }
}


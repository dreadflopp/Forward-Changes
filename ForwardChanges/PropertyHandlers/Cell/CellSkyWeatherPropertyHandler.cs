using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class CellSkyWeatherPropertyHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IRegionGetter>>
    {
        public override string PropertyName => "SkyAndWeatherFromRegion";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IRegionGetter>? value)
        {
            if (record is ICell cell)
            {
                if (value != null)
                {
                    cell.SkyAndWeatherFromRegion.SetTo(value.FormKey);
                }
                else
                {
                    cell.SkyAndWeatherFromRegion.SetTo(null);
                }
            }
        }

        public override IFormLinkNullableGetter<IRegionGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cell)
            {
                return cell.SkyAndWeatherFromRegion;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IRegionGetter>? value1, IFormLinkNullableGetter<IRegionGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}
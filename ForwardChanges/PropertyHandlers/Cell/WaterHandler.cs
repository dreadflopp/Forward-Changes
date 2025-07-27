using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class WaterHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IWaterGetter>>
    {
        public override string PropertyName => "Water";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IWaterGetter>? value)
        {
            if (record is ICell cell)
            {
                if (value != null)
                {
                    cell.Water.SetTo(value.FormKey);
                }
                else
                {
                    cell.Water.SetTo(null);
                }
            }
        }

        public override IFormLinkNullableGetter<IWaterGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cell)
            {
                return cell.Water;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IWaterGetter>? value1, IFormLinkNullableGetter<IWaterGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}


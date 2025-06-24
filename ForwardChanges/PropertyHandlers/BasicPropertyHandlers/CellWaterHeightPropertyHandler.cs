using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class CellWaterHeightPropertyHandler : AbstractPropertyHandler<float?>
    {
        public override string PropertyName => "WaterHeight";

        public override void SetValue(IMajorRecord record, float? value)
        {
            if (record is ICell cell)
            {
                cell.WaterHeight = value;
            }
        }

        public override float? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cell)
            {
                return cell.WaterHeight;
            }
            return null;
        }

        public override bool AreValuesEqual(float? value1, float? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return Math.Abs(value1.Value - value2.Value) < 0.001f; // Use small epsilon for float comparison
        }
    }
}
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class CellLightingPropertyHandler : AbstractPropertyHandler<ICellLightingGetter>
    {
        public override string PropertyName => "Lighting";

        public override void SetValue(IMajorRecord record, ICellLightingGetter? value)
        {
            if (record is ICell cell)
            {
                cell.Lighting = value?.DeepCopy();
            }
        }

        public override ICellLightingGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cell)
            {
                return cell.Lighting;
            }
            return null;
        }

        public override bool AreValuesEqual(ICellLightingGetter? value1, ICellLightingGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Use Mutagen's translation mask to compare only the Lighting property
            // This treats the entire Lighting group as a single unit
            return value1.Equals(value2);
        }
    }
}
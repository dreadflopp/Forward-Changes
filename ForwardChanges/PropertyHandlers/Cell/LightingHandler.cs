using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class LightingHandler : AbstractPropertyHandler<ICellLightingGetter>
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

            // Compare all properties using value-based comparison
            return value1.Versioning == value2.Versioning &&
                   value1.AmbientColor == value2.AmbientColor &&
                   value1.DirectionalColor == value2.DirectionalColor &&
                   value1.FogNearColor == value2.FogNearColor &&
                   value1.FogNear == value2.FogNear &&
                   value1.FogFar == value2.FogFar &&
                   value1.DirectionalRotationXY == value2.DirectionalRotationXY &&
                   value1.DirectionalRotationZ == value2.DirectionalRotationZ &&
                   value1.DirectionalFade == value2.DirectionalFade &&
                   value1.FogClipDistance == value2.FogClipDistance &&
                   value1.FogPower == value2.FogPower &&
                   value1.AmbientColors.Equals(value2.AmbientColors) &&
                   value1.FogFarColor == value2.FogFarColor &&
                   value1.FogMax == value2.FogMax &&
                   value1.LightFadeBegin == value2.LightFadeBegin &&
                   value1.LightFadeEnd == value2.LightFadeEnd &&
                   value1.Inherits == value2.Inherits;
        }
    }
}


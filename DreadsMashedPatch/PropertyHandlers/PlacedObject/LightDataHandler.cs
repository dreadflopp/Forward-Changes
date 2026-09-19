using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using DreadsMashedPatch;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using Noggog;

namespace DreadsMashedPatch.PropertyHandlers.PlacedObject
{
    /// <summary>
    /// Handles LightData on IPlacedObject as one object: any change in any value counts as a change.
    /// Does not merge; forwards the whole object. Floats use 6 decimals (xEdit precision).
    /// </summary>
    public class LightDataHandler : AbstractPropertyHandler<ILightDataGetter?>
    {
        public override string PropertyName => "LightData";

        public override ILightDataGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placed)
            {
                return placed.LightData;
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, ILightDataGetter? value)
        {
            if (record is not IPlacedObject placed)
            {
                return;
            }

            if (value == null)
            {
                placed.LightData = null;
                return;
            }

            placed.LightData = new LightData
            {
                FovOffset = value.FovOffset,
                FadeOffset = value.FadeOffset,
                EndDistanceCap = value.EndDistanceCap,
                ShadowDepthBias = value.ShadowDepthBias,
                Unknown = value.Unknown
            };
        }

        public override bool AreValuesEqual(ILightDataGetter? value1, ILightDataGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            const float eps = P3FloatComparison.Float5DecimalsEpsilon;
            return value1.FovOffset.EqualsWithin(value2.FovOffset, eps)
                   && value1.FadeOffset.EqualsWithin(value2.FadeOffset, eps)
                   && value1.EndDistanceCap.EqualsWithin(value2.EndDistanceCap, eps)
                   && value1.ShadowDepthBias.EqualsWithin(value2.ShadowDepthBias, eps)
                   && value1.Unknown == value2.Unknown;
        }

        public override string FormatValue(object? value)
        {
            if (value is not ILightDataGetter ld)
            {
                return value?.ToString() ?? "null";
            }
            return $"FovOffset={ld.FovOffset:F6}, FadeOffset={ld.FadeOffset:F6}, EndDistanceCap={ld.EndDistanceCap:F6}, ShadowDepthBias={ld.ShadowDepthBias:F6}, Unknown={ld.Unknown}";
        }
    }
}

using System.Globalization;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace DreadsMashedPatch.PropertyHandlers.General;

/// <summary>
/// Treats a placed reference's complete DATA placement as one cohesive value while
/// comparing its serialized position and rotation components at xEdit-visible precision.
/// </summary>
public class PlacementPropertyHandler<TRecord, TRecordGetter>
    : ComplexReflectionPropertyHandler<IPlacementGetter, TRecord, TRecordGetter>
    where TRecord : class, IMajorRecord
    where TRecordGetter : class, IMajorRecordGetter
{
    public PlacementPropertyHandler()
        : base("Placement")
    {
    }

    public override bool AreValuesEqual(IPlacementGetter? value1, IPlacementGetter? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return P3FloatComparison.PositionsEqual(value1.Position, value2.Position)
            && P3FloatComparison.RotationsEqual(value1.Rotation, value2.Rotation);
    }

    public override string FormatValue(object? value)
    {
        if (value is not IPlacementGetter placement)
        {
            return base.FormatValue(value);
        }

        return $"Position=({FormatPosition(placement.Position)}), RotationDegrees=({FormatRotation(placement.Rotation)})";
    }

    private static string FormatPosition(P3Float value)
    {
        return string.Join(", ",
            FormatPositionComponent(value.X),
            FormatPositionComponent(value.Y),
            FormatPositionComponent(value.Z));
    }

    private static string FormatRotation(P3Float value)
    {
        return string.Join(", ",
            FormatRotationComponent(value.X),
            FormatRotationComponent(value.Y),
            FormatRotationComponent(value.Z));
    }

    private static string FormatPositionComponent(float value)
    {
        var normalized = value == 0f ? 0f : value;
        return normalized.ToString("F6", CultureInfo.InvariantCulture);
    }

    private static string FormatRotationComponent(float radians)
    {
        return P3FloatComparison.NormalizedDegreesAtXEditPrecision(radians)
            .ToString("F4", CultureInfo.InvariantCulture);
    }
}

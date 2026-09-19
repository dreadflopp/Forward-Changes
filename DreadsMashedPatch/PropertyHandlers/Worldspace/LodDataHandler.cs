using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.Worldspace;

public sealed record WorldspaceLodDataValue(FormKey Water, float? Height);

/// <summary>
/// Owns WRLD NAM3/NAM4 as one xEdit LOD Data structure. NAM3 may be absent and
/// use the default water, while NAM4 is required whenever the structure exists.
/// </summary>
public sealed class LodDataHandler : AbstractPropertyHandler<WorldspaceLodDataValue>
{
    public override string PropertyName => "LodData";

    public override WorldspaceLodDataValue GetValue(IMajorRecordGetter record)
    {
        if (record is not IWorldspaceGetter worldspace)
        {
            throw new InvalidOperationException($"Expected IWorldspaceGetter but got {record.GetType()}");
        }

        return new WorldspaceLodDataValue(worldspace.LodWater.FormKey, worldspace.LodWaterHeight);
    }

    public override void SetValue(IMajorRecord record, WorldspaceLodDataValue? value)
    {
        if (record is not IWorldspace worldspace)
        {
            throw new InvalidOperationException($"Expected IWorldspace but got {record.GetType()}");
        }

        if (value == null)
        {
            worldspace.LodWater.SetTo(FormKey.Null);
            worldspace.LodWaterHeight = null;
            return;
        }

        if (!value.Water.IsNull && !value.Height.HasValue)
        {
            throw new InvalidOperationException(
                "WRLD LOD Data cannot contain NAM3 without the required NAM4 height.");
        }

        worldspace.LodWater.SetTo(value.Water);
        worldspace.LodWaterHeight = value.Height;
    }

    public override bool AreValuesEqual(
        WorldspaceLodDataValue? value1,
        WorldspaceLodDataValue? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return value1.Water.Equals(value2.Water)
            && NullableFloatEquals(value1.Height, value2.Height);
    }

    private static bool NullableFloatEquals(float? left, float? right)
        => left.HasValue == right.HasValue
            && (!left.HasValue || Math.Abs(left.Value - right!.Value) < 0.0001f);
}

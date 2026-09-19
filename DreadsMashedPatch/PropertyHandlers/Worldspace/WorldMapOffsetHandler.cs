using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace DreadsMashedPatch.PropertyHandlers.Worldspace;

public sealed record WorldspaceMapOffsetValue(float Scale, P3Float CellOffset);

/// <summary>
/// Owns the complete required WRLD ONAM structure.
/// </summary>
public sealed class WorldMapOffsetHandler : AbstractPropertyHandler<WorldspaceMapOffsetValue>
{
    public override string PropertyName => "WorldMapOffset";

    public override WorldspaceMapOffsetValue GetValue(IMajorRecordGetter record)
    {
        if (record is not IWorldspaceGetter worldspace)
        {
            throw new InvalidOperationException($"Expected IWorldspaceGetter but got {record.GetType()}");
        }

        return new WorldspaceMapOffsetValue(
            worldspace.WorldMapOffsetScale,
            worldspace.WorldMapCellOffset);
    }

    public override void SetValue(IMajorRecord record, WorldspaceMapOffsetValue? value)
    {
        if (record is not IWorldspace worldspace)
        {
            throw new InvalidOperationException($"Expected IWorldspace but got {record.GetType()}");
        }

        if (value == null)
        {
            throw new InvalidOperationException("Required WRLD World Map Offset data cannot be null.");
        }

        worldspace.WorldMapOffsetScale = value.Scale;
        worldspace.WorldMapCellOffset = value.CellOffset;
    }

    public override bool AreValuesEqual(
        WorldspaceMapOffsetValue? value1,
        WorldspaceMapOffsetValue? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return Math.Abs(value1.Scale - value2.Scale) < 0.0001f
            && P3FloatComparison.EqualsWithin(value1.CellOffset, value2.CellOffset);
    }
}

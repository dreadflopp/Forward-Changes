using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.Worldspace;

/// <summary>
/// Treats absent WRLD NAMA as xEdit's semantic default of 1.0 and always writes
/// an explicit value when a forwarding decision must be applied.
/// </summary>
public sealed class DistantLodMultiplierHandler : AbstractPropertyHandler<float>
{
    public override string PropertyName => "DistantLodMultiplier";

    public override float GetValue(IMajorRecordGetter record)
        => record is IWorldspaceGetter worldspace
            ? worldspace.DistantLodMultiplier ?? 1f
            : throw new InvalidOperationException($"Expected IWorldspaceGetter but got {record.GetType()}");

    public override void SetValue(IMajorRecord record, float value)
    {
        if (record is not IWorldspace worldspace)
        {
            throw new InvalidOperationException($"Expected IWorldspace but got {record.GetType()}");
        }

        worldspace.DistantLodMultiplier = value;
    }

    public override bool AreValuesEqual(float value1, float value2)
        => Math.Abs(value1 - value2) < 0.0001f;
}

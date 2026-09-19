using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace DreadsMashedPatch.PropertyHandlers.ReverbParameters;

/// <summary>
/// Immutable snapshot of the complete packed REVB DATA subrecord.
/// </summary>
public sealed record ReverbDataValue(
    ushort DecayMilliseconds,
    ushort HfReferenceHertz,
    sbyte RoomFilter,
    sbyte RoomHfFilter,
    sbyte Reflections,
    sbyte ReverbAmp,
    float DecayHfRatio,
    byte ReflectDelayMS,
    byte ReverbDelayMS,
    Percent DiffusionPercent,
    Percent DensityPercent,
    byte Unknown);

/// <summary>
/// Treats the complete REVB DATA subrecord as one acoustically coupled preset.
/// A change to any member forwards every DATA member from the same override.
/// </summary>
public sealed class ReverbDataHandler : AbstractPropertyHandler<ReverbDataValue>
{
    public override string PropertyName => "ReverbData";

    public override ReverbDataValue? GetValue(IMajorRecordGetter record) =>
        record is IReverbParametersGetter reverb
            ? new ReverbDataValue(
                reverb.DecayMilliseconds,
                reverb.HfReferenceHertz,
                reverb.RoomFilter,
                reverb.RoomHfFilter,
                reverb.Reflections,
                reverb.ReverbAmp,
                reverb.DecayHfRatio,
                reverb.ReflectDelayMS,
                reverb.ReverbDelayMS,
                reverb.DiffusionPercent,
                reverb.DensityPercent,
                reverb.Unknown)
            : null;

    public override void SetValue(IMajorRecord record, ReverbDataValue? value)
    {
        if (record is not IReverbParameters reverb || value == null)
        {
            return;
        }

        reverb.DecayMilliseconds = value.DecayMilliseconds;
        reverb.HfReferenceHertz = value.HfReferenceHertz;
        reverb.RoomFilter = value.RoomFilter;
        reverb.RoomHfFilter = value.RoomHfFilter;
        reverb.Reflections = value.Reflections;
        reverb.ReverbAmp = value.ReverbAmp;
        reverb.DecayHfRatio = value.DecayHfRatio;
        reverb.ReflectDelayMS = value.ReflectDelayMS;
        reverb.ReverbDelayMS = value.ReverbDelayMS;
        reverb.DiffusionPercent = value.DiffusionPercent;
        reverb.DensityPercent = value.DensityPercent;
        reverb.Unknown = value.Unknown;
    }

    public override string FormatValue(object? value) =>
        value is ReverbDataValue data
            ? $"Decay={data.DecayMilliseconds}, HfReference={data.HfReferenceHertz}, "
              + $"RoomFilter={data.RoomFilter}, RoomHfFilter={data.RoomHfFilter}, "
              + $"Reflections={data.Reflections}, ReverbAmp={data.ReverbAmp}, "
              + $"DecayHfRatio={data.DecayHfRatio}, ReflectDelay={data.ReflectDelayMS}, "
              + $"ReverbDelay={data.ReverbDelayMS}, Diffusion={data.DiffusionPercent}, "
              + $"Density={data.DensityPercent}, Unknown={data.Unknown}"
            : "null";
}

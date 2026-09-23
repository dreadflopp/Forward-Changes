using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.SoundDescriptor;

public readonly record struct SoundDescriptorPitch(
    sbyte PercentFrequencyShift,
    sbyte PercentFrequencyVariance);

public readonly record struct SoundDescriptorVolume(
    byte Variance,
    float StaticAttenuation);

public sealed class SoundDescriptorPitchHandler : AbstractPropertyHandler<SoundDescriptorPitch>
{
    public override string PropertyName => "Pitch";

    public override SoundDescriptorPitch GetValue(IMajorRecordGetter record)
    {
        var descriptor = RequireGetter(record);
        return new(descriptor.PercentFrequencyShift, descriptor.PercentFrequencyVariance);
    }

    public override void SetValue(IMajorRecord record, SoundDescriptorPitch value)
    {
        var descriptor = RequireSetter(record);
        descriptor.PercentFrequencyShift = value.PercentFrequencyShift;
        descriptor.PercentFrequencyVariance = value.PercentFrequencyVariance;
    }

    private static ISoundDescriptorGetter RequireGetter(IMajorRecordGetter record) =>
        record as ISoundDescriptorGetter
        ?? throw new InvalidOperationException($"Expected ISoundDescriptorGetter but got {record.GetType()}");

    private static ISoundDescriptor RequireSetter(IMajorRecord record) =>
        record as ISoundDescriptor
        ?? throw new InvalidOperationException($"Expected ISoundDescriptor but got {record.GetType()}");
}

public sealed class SoundDescriptorVolumeHandler : AbstractPropertyHandler<SoundDescriptorVolume>
{
    public override string PropertyName => "Volume";

    public override SoundDescriptorVolume GetValue(IMajorRecordGetter record)
    {
        var descriptor = RequireGetter(record);
        return new(descriptor.Variance, descriptor.StaticAttenuation);
    }

    public override void SetValue(IMajorRecord record, SoundDescriptorVolume value)
    {
        var descriptor = RequireSetter(record);
        descriptor.Variance = value.Variance;
        descriptor.StaticAttenuation = value.StaticAttenuation;
    }

    private static ISoundDescriptorGetter RequireGetter(IMajorRecordGetter record) =>
        record as ISoundDescriptorGetter
        ?? throw new InvalidOperationException($"Expected ISoundDescriptorGetter but got {record.GetType()}");

    private static ISoundDescriptor RequireSetter(IMajorRecord record) =>
        record as ISoundDescriptor
        ?? throw new InvalidOperationException($"Expected ISoundDescriptor but got {record.GetType()}");
}

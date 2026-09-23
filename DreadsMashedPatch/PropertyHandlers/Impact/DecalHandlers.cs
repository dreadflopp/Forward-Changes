using System.Globalization;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.Impact;

public readonly record struct ImpactDecalBounds(
    float MinWidth,
    float MaxWidth,
    float MinHeight,
    float MaxHeight);

public readonly record struct ImpactDecalParallax(float Scale, byte Passes);

public sealed class ImpactDecalPresenceHandler : AbstractPropertyHandler<bool>
{
    public override string PropertyName => "Decal.Presence";

    public override bool GetValue(IMajorRecordGetter record) =>
        record is IImpactGetter impact && impact.Decal != null;

    public override void SetValue(IMajorRecord record, bool value)
    {
        if (record is not IImpact impact) return;
        impact.Decal = value ? impact.Decal ?? new Decal() : null;
    }

    public override string FormatValue(object? value) => value is true ? "Present" : "Absent";
}

/// <summary>
/// Keeps the four decal dimensions together so forwarding cannot create invalid
/// min/max combinations while still allowing them to merge independently from
/// flags and the remaining DODT settings.
/// </summary>
public sealed class ImpactDecalBoundsHandler : AbstractPropertyHandler<ImpactDecalBounds?>
{
    public override string PropertyName => "Decal.Bounds";

    public override ImpactDecalBounds? GetValue(IMajorRecordGetter record)
    {
        if (record is not IImpactGetter { Decal: { } decal }) return null;
        return new ImpactDecalBounds(
            decal.MinWidth,
            decal.MaxWidth,
            decal.MinHeight,
            decal.MaxHeight);
    }

    public override void SetValue(IMajorRecord record, ImpactDecalBounds? value)
    {
        if (record is not IImpact impact || value is not { } bounds) return;
        impact.Decal ??= new Decal();
        impact.Decal.MinWidth = bounds.MinWidth;
        impact.Decal.MaxWidth = bounds.MaxWidth;
        impact.Decal.MinHeight = bounds.MinHeight;
        impact.Decal.MaxHeight = bounds.MaxHeight;
    }

    public override string FormatValue(object? value) => value is ImpactDecalBounds bounds
        ? $"Width={FormatRange(bounds.MinWidth, bounds.MaxWidth)}, Height={FormatRange(bounds.MinHeight, bounds.MaxHeight)}"
        : "null";

    private static string FormatRange(float min, float max) =>
        $"{min.ToString("G9", CultureInfo.InvariantCulture)}..{max.ToString("G9", CultureInfo.InvariantCulture)}";
}

/// <summary>
/// Scale and pass count jointly define the parallax configuration and therefore
/// move through conflict resolution as one value.
/// </summary>
public sealed class ImpactDecalParallaxHandler : AbstractPropertyHandler<ImpactDecalParallax?>
{
    public override string PropertyName => "Decal.Parallax";

    public override ImpactDecalParallax? GetValue(IMajorRecordGetter record)
    {
        if (record is not IImpactGetter { Decal: { } decal }) return null;
        return new ImpactDecalParallax(decal.ParallaxScale, decal.ParallaxPasses);
    }

    public override void SetValue(IMajorRecord record, ImpactDecalParallax? value)
    {
        if (record is not IImpact impact || value is not { } parallax) return;
        impact.Decal ??= new Decal();
        impact.Decal.ParallaxScale = parallax.Scale;
        impact.Decal.ParallaxPasses = parallax.Passes;
    }

    public override string FormatValue(object? value) => value is ImpactDecalParallax parallax
        ? $"Scale={parallax.Scale.ToString("G9", CultureInfo.InvariantCulture)}, Passes={parallax.Passes}"
        : "null";
}

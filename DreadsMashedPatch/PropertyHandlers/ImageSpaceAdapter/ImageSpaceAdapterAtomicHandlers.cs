using System.Drawing;
using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace ForwardChanges.PropertyHandlers.ImageSpaceAdapter;

public sealed record AnimationSettingsValue(bool Animatable, float Duration);

public sealed record KeyFrameCurvePairValue(
    IReadOnlyList<IKeyFrameGetter>? Mult,
    IReadOnlyList<IKeyFrameGetter>? Add);

public sealed record RadialBlurValue(
    bool UseTarget,
    P2Float Center,
    IReadOnlyList<IKeyFrameGetter>? Strength,
    IReadOnlyList<IKeyFrameGetter>? RampUp,
    IReadOnlyList<IKeyFrameGetter>? Start,
    IReadOnlyList<IKeyFrameGetter>? RampDown,
    IReadOnlyList<IKeyFrameGetter>? DownStart);

public sealed record DepthOfFieldValue(
    Mutagen.Bethesda.Skyrim.ImageSpaceAdapter.DepthOfFieldFlag Flags,
    IReadOnlyList<IKeyFrameGetter>? Strength,
    IReadOnlyList<IKeyFrameGetter>? Distance,
    IReadOnlyList<IKeyFrameGetter>? Range);

public sealed class AnimationSettingsHandler : AbstractPropertyHandler<AnimationSettingsValue>
{
    public override string PropertyName => "AnimationSettings";

    public override AnimationSettingsValue? GetValue(IMajorRecordGetter record) =>
        record is IImageSpaceAdapterGetter adapter
            ? new AnimationSettingsValue(adapter.Animatable, adapter.Duration)
            : null;

    public override void SetValue(IMajorRecord record, AnimationSettingsValue? value)
    {
        if (record is not IImageSpaceAdapter adapter || value == null)
        {
            return;
        }

        adapter.Animatable = value.Animatable;
        adapter.Duration = value.Duration;
    }

    public override bool AreValuesEqual(AnimationSettingsValue? value1, AnimationSettingsValue? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return value1.Animatable == value2.Animatable
            && value1.Duration.EqualsWithin(value2.Duration, 0.001f);
    }
}

public sealed class AtomicKeyFrameCurveHandler
    : AbstractPropertyHandler<IReadOnlyList<IKeyFrameGetter>>
{
    private readonly Func<IImageSpaceAdapterGetter, IReadOnlyList<IKeyFrameGetter>?> _getter;
    private readonly Action<IImageSpaceAdapter, ExtendedList<KeyFrame>?> _setter;

    public AtomicKeyFrameCurveHandler(
        string propertyName,
        Func<IImageSpaceAdapterGetter, IReadOnlyList<IKeyFrameGetter>?> getter,
        Action<IImageSpaceAdapter, ExtendedList<KeyFrame>?> setter)
    {
        PropertyName = propertyName;
        _getter = getter;
        _setter = setter;
    }

    public override string PropertyName { get; }

    public override IReadOnlyList<IKeyFrameGetter>? GetValue(IMajorRecordGetter record) =>
        record is IImageSpaceAdapterGetter adapter ? _getter(adapter) : null;

    public override void SetValue(IMajorRecord record, IReadOnlyList<IKeyFrameGetter>? value)
    {
        if (record is IImageSpaceAdapter adapter)
        {
            _setter(adapter, ImageSpaceAdapterCurveComparison.Copy(value));
        }
    }

    public override bool AreValuesEqual(
        IReadOnlyList<IKeyFrameGetter>? value1,
        IReadOnlyList<IKeyFrameGetter>? value2) =>
        ImageSpaceAdapterCurveComparison.AreEqual(value1, value2);
}

public sealed class AtomicColorFrameCurveHandler
    : AbstractPropertyHandler<IReadOnlyList<IColorFrameGetter>>
{
    private readonly Func<IImageSpaceAdapterGetter, IReadOnlyList<IColorFrameGetter>?> _getter;
    private readonly Action<IImageSpaceAdapter, ExtendedList<ColorFrame>?> _setter;

    public AtomicColorFrameCurveHandler(
        string propertyName,
        Func<IImageSpaceAdapterGetter, IReadOnlyList<IColorFrameGetter>?> getter,
        Action<IImageSpaceAdapter, ExtendedList<ColorFrame>?> setter)
    {
        PropertyName = propertyName;
        _getter = getter;
        _setter = setter;
    }

    public override string PropertyName { get; }

    public override IReadOnlyList<IColorFrameGetter>? GetValue(IMajorRecordGetter record) =>
        record is IImageSpaceAdapterGetter adapter ? _getter(adapter) : null;

    public override void SetValue(IMajorRecord record, IReadOnlyList<IColorFrameGetter>? value)
    {
        if (record is IImageSpaceAdapter adapter)
        {
            _setter(adapter, ImageSpaceAdapterCurveComparison.Copy(value));
        }
    }

    public override bool AreValuesEqual(
        IReadOnlyList<IColorFrameGetter>? value1,
        IReadOnlyList<IColorFrameGetter>? value2) =>
        ImageSpaceAdapterCurveComparison.AreEqual(value1, value2);
}

public sealed class KeyFrameCurvePairHandler : AbstractPropertyHandler<KeyFrameCurvePairValue>
{
    private readonly Func<IImageSpaceAdapterGetter, IReadOnlyList<IKeyFrameGetter>?> _multGetter;
    private readonly Func<IImageSpaceAdapterGetter, IReadOnlyList<IKeyFrameGetter>?> _addGetter;
    private readonly Action<IImageSpaceAdapter, ExtendedList<KeyFrame>?> _multSetter;
    private readonly Action<IImageSpaceAdapter, ExtendedList<KeyFrame>?> _addSetter;

    public KeyFrameCurvePairHandler(
        string propertyName,
        Func<IImageSpaceAdapterGetter, IReadOnlyList<IKeyFrameGetter>?> multGetter,
        Func<IImageSpaceAdapterGetter, IReadOnlyList<IKeyFrameGetter>?> addGetter,
        Action<IImageSpaceAdapter, ExtendedList<KeyFrame>?> multSetter,
        Action<IImageSpaceAdapter, ExtendedList<KeyFrame>?> addSetter)
    {
        PropertyName = propertyName;
        _multGetter = multGetter;
        _addGetter = addGetter;
        _multSetter = multSetter;
        _addSetter = addSetter;
    }

    public override string PropertyName { get; }

    public override KeyFrameCurvePairValue? GetValue(IMajorRecordGetter record) =>
        record is IImageSpaceAdapterGetter adapter
            ? new KeyFrameCurvePairValue(_multGetter(adapter), _addGetter(adapter))
            : null;

    public override void SetValue(IMajorRecord record, KeyFrameCurvePairValue? value)
    {
        if (record is not IImageSpaceAdapter adapter || value == null)
        {
            return;
        }

        _multSetter(adapter, ImageSpaceAdapterCurveComparison.Copy(value.Mult));
        _addSetter(adapter, ImageSpaceAdapterCurveComparison.Copy(value.Add));
    }

    public override bool AreValuesEqual(KeyFrameCurvePairValue? value1, KeyFrameCurvePairValue? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return ImageSpaceAdapterCurveComparison.AreEqual(value1.Mult, value2.Mult)
            && ImageSpaceAdapterCurveComparison.AreEqual(value1.Add, value2.Add);
    }
}

public sealed class RadialBlurHandler : AbstractPropertyHandler<RadialBlurValue>
{
    public override string PropertyName => "RadialBlur";

    public override RadialBlurValue? GetValue(IMajorRecordGetter record) =>
        record is IImageSpaceAdapterGetter adapter
            ? new RadialBlurValue(
                adapter.RadialBlurUseTarget,
                adapter.RadialBlurCenter,
                adapter.RadialBlurStrength,
                adapter.RadialBlurRampUp,
                adapter.RadialBlurStart,
                adapter.RadialBlurRampDown,
                adapter.RadialBlurDownStart)
            : null;

    public override void SetValue(IMajorRecord record, RadialBlurValue? value)
    {
        if (record is not IImageSpaceAdapter adapter || value == null)
        {
            return;
        }

        adapter.RadialBlurUseTarget = value.UseTarget;
        adapter.RadialBlurCenter = value.Center;
        adapter.RadialBlurStrength = ImageSpaceAdapterCurveComparison.Copy(value.Strength);
        adapter.RadialBlurRampUp = ImageSpaceAdapterCurveComparison.Copy(value.RampUp);
        adapter.RadialBlurStart = ImageSpaceAdapterCurveComparison.Copy(value.Start);
        adapter.RadialBlurRampDown = ImageSpaceAdapterCurveComparison.Copy(value.RampDown);
        adapter.RadialBlurDownStart = ImageSpaceAdapterCurveComparison.Copy(value.DownStart);
    }

    public override bool AreValuesEqual(RadialBlurValue? value1, RadialBlurValue? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return value1.UseTarget == value2.UseTarget
            && value1.Center.Equals(value2.Center)
            && ImageSpaceAdapterCurveComparison.AreEqual(value1.Strength, value2.Strength)
            && ImageSpaceAdapterCurveComparison.AreEqual(value1.RampUp, value2.RampUp)
            && ImageSpaceAdapterCurveComparison.AreEqual(value1.Start, value2.Start)
            && ImageSpaceAdapterCurveComparison.AreEqual(value1.RampDown, value2.RampDown)
            && ImageSpaceAdapterCurveComparison.AreEqual(value1.DownStart, value2.DownStart);
    }
}

public sealed class DepthOfFieldHandler : AbstractPropertyHandler<DepthOfFieldValue>
{
    public override string PropertyName => "DepthOfField";

    public override DepthOfFieldValue? GetValue(IMajorRecordGetter record) =>
        record is IImageSpaceAdapterGetter adapter
            ? new DepthOfFieldValue(
                adapter.DepthOfFieldFlags,
                adapter.DepthOfFieldStrength,
                adapter.DepthOfFieldDistance,
                adapter.DepthOfFieldRange)
            : null;

    public override void SetValue(IMajorRecord record, DepthOfFieldValue? value)
    {
        if (record is not IImageSpaceAdapter adapter || value == null)
        {
            return;
        }

        adapter.DepthOfFieldFlags = value.Flags;
        adapter.DepthOfFieldStrength = ImageSpaceAdapterCurveComparison.Copy(value.Strength);
        adapter.DepthOfFieldDistance = ImageSpaceAdapterCurveComparison.Copy(value.Distance);
        adapter.DepthOfFieldRange = ImageSpaceAdapterCurveComparison.Copy(value.Range);
    }

    public override bool AreValuesEqual(DepthOfFieldValue? value1, DepthOfFieldValue? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return value1.Flags == value2.Flags
            && ImageSpaceAdapterCurveComparison.AreEqual(value1.Strength, value2.Strength)
            && ImageSpaceAdapterCurveComparison.AreEqual(value1.Distance, value2.Distance)
            && ImageSpaceAdapterCurveComparison.AreEqual(value1.Range, value2.Range);
    }
}

internal static class ImageSpaceAdapterCurveComparison
{
    public static bool AreEqual(
        IReadOnlyList<IKeyFrameGetter>? value1,
        IReadOnlyList<IKeyFrameGetter>? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        if (value1.Count != value2.Count)
        {
            return false;
        }

        for (var i = 0; i < value1.Count; i++)
        {
            if (!value1[i].Time.EqualsWithin(value2[i].Time, 1E-09f)
                || !value1[i].Value.EqualsWithin(value2[i].Value, 1E-09f))
            {
                return false;
            }
        }

        return true;
    }

    public static bool AreEqual(
        IReadOnlyList<IColorFrameGetter>? value1,
        IReadOnlyList<IColorFrameGetter>? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        if (value1.Count != value2.Count)
        {
            return false;
        }

        for (var i = 0; i < value1.Count; i++)
        {
            if (!value1[i].Time.EqualsWithin(value2[i].Time, 1E-09f)
                || !ColorsEqual(value1[i].Color, value2[i].Color))
            {
                return false;
            }
        }

        return true;
    }

    public static ExtendedList<KeyFrame>? Copy(IReadOnlyList<IKeyFrameGetter>? value)
    {
        if (value == null)
        {
            return null;
        }

        var copy = new ExtendedList<KeyFrame>();
        foreach (var frame in value)
        {
            copy.Add(frame.DeepCopy());
        }

        return copy;
    }

    public static ExtendedList<ColorFrame>? Copy(IReadOnlyList<IColorFrameGetter>? value)
    {
        if (value == null)
        {
            return null;
        }

        var copy = new ExtendedList<ColorFrame>();
        foreach (var frame in value)
        {
            copy.Add(frame.DeepCopy());
        }

        return copy;
    }

    private static bool ColorsEqual(Color value1, Color value2) =>
        value1.R == value2.R
        && value1.G == value2.G
        && value1.B == value2.B;
}

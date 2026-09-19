using DreadsMashedPatch.PropertyHandlers.ImageSpaceAdapter;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class ImageSpaceAdapterRecordHandlerTests
{
    private static readonly ModKey TestModKey = ModKey.FromNameAndExtension("ImageSpaceAdapterTests.esp");

    [Fact]
    public void RegistersSemanticAtomicGroupsInsteadOfIndividualGroupMembers()
    {
        var handlers = new ImageSpaceAdapterRecordHandler().PropertyHandlers;

        Assert.IsType<AnimationSettingsHandler>(handlers["AnimationSettings"]);
        Assert.IsType<RadialBlurHandler>(handlers["RadialBlur"]);
        Assert.IsType<DepthOfFieldHandler>(handlers["DepthOfField"]);
        Assert.IsType<KeyFrameCurvePairHandler>(handlers["HdrBloomScale"]);
        Assert.IsType<KeyFrameCurvePairHandler>(handlers["CinematicContrast"]);

        Assert.DoesNotContain("Animatable", handlers.Keys);
        Assert.DoesNotContain("RadialBlurStrength", handlers.Keys);
        Assert.DoesNotContain("DepthOfFieldFlags", handlers.Keys);
        Assert.DoesNotContain("HdrBloomScaleMult", handlers.Keys);
        Assert.DoesNotContain("HdrBloomScaleAdd", handlers.Keys);
    }

    [Fact]
    public void AtomicCurvePreservesOrderDuplicatesNullStateAndCopiesFrames()
    {
        var handler = Assert.IsType<AtomicKeyFrameCurveHandler>(
            new ImageSpaceAdapterRecordHandler().PropertyHandlers["BlurRadius"]);
        var source = CreateAdapter(0x800);
        var target = CreateAdapter(0x801);
        source.BlurRadius = new ExtendedList<KeyFrame>
        {
            Frame(0.25f, 1f),
            Frame(0.75f, 2f),
            Frame(0.25f, 1f)
        };

        var value = handler.GetValue(source);
        handler.SetValue(target, value);
        var targetCurve = target.BlurRadius!;

        Assert.Equal([0.25f, 0.75f, 0.25f], targetCurve.Select(frame => frame.Time));
        Assert.Equal([1f, 2f, 1f], targetCurve.Select(frame => frame.Value));
        Assert.False(handler.AreValuesEqual(value, [value![1], value[0], value[2]]));
        Assert.False(handler.AreValuesEqual(null, Array.Empty<IKeyFrameGetter>()));

        source.BlurRadius[0].Value = 99f;
        Assert.Equal(1f, targetCurve[0].Value);
    }

    [Fact]
    public void RadialBlurAndDepthOfFieldMoveAsCompleteGroups()
    {
        var handlers = new ImageSpaceAdapterRecordHandler().PropertyHandlers;
        var radialHandler = Assert.IsType<RadialBlurHandler>(handlers["RadialBlur"]);
        var depthHandler = Assert.IsType<DepthOfFieldHandler>(handlers["DepthOfField"]);
        var source = CreateAdapter(0x802);
        var target = CreateAdapter(0x803);

        source.RadialBlurUseTarget = true;
        source.RadialBlurCenter = new P2Float(0.2f, 0.8f);
        source.RadialBlurStrength = new ExtendedList<KeyFrame> { Frame(0f, 1f), Frame(1f, 4f) };
        source.RadialBlurRampUp = new ExtendedList<KeyFrame> { Frame(0.5f, 2f) };
        source.RadialBlurStart = new ExtendedList<KeyFrame>();
        source.RadialBlurRampDown = null;
        source.RadialBlurDownStart = new ExtendedList<KeyFrame> { Frame(0.75f, 3f) };
        source.DepthOfFieldFlags = Mutagen.Bethesda.Skyrim.ImageSpaceAdapter.DepthOfFieldFlag.UseTarget
            | Mutagen.Bethesda.Skyrim.ImageSpaceAdapter.DepthOfFieldFlag.NoSky;
        source.DepthOfFieldStrength = new ExtendedList<KeyFrame> { Frame(0f, 5f) };
        source.DepthOfFieldDistance = new ExtendedList<KeyFrame> { Frame(0f, 6f) };
        source.DepthOfFieldRange = new ExtendedList<KeyFrame> { Frame(0f, 7f) };

        radialHandler.SetValue(target, radialHandler.GetValue(source));
        depthHandler.SetValue(target, depthHandler.GetValue(source));

        Assert.True(target.RadialBlurUseTarget);
        Assert.Equal(source.RadialBlurCenter, target.RadialBlurCenter);
        Assert.Equal([1f, 4f], target.RadialBlurStrength!.Select(frame => frame.Value));
        Assert.Empty(target.RadialBlurStart!);
        Assert.Null(target.RadialBlurRampDown);
        Assert.Equal(source.DepthOfFieldFlags, target.DepthOfFieldFlags);
        Assert.Equal(6f, Assert.Single(target.DepthOfFieldDistance!).Value);
    }

    [Fact]
    public void MultAndAddCurvesAreCopiedTogetherAndSerializeWithDerivedCounts()
    {
        var pairHandler = Assert.IsType<KeyFrameCurvePairHandler>(
            new ImageSpaceAdapterRecordHandler().PropertyHandlers["HdrBloomScale"]);
        var source = CreateAdapter(0x804);
        var target = CreateAdapter(0x805);
        source.HdrBloomScaleMult = new ExtendedList<KeyFrame> { Frame(0f, 1f), Frame(1f, 2f) };
        source.HdrBloomScaleAdd = new ExtendedList<KeyFrame> { Frame(0f, 3f) };

        pairHandler.SetValue(target, pairHandler.GetValue(source));

        var mod = new SkyrimMod(TestModKey, SkyrimRelease.SkyrimSE);
        mod.ImageSpaceAdapters.Add(target);
        using var stream = new MemoryStream();
        mod.WriteToBinary(stream);
        stream.Position = 0;
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(stream, SkyrimRelease.SkyrimSE, TestModKey);
        var written = Assert.Single(overlay.ImageSpaceAdapters);
        var writtenMult = written.HdrBloomScaleMult!;
        var writtenAdd = written.HdrBloomScaleAdd!;

        Assert.Equal(2, writtenMult.Count);
        Assert.Single(writtenAdd);
        Assert.Equal([1f, 2f], writtenMult.Select(frame => frame.Value));
        Assert.Equal(3f, writtenAdd[0].Value);
    }

    [Fact]
    public void ImageSpaceAdaptersAreExplicitlyDisabledInProgram()
    {
        Assert.DoesNotContain(typeof(IImageSpaceAdapterGetter), Program.SupportedRecordTypes);
        var reason = Assert.Contains(typeof(IImageSpaceAdapterGetter), Program.ExcludedRecordTypes);
        Assert.Contains("DNAM", reason, StringComparison.Ordinal);
    }

    private static Mutagen.Bethesda.Skyrim.ImageSpaceAdapter CreateAdapter(uint id) =>
        new(new FormKey(TestModKey, id), SkyrimRelease.SkyrimSE);

    private static KeyFrame Frame(float time, float value) =>
        new() { Time = time, Value = value };
}

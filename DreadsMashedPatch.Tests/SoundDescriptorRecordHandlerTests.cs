using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.SoundDescriptor;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class SoundDescriptorRecordHandlerTests
{
    private static readonly ModKey TestModKey = ModKey.FromNameAndExtension("SoundDescriptorTests.esp");

    [Fact]
    public void UsesAtomicBnamPairsAndIndependentPriority()
    {
        var handlers = new SoundDescriptorRecordHandler().PropertyHandlers;

        Assert.IsType<ComplexReflectionPropertyHandler<ITranslatedStringGetter, ISoundDescriptor, ISoundDescriptorGetter>>(
            handlers["String"]);
        Assert.IsType<SoundDescriptorPitchHandler>(handlers["Pitch"]);
        Assert.IsType<SoundDescriptorVolumeHandler>(handlers["Volume"]);
        Assert.IsType<SimpleReflectionPropertyHandler<byte, ISoundDescriptor, ISoundDescriptorGetter>>(
            handlers["Priority"]);
        Assert.DoesNotContain("PercentFrequencyShift", handlers.Keys);
        Assert.DoesNotContain("PercentFrequencyVariance", handlers.Keys);
        Assert.DoesNotContain("Variance", handlers.Keys);
        Assert.DoesNotContain("StaticAttenuation", handlers.Keys);
    }

    [Fact]
    public void ReadsAndWritesAtomicBnamPairs()
    {
        var handlers = new SoundDescriptorRecordHandler().PropertyHandlers;
        var source = CreateDescriptor(0x800);
        source.PercentFrequencyShift = -12;
        source.PercentFrequencyVariance = 23;
        source.Priority = 191;
        source.Variance = 207;
        source.StaticAttenuation = 12.5f;
        var target = CreateDescriptor(0x801);

        foreach (var propertyName in new[] { "Pitch", "Priority", "Volume" })
        {
            var handler = handlers[propertyName];
            handler.SetValue(target, handler.GetValue(source));
        }

        Assert.Equal((sbyte)-12, target.PercentFrequencyShift);
        Assert.Equal((sbyte)23, target.PercentFrequencyVariance);
        Assert.Equal((byte)191, target.Priority);
        Assert.Equal((byte)207, target.Variance);
        Assert.Equal(12.5f, target.StaticAttenuation);
    }

    [Fact]
    public void ChangingEitherMemberChangesTheWholeSemanticGroup()
    {
        var pitchHandler = new SoundDescriptorPitchHandler();
        var volumeHandler = new SoundDescriptorVolumeHandler();

        Assert.False(pitchHandler.AreValuesEqual(
            new SoundDescriptorPitch(-12, 23),
            new SoundDescriptorPitch(-12, 24)));
        Assert.False(volumeHandler.AreValuesEqual(
            new SoundDescriptorVolume(20, 12.5f),
            new SoundDescriptorVolume(21, 12.5f)));
    }

    [Fact]
    public void ReadsAndDeepCopiesTranslatedString()
    {
        var handler = new SoundDescriptorRecordHandler().PropertyHandlers["String"];
        var source = CreateDescriptor(0x802);
        source.String = "Forwarded sound text";
        var target = CreateDescriptor(0x803);

        handler.SetValue(target, handler.GetValue(source));

        Assert.NotNull(target.String);
        Assert.Equal("Forwarded sound text", target.String.String);
        Assert.NotSame(source.String, target.String);
    }

    private static SoundDescriptor CreateDescriptor(uint id) =>
        new(new FormKey(TestModKey, id), SkyrimRelease.SkyrimSE);
}

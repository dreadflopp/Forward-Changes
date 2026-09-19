using DreadsMashedPatch.PropertyHandlers.General;
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
    public void UsesMutagenTypesForScalarAndTranslatedStringFields()
    {
        var handlers = new SoundDescriptorRecordHandler().PropertyHandlers;

        Assert.IsType<ComplexReflectionPropertyHandler<ITranslatedStringGetter, ISoundDescriptor, ISoundDescriptorGetter>>(
            handlers["String"]);
        Assert.IsType<SimpleReflectionPropertyHandler<sbyte, ISoundDescriptor, ISoundDescriptorGetter>>(
            handlers["PercentFrequencyShift"]);
        Assert.IsType<SimpleReflectionPropertyHandler<sbyte, ISoundDescriptor, ISoundDescriptorGetter>>(
            handlers["PercentFrequencyVariance"]);
        Assert.IsType<SimpleReflectionPropertyHandler<byte, ISoundDescriptor, ISoundDescriptorGetter>>(
            handlers["Priority"]);
        Assert.IsType<SimpleReflectionPropertyHandler<byte, ISoundDescriptor, ISoundDescriptorGetter>>(
            handlers["Variance"]);
    }

    [Fact]
    public void ReadsAndWritesActualScalarValues()
    {
        var handlers = new SoundDescriptorRecordHandler().PropertyHandlers;
        var source = CreateDescriptor(0x800);
        source.PercentFrequencyShift = -12;
        source.PercentFrequencyVariance = 23;
        source.Priority = 191;
        source.Variance = 207;
        var target = CreateDescriptor(0x801);

        foreach (var propertyName in new[]
                 {
                     "PercentFrequencyShift",
                     "PercentFrequencyVariance",
                     "Priority",
                     "Variance"
                 })
        {
            var handler = handlers[propertyName];
            handler.SetValue(target, handler.GetValue(source));
        }

        Assert.Equal((sbyte)-12, target.PercentFrequencyShift);
        Assert.Equal((sbyte)23, target.PercentFrequencyVariance);
        Assert.Equal((byte)191, target.Priority);
        Assert.Equal((byte)207, target.Variance);
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

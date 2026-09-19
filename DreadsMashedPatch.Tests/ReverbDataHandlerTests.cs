using ForwardChanges.PropertyHandlers.ReverbParameters;
using ForwardChanges.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class ReverbDataHandlerTests
{
    private static readonly ModKey TestModKey = ModKey.FromNameAndExtension("Test.esp");
    private readonly ReverbDataHandler _handler = new();

    [Fact]
    public void RecordHandlerRegistersExactlyOneAtomicDataProperty()
    {
        var recordHandler = new ReverbParametersRecordHandler();

        Assert.Equal(
            ["EditorID", "MajorRecordFlagsRaw", "SkyrimMajorRecordFlags", "ReverbData"],
            recordHandler.PropertyHandlers.Keys);
        Assert.IsType<ReverbDataHandler>(recordHandler.PropertyHandlers["ReverbData"]);
    }

    [Fact]
    public void AnyMemberDifferenceChangesTheAtomicSnapshot()
    {
        var first = CreateReverb(0x800);
        var second = CreateReverb(0x801);
        first.ReverbAmp = 3;
        second.ReverbAmp = 5;

        Assert.False(_handler.AreValuesEqual(
            _handler.GetValue(first),
            _handler.GetValue(second)));
    }

    [Fact]
    public void SetValueCopiesTheCompletePackedDataIncludingUnknown()
    {
        var source = CreateReverb(0x800);
        source.DecayMilliseconds = 101;
        source.HfReferenceHertz = 202;
        source.RoomFilter = -3;
        source.RoomHfFilter = -4;
        source.Reflections = 5;
        source.ReverbAmp = 6;
        source.DecayHfRatio = 0.77f;
        source.ReflectDelayMS = 8;
        source.ReverbDelayMS = 9;
        source.DiffusionPercent = Percent.FactoryPutInRange(0.42);
        source.DensityPercent = Percent.FactoryPutInRange(0.84);
        source.Unknown = 12;
        var target = CreateReverb(0x801);

        _handler.SetValue(target, _handler.GetValue(source));

        Assert.Equal(_handler.GetValue(source), _handler.GetValue(target));
        Assert.Equal((byte)12, target.Unknown);
    }

    [Fact]
    public void EditorIdAndFlagsDoNotAffectTheDataSnapshot()
    {
        var first = CreateReverb(0x800);
        var second = CreateReverb(0x801);
        first.EditorID = "First";
        second.EditorID = "Second";
        first.MajorRecordFlagsRaw = 1;
        second.MajorRecordFlagsRaw = 2;

        Assert.True(_handler.AreValuesEqual(
            _handler.GetValue(first),
            _handler.GetValue(second)));
    }

    private static ReverbParameters CreateReverb(uint id) =>
        new(new FormKey(TestModKey, id), SkyrimRelease.SkyrimSE);
}

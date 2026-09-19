using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class SoundOutputModelRecordHandlerTests
{
    private static readonly ModKey SourceModKey = ModKey.FromNameAndExtension("SoundOutputSource.esp");
    private static readonly ModKey PatchModKey = ModKey.FromNameAndExtension("SoundOutputPatch.esp");

    [Fact]
    public void UsesGeneratedCopiesForOverlayAggregates()
    {
        var handlers = new SoundOutputModelRecordHandler().PropertyHandlers;

        Assert.IsType<GeneratedCopyReflectionPropertyHandler<ISoundOutputDataGetter, SoundOutputData, ISoundOutputModel, ISoundOutputModelGetter>>(
            handlers["Data"]);
        Assert.IsType<GeneratedCopyReflectionPropertyHandler<ISoundOutputChannelsGetter, SoundOutputChannels, ISoundOutputModel, ISoundOutputModelGetter>>(
            handlers["OutputChannels"]);
        Assert.IsType<GeneratedCopyReflectionPropertyHandler<ISoundOutputAttenuationGetter, SoundOutputAttenuation, ISoundOutputModel, ISoundOutputModelGetter>>(
            handlers["Attenuation"]);
    }

    [Fact]
    public void OverlayForwardingPreservesCurveUnknownBytesAndChannels()
    {
        var sourceMod = new SkyrimMod(SourceModKey, SkyrimRelease.SkyrimSE);
        var sourceRecord = new SoundOutputModel(
            new FormKey(SourceModKey, 0x800), SkyrimRelease.SkyrimSE)
        {
            Data = new SoundOutputData
            {
                Flags = SoundOutputModel.Flag.AttenuatesWithDistance,
                Unknown = 0x1234
            },
            OutputChannels = new SoundOutputChannels
            {
                Channel0 = new SoundOutputChannel { L = 1, R = 2, C = 3, LFE = 4, RL = 5, RR = 6, BL = 7, BR = 8 },
                Channel1 = new SoundOutputChannel { L = 11, R = 12, C = 13, LFE = 14, RL = 15, RR = 16, BL = 17, BR = 18 },
                Channel2 = new SoundOutputChannel { L = 21, R = 22, C = 23, LFE = 24, RL = 25, RR = 26, BL = 27, BR = 28 }
            },
            Attenuation = new SoundOutputAttenuation
            {
                Unknown = 0x10203040,
                MinDistance = 12.5f,
                MaxDistance = 987.25f,
                Curve = new byte[] { 9, 17, 33, 65, 129 },
                Unknown2 = new byte[] { 7, 11, 13 }
            }
        };
        sourceMod.SoundOutputModels.Add(sourceRecord);

        using var sourceStream = new MemoryStream();
        sourceMod.WriteToBinary(sourceStream);
        sourceStream.Position = 0;
        using var sourceOverlay = SkyrimMod.CreateFromBinaryOverlay(
            sourceStream, SkyrimRelease.SkyrimSE, SourceModKey);
        var overlayRecord = Assert.Single(sourceOverlay.SoundOutputModels);
        var target = new SoundOutputModel(
            new FormKey(PatchModKey, 0x800), SkyrimRelease.SkyrimSE);
        var handlers = new SoundOutputModelRecordHandler().PropertyHandlers;

        foreach (var propertyName in new[] { "Data", "OutputChannels", "Attenuation" })
        {
            var handler = handlers[propertyName];
            handler.SetValue(target, handler.GetValue(overlayRecord));
        }

        Assert.Equal(SoundOutputModel.Flag.AttenuatesWithDistance, target.Data!.Flags);
        Assert.Equal((ushort)0x1234, target.Data.Unknown);
        Assert.Equal(new byte[] { 9, 17, 33, 65, 129 }, target.Attenuation!.Curve.ToArray());
        Assert.Equal(new byte[] { 7, 11, 13 }, target.Attenuation.Unknown2.ToArray());
        Assert.Equal(12.5f, target.Attenuation.MinDistance);
        Assert.Equal(987.25f, target.Attenuation.MaxDistance);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, ChannelValues(target.OutputChannels!.Channel0));
        Assert.Equal(new byte[] { 11, 12, 13, 14, 15, 16, 17, 18 }, ChannelValues(target.OutputChannels.Channel1));
        Assert.Equal(new byte[] { 21, 22, 23, 24, 25, 26, 27, 28 }, ChannelValues(target.OutputChannels.Channel2));
    }

    private static byte[] ChannelValues(ISoundOutputChannelGetter channel) =>
        [channel.L, channel.R, channel.C, channel.LFE, channel.RL, channel.RR, channel.BL, channel.BR];
}

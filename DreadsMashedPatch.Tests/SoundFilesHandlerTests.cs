using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.SoundDescriptor;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class SoundFilesHandlerTests
{
    private static readonly ModKey TestModKey = ModKey.FromNameAndExtension("SoundTests.esp");
    private readonly SoundFilesHandler _handler = new();

    [Fact]
    public void UsesExactIndexedOrder()
    {
        Assert.Equal(ListSemantics.ExactOrdered, _handler.Semantics);

        var forward = CreateLinks("fx/one.wav", "fx/two.wav");
        var reversed = CreateLinks("fx/two.wav", "fx/one.wav");

        Assert.False(_handler.AreValuesEqual(forward, reversed));
    }

    [Theory]
    [InlineData("data\\Sound\\fx\\wpn\\bash\\blade\\wpn_bash_blade_01.wav")]
    [InlineData("fx\\wpn\\bash\\blade\\WPN_Bash_Blade_02.wav")]
    [InlineData("Sound\\fx\\wpn\\bash\\blade\\WPN_Bash_Blade_02.wav")]
    public void SetValuePreservesExactSerializedGivenPath(string path)
    {
        var source = CreateDescriptor(0x800);
        source.SoundFiles.Add(new AssetLink<SkyrimSoundAssetType>(path));
        var target = CreateDescriptor(0x801);

        _handler.SetValue(target, _handler.GetValue(source));

        Assert.Equal(path, Assert.Single(target.SoundFiles).GivenPath);
    }

    [Fact]
    public void DataPrefixIsNotHiddenFromSemanticComparison()
    {
        var prefixed = CreateLinks("data\\Sound\\fx\\example.wav");
        var relative = CreateLinks("fx\\example.wav");

        Assert.False(_handler.AreValuesEqual(prefixed, relative));
    }

    [Fact]
    public void ComparisonIgnoresOnlyCaseAndSeparatorSpelling()
    {
        var first = CreateLinks("fx\\WPN\\Example.wav");
        var second = CreateLinks("FX/Wpn/example.WAV");

        Assert.True(_handler.AreValuesEqual(first, second));
    }

    private static List<IAssetLinkGetter<SkyrimSoundAssetType>> CreateLinks(params string[] paths) =>
        paths.Select(path => (IAssetLinkGetter<SkyrimSoundAssetType>)new AssetLink<SkyrimSoundAssetType>(path)).ToList();

    private static SoundDescriptor CreateDescriptor(uint id) =>
        new(new FormKey(TestModKey, id), SkyrimRelease.SkyrimSE);
}

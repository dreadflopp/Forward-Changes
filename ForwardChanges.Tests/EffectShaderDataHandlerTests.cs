using ForwardChanges.PropertyHandlers.EffectShader;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Xunit;
using EffectShaderRecord = Mutagen.Bethesda.Skyrim.EffectShader;

namespace ForwardChanges.Tests;

public sealed class EffectShaderDataHandlerTests
{
    private static readonly ModKey TestModKey = ModKey.FromNameAndExtension("Test.esp");
    private readonly EffectShaderDataHandler _handler = new();

    [Fact]
    public void DetectsDataFloatDifferenceBelowLegacyTolerance()
    {
        var first = CreateEffectShader(0xA00);
        var second = CreateEffectShader(0xA01);
        first.FillAlphaFadeInTime = 1.0f;
        second.FillAlphaFadeInTime = 1.0005f;

        Assert.False(_handler.AreValuesEqual(first, second));
    }

    [Fact]
    public void DetectsUnknownDataDifference()
    {
        var first = CreateEffectShader(0xA00);
        var second = CreateEffectShader(0xA01);
        first.Unknown = 1;
        second.Unknown = 2;

        Assert.False(_handler.AreValuesEqual(first, second));
    }

    [Fact]
    public void IgnoresPropertiesHandledOutsideData()
    {
        var first = CreateEffectShader(0xA00);
        var second = CreateEffectShader(0xA01);
        first.EditorID = "First";
        second.EditorID = "Second";
        first.FillTexture = new AssetLink<SkyrimTextureAssetType>("Effects/First.dds");
        second.FillTexture = new AssetLink<SkyrimTextureAssetType>("Effects/Second.dds");
        first.ParticleShaderTexture = new AssetLink<SkyrimTextureAssetType>("Effects/FirstParticle.dds");
        second.ParticleShaderTexture = new AssetLink<SkyrimTextureAssetType>("Effects/SecondParticle.dds");
        first.HolesTexture = new AssetLink<SkyrimTextureAssetType>("Effects/FirstHoles.dds");
        second.HolesTexture = new AssetLink<SkyrimTextureAssetType>("Effects/SecondHoles.dds");
        first.MembranePaletteTexture = new AssetLink<SkyrimTextureAssetType>("Effects/FirstMembrane.dds");
        second.MembranePaletteTexture = new AssetLink<SkyrimTextureAssetType>("Effects/SecondMembrane.dds");
        first.ParticlePaletteTexture = new AssetLink<SkyrimTextureAssetType>("Effects/FirstPalette.dds");
        second.ParticlePaletteTexture = new AssetLink<SkyrimTextureAssetType>("Effects/SecondPalette.dds");

        Assert.True(_handler.AreValuesEqual(first, second));
    }

    [Fact]
    public void IgnoresDataSerializationState()
    {
        var first = CreateEffectShader(0xA00);
        var second = CreateEffectShader(0xA01);
        first.DATADataTypeState = EffectShaderRecord.DATADataType.Break0;
        second.DATADataTypeState = EffectShaderRecord.DATADataType.Break3;

        Assert.True(_handler.AreValuesEqual(first, second));
    }

    private static EffectShaderRecord CreateEffectShader(uint id) =>
        new(new FormKey(TestModKey, id), SkyrimRelease.SkyrimSE);
}

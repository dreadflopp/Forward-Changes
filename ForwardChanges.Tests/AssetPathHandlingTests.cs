using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Debris;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.MusicTrack;
using ForwardChanges.PropertyHandlers.Static;
using ForwardChanges.PropertyHandlers.Weather;
using ForwardChanges.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class AssetPathHandlingTests
{
    private static readonly ModKey SourceModKey = ModKey.FromNameAndExtension("AssetPathSource.esp");
    private static readonly ModKey PatchModKey = ModKey.FromNameAndExtension("AssetPathPatch.esp");

    [Fact]
    public void SharedModelPreservesSerializedGivenPath()
    {
        var source = new Model
        {
            File = new AssetLink<SkyrimModelAssetType>("Data\\Meshes\\Actors\\ExactCase.NIF")
        };
        var target = new Light(new FormKey(PatchModKey, 0x800), SkyrimRelease.SkyrimSE);
        var handler = new ForwardChanges.PropertyHandlers.General.ModelHandler();

        handler.SetValue(target, source);

        Assert.Equal(source.File.GivenPath, target.Model!.File.GivenPath);
        Assert.False(handler.AreValuesEqual(
            source,
            new Model { File = new AssetLink<SkyrimModelAssetType>("Actors\\ExactCase.NIF") }));
    }

    [Fact]
    public void ScalarReflectionAssetHandlerCopiesOverlayGivenPathAndWritesIt()
    {
        const string path = "Data\\Meshes\\Furniture\\ExactCase.NIF";
        var source = new SkyrimMod(SourceModKey, SkyrimRelease.SkyrimSE);
        source.Furniture.Add(new Furniture(new FormKey(SourceModKey, 0x801), SkyrimRelease.SkyrimSE)
        {
            ModelFilename = new AssetLink<SkyrimModelAssetType>(path)
        });

        using var stream = new MemoryStream();
        source.WriteToBinary(stream);
        stream.Position = 0;
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(stream, SkyrimRelease.SkyrimSE, SourceModKey);
        var overlayRecord = Assert.Single(overlay.Furniture);
        var target = new Furniture(new FormKey(PatchModKey, 0x802), SkyrimRelease.SkyrimSE);
        var handler = new SimpleReflectionAssetLinkPropertyHandler<SkyrimModelAssetType, IFurniture, IFurnitureGetter>("ModelFilename");

        handler.SetValue(target, handler.GetValue(overlayRecord));

        Assert.Equal(path, target.ModelFilename!.GivenPath);
        AssertSerializedPath(target, path, mod => mod.Furniture.Add(target));
    }

    [Fact]
    public void MusicTrackPreservesSerializedGivenPath()
    {
        const string path = "Data\\Music\\Special\\ExactCase.XWM";
        var source = new MusicTrack(new FormKey(SourceModKey, 0x803), SkyrimRelease.SkyrimSE)
        {
            TrackFilename = new AssetLink<SkyrimMusicAssetType>(path)
        };
        var target = new MusicTrack(new FormKey(PatchModKey, 0x804), SkyrimRelease.SkyrimSE);
        var handler = new TrackAssetLinkHandler("TrackFilename");

        handler.SetValue(target, handler.GetValue(source));

        Assert.Equal(path, target.TrackFilename!.GivenPath);
    }

    [Fact]
    public void StaticLodPreservesEverySerializedGivenPath()
    {
        var source = new Lod
        {
            Level0 = new AssetLink<SkyrimModelAssetType>("Data\\Meshes\\LOD\\Zero.NIF"),
            Level1 = new AssetLink<SkyrimModelAssetType>("Meshes\\LOD\\One.NIF"),
            Level2 = new AssetLink<SkyrimModelAssetType>("LOD\\Two.NIF"),
            Level3 = new AssetLink<SkyrimModelAssetType>("LOD/Three.NIF")
        };
        var target = new Mutagen.Bethesda.Skyrim.Static(
            new FormKey(PatchModKey, 0x805), SkyrimRelease.SkyrimSE);
        var handler = new LodHandler();

        handler.SetValue(target, source);

        Assert.Equal(source.Level0.GivenPath, target.Lod!.Level0.GivenPath);
        Assert.Equal(source.Level1.GivenPath, target.Lod.Level1.GivenPath);
        Assert.Equal(source.Level2.GivenPath, target.Lod.Level2.GivenPath);
        Assert.Equal(source.Level3.GivenPath, target.Lod.Level3.GivenPath);
    }

    [Fact]
    public void WeatherCloudTexturesPreserveIndexedSerializedPaths()
    {
        var source = new Weather(new FormKey(SourceModKey, 0x806), SkyrimRelease.SkyrimSE);
        source.CloudTextures[0] = new AssetLink<SkyrimTextureAssetType>("Data\\Textures\\Sky\\ExactCase.DDS");
        source.CloudTextures[1] = new AssetLink<SkyrimTextureAssetType>("Sky/Second.DDS");
        var target = new Weather(new FormKey(PatchModKey, 0x807), SkyrimRelease.SkyrimSE);
        var handler = new CloudTexturesHandler();

        handler.SetValue(target, handler.GetValue(source));

        Assert.Equal(source.CloudTextures[0]!.GivenPath, target.CloudTextures[0]!.GivenPath);
        Assert.Equal(source.CloudTextures[1]!.GivenPath, target.CloudTextures[1]!.GivenPath);
    }

    [Fact]
    public void RegisteredPathPropertiesNoLongerUseGenericReflectionHandlers()
    {
        Assert.IsType<SimpleReflectionAssetLinkPropertyHandler<SkyrimModelAssetType, IFurniture, IFurnitureGetter>>(
            new FurnitureRecordHandler().PropertyHandlers["ModelFilename"]);
        Assert.IsType<SimpleReflectionIconsPropertyHandler<ILoadScreen, ILoadScreenGetter>>(
            new LoadScreenRecordHandler().PropertyHandlers["Icons"]);
        Assert.IsType<SimpleReflectionModelPropertyHandler<IWeather, IWeatherGetter>>(
            new WeatherRecordHandler().PropertyHandlers["Aurora"]);
    }

    [Fact]
    public void IconsAggregateCopiesOverlayPathsWithoutNormalization()
    {
        const string large = "Data\\Textures\\Interface\\LargeExact.DDS";
        const string small = "Textures\\Interface\\SmallExact.DDS";
        var source = new SkyrimMod(SourceModKey, SkyrimRelease.SkyrimSE);
        source.LoadScreens.Add(new LoadScreen(new FormKey(SourceModKey, 0x808), SkyrimRelease.SkyrimSE)
        {
            Icons = new Icons
            {
                LargeIconFilename = new AssetLink<SkyrimTextureAssetType>(large),
                SmallIconFilename = new AssetLink<SkyrimTextureAssetType>(small)
            }
        });

        using var stream = new MemoryStream();
        source.WriteToBinary(stream);
        stream.Position = 0;
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(stream, SkyrimRelease.SkyrimSE, SourceModKey);
        var sourceRecord = Assert.Single(overlay.LoadScreens);
        var target = new LoadScreen(new FormKey(PatchModKey, 0x809), SkyrimRelease.SkyrimSE);
        var handler = new SimpleReflectionIconsPropertyHandler<ILoadScreen, ILoadScreenGetter>("Icons");

        handler.SetValue(target, handler.GetValue(sourceRecord));

        Assert.Equal(large, target.Icons!.LargeIconFilename.GivenPath);
        Assert.Equal(small, target.Icons.SmallIconFilename!.GivenPath);
    }

    [Fact]
    public void DebrisModelsCopyOverlayModelFilenameWithoutNormalization()
    {
        const string path = "Data\\Meshes\\Debris\\ExactCase.NIF";
        var source = new SkyrimMod(SourceModKey, SkyrimRelease.SkyrimSE);
        var sourceRecord = new Debris(new FormKey(SourceModKey, 0x80A), SkyrimRelease.SkyrimSE);
        sourceRecord.Models.Add(new DebrisModel
        {
            Percentage = 100,
            ModelFilename = new AssetLink<SkyrimModelAssetType>(path)
        });
        source.Debris.Add(sourceRecord);

        using var stream = new MemoryStream();
        source.WriteToBinary(stream);
        stream.Position = 0;
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(stream, SkyrimRelease.SkyrimSE, SourceModKey);
        var overlayRecord = Assert.Single(overlay.Debris);
        var target = new Debris(new FormKey(PatchModKey, 0x80B), SkyrimRelease.SkyrimSE);
        var handler = new ModelsHandler();

        handler.SetValue(target, handler.GetValue(overlayRecord));

        Assert.Equal(path, Assert.Single(target.Models).ModelFilename.GivenPath);
    }

    [Fact]
    public void PathComparisonIgnoresCaseAndSeparatorsButNotPrefixes()
    {
        var handler = new SimpleReflectionAssetLinkPropertyHandler<SkyrimTextureAssetType, IWater, IWaterGetter>("NoiseLayerOneTexture");
        var backslash = new AssetLinkGetter<SkyrimTextureAssetType>("Water\\ExactCase.DDS");
        var slashAndCase = new AssetLinkGetter<SkyrimTextureAssetType>("water/exactcase.dds");
        var prefixed = new AssetLinkGetter<SkyrimTextureAssetType>("Textures\\Water\\ExactCase.DDS");

        Assert.True(handler.AreValuesEqual(backslash, slashAndCase));
        Assert.False(handler.AreValuesEqual(backslash, prefixed));
    }

    private static void AssertSerializedPath<TRecord>(
        TRecord record,
        string expected,
        Action<SkyrimMod> addRecord)
        where TRecord : class
    {
        var patch = new SkyrimMod(PatchModKey, SkyrimRelease.SkyrimSE);
        addRecord(patch);
        using var stream = new MemoryStream();
        patch.WriteToBinary(stream);
        var bytes = System.Text.Encoding.Latin1.GetString(stream.ToArray());
        Assert.Contains(expected, bytes, StringComparison.Ordinal);
    }
}

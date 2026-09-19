using DreadsMashedPatch.PropertyHandlers.General;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Noggog;
using Xunit;
using CloudModelHandler = DreadsMashedPatch.PropertyHandlers.Worldspace.CloudModelHandler;
using ScopeModelHandler = DreadsMashedPatch.PropertyHandlers.Weapon.ScopeModelHandler;
using StaticModelHandler = DreadsMashedPatch.PropertyHandlers.Static.ModelHandler;

namespace DreadsMashedPatch.Tests;

public sealed class ModelHandlerRegressionTests
{
    [Fact]
    public void SharedModelEqualityIncludesDataAndAlternateTextureIndex()
    {
        var handler = new ModelHandler();

        Assert.False(handler.AreValuesEqual(Model([1], 3), Model([2], 3)));
        Assert.False(handler.AreValuesEqual(Model([1], 3), Model([1], 4)));
        Assert.True(handler.AreValuesEqual(Model([1], 3), Model([1], 3)));
    }

    [Fact]
    public void StaticModelCopiesAndComparesAllSerializedFields()
    {
        var handler = new StaticModelHandler();
        var target = new Mutagen.Bethesda.Skyrim.Static(
            new FormKey(TestModKey, 0x200),
            SkyrimRelease.SkyrimSE);
        var source = Model([1, 2], 7);

        handler.SetValue(target, source);

        Assert.Equal(new byte[] { 1, 2 }, target.Model!.Data!.Value.ToArray());
        Assert.Equal(7, target.Model.AlternateTextures![0].Index);
        Assert.False(handler.AreValuesEqual(source, Model([9, 9], 7)));
    }

    [Fact]
    public void ScopeModelEqualityIncludesDataAndAlternateTextureIndex()
    {
        var handler = new ScopeModelHandler();

        Assert.False(handler.AreValuesEqual(Model([1], 3), Model([2], 3)));
        Assert.False(handler.AreValuesEqual(Model([1], 3), Model([1], 4)));
    }

    [Fact]
    public void CloudModelCopiesAndComparesAllSerializedFields()
    {
        var handler = new CloudModelHandler();
        var target = new Worldspace(
            new FormKey(TestModKey, 0x201),
            SkyrimRelease.SkyrimSE);
        var source = Model([4, 5], 11);

        handler.SetValue(target, source);

        Assert.Equal(new byte[] { 4, 5 }, target.CloudModel!.Data!.Value.ToArray());
        Assert.Equal(11, target.CloudModel.AlternateTextures![0].Index);
        Assert.False(handler.AreValuesEqual(source, Model([8, 5], 11)));
        Assert.False(handler.AreValuesEqual(source, Model([4, 5], 12)));
    }

    private static Model Model(byte[] data, int alternateTextureIndex)
        => new()
        {
            File = new AssetLink<SkyrimModelAssetType>("Meshes\\Test\\Model.nif"),
            Data = new MemorySlice<byte>(data),
            AlternateTextures = new ExtendedList<AlternateTexture>
            {
                new()
                {
                    Name = "Body",
                    NewTexture = new FormLink<ITextureSetGetter>(new FormKey(TestModKey, 0x202)),
                    Index = alternateTextureIndex
                }
            }
        };

    private static readonly ModKey TestModKey = new("ModelHandlerTests.esp", ModType.Plugin);
}

using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using System.Text;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class LightRecordHandlerTests
{
    private static readonly ModKey SourceModKey = ModKey.FromNameAndExtension("LightSource.esp");
    private static readonly ModKey PatchModKey = ModKey.FromNameAndExtension("LightPatch.esp");

    [Fact]
    public void ModelAndBoundsUseSharedCoordinator()
    {
        var handlers = new LightRecordHandler().PropertyHandlers;
        Assert.IsType<ModelBoundsHandler>(handlers["ModelAndBounds"]);
        Assert.DoesNotContain("Model", handlers.Keys);
        Assert.DoesNotContain("ObjectBounds", handlers.Keys);
    }

    [Fact]
    public void OverlayModelWritesDataRelativePathWithoutDuplicatingMeshesRoot()
    {
        using var sourceStream = CreateSourcePlugin();
        using var sourceOverlay = SkyrimMod.CreateFromBinaryOverlay(
            sourceStream,
            SkyrimRelease.SkyrimSE,
            SourceModKey);
        var sourceModel = Assert.IsAssignableFrom<IModelGetter>(Assert.Single(sourceOverlay.Lights).Model);
        Assert.Equal("Meshes\\NAT\\torch.nif", sourceModel.File.ToString(), ignoreCase: true);

        var patch = new SkyrimMod(PatchModKey, SkyrimRelease.SkyrimSE);
        var target = new Light(new FormKey(PatchModKey, 0x801), SkyrimRelease.SkyrimSE);
        patch.Lights.Add(target);
        new ModelHandler().SetValue(target, sourceModel);

        Assert.Equal("Meshes\\NAT\\torch.nif", target.Model!.File.ToString(), ignoreCase: true);

        using var patchStream = new MemoryStream();
        patch.WriteToBinary(patchStream);
        var serializedPlugin = Encoding.Latin1.GetString(patchStream.ToArray());

        Assert.Contains("NAT\\torch.nif", serializedPlugin, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Meshes\\NAT\\torch.nif", serializedPlugin, StringComparison.OrdinalIgnoreCase);
    }

    private static MemoryStream CreateSourcePlugin()
    {
        var source = new SkyrimMod(SourceModKey, SkyrimRelease.SkyrimSE);
        var light = new Light(new FormKey(SourceModKey, 0x800), SkyrimRelease.SkyrimSE)
        {
            Model = new Model
            {
                File = new AssetLink<SkyrimModelAssetType>("NAT\\torch.nif")
            }
        };
        source.Lights.Add(light);

        var stream = new MemoryStream();
        source.WriteToBinary(stream);
        stream.Position = 0;
        return stream;
    }
}

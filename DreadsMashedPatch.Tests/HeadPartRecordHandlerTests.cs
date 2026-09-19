using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.HeadPart;
using ForwardChanges.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class HeadPartRecordHandlerTests
{
    private static readonly ModKey SourceModKey = ModKey.FromNameAndExtension("HeadPartSource.esp");
    private static readonly ModKey PatchModKey = ModKey.FromNameAndExtension("HeadPartPatch.esp");

    [Fact]
    public void PartsUsesSpecializedProgressiveOrderingHandler()
    {
        Assert.IsType<PartsHandler>(new HeadPartRecordHandler().PropertyHandlers["Parts"]);
        var inspectable = new InspectablePartsHandler();

        Assert.Equal(ListSemantics.AlignedOrdered, inspectable.ExposedSemantics);
    }

    [Fact]
    public void PartsComparisonTreatsTypeAndFilenameAsAnOrderedAtomicUnit()
    {
        var handler = new PartsHandler();
        var tri = CreatePart(Part.PartTypeEnum.Tri, "Actors/Character/Eyes.tri");
        var chargen = CreatePart(Part.PartTypeEnum.ChargenMorph, "Actors/Character/EyesChargen.tri");
        var differentTri = CreatePart(Part.PartTypeEnum.Tri, "Actors/Character/OtherEyes.tri");

        Assert.True(handler.AreValuesEqual([tri, chargen], [tri.DeepCopy(), chargen.DeepCopy()]));
        Assert.False(handler.AreValuesEqual([tri, chargen], [chargen, tri]));
        Assert.False(handler.AreValuesEqual([tri], [differentTri]));
    }

    [Fact]
    public void CopiesFilenamesFromBinaryOverlayAndWritesThemBackOut()
    {
        using var sourceStream = CreateSourcePlugin();
        using var sourceOverlay = SkyrimMod.CreateFromBinaryOverlay(
            sourceStream,
            SkyrimRelease.SkyrimSE,
            SourceModKey);
        var overlayHeadPart = Assert.Single(sourceOverlay.HeadParts);
        var overlayParts = new PartsHandler().GetValue(overlayHeadPart);

        var patch = new SkyrimMod(PatchModKey, SkyrimRelease.SkyrimSE);
        var target = new Mutagen.Bethesda.Skyrim.HeadPart(
            new FormKey(PatchModKey, 0x801),
            SkyrimRelease.SkyrimSE)
        {
            Type = Mutagen.Bethesda.Skyrim.HeadPart.TypeEnum.Eyes
        };
        patch.HeadParts.Add(target);
        new PartsHandler().SetValue(target, overlayParts);

        Assert.Equal(
            ["Meshes\\Actors\\Character\\EyesFemale.tri", "Meshes\\Actors\\Character\\EyesChildChargen.tri"],
            target.Parts.Select(part => part.FileName?.DataRelativePath.ToString()));

        using var patchStream = new MemoryStream();
        patch.WriteToBinary(patchStream);
        patchStream.Position = 0;
        using var patchOverlay = SkyrimMod.CreateFromBinaryOverlay(
            patchStream,
            SkyrimRelease.SkyrimSE,
            PatchModKey);
        var writtenParts = Assert.Single(patchOverlay.HeadParts).Parts;

        Assert.Equal(
            ["Meshes\\Actors\\Character\\EyesFemale.tri", "Meshes\\Actors\\Character\\EyesChildChargen.tri"],
            writtenParts.Select(part => part.FileName?.DataRelativePath.ToString()));
    }

    private static MemoryStream CreateSourcePlugin()
    {
        var source = new SkyrimMod(SourceModKey, SkyrimRelease.SkyrimSE);
        var headPart = new Mutagen.Bethesda.Skyrim.HeadPart(
            new FormKey(SourceModKey, 0x800),
            SkyrimRelease.SkyrimSE)
        {
            Type = Mutagen.Bethesda.Skyrim.HeadPart.TypeEnum.Eyes
        };
        headPart.Parts.Add(CreatePart(Part.PartTypeEnum.Tri, "Actors/Character/EyesFemale.tri"));
        headPart.Parts.Add(CreatePart(Part.PartTypeEnum.ChargenMorph, "Actors/Character/EyesChildChargen.tri"));
        source.HeadParts.Add(headPart);

        var stream = new MemoryStream();
        source.WriteToBinary(stream);
        stream.Position = 0;
        return stream;
    }

    private static Part CreatePart(Part.PartTypeEnum type, string path) =>
        new()
        {
            PartType = type,
            FileName = new AssetLink<SkyrimDeformedModelAssetType>(path)
        };

    private sealed class InspectablePartsHandler : PartsHandler
    {
        public ListSemantics ExposedSemantics => Semantics;
    }
}

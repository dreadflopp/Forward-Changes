using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class FormLinkNullSemanticsTests
{
    private static readonly ModKey SourceModKey = new("OwnerSource.esp", ModType.Plugin);
    private static readonly ModKey PatchModKey = new("OwnerPatch.esp", ModType.Plugin);
    private static readonly FormKey OwnerKey = new(SourceModKey, 0xB02);

    [Fact]
    public void ClearingPlacedObjectOwnerSerializesAsAbsentInsteadOfNullReference()
    {
        var patch = new SkyrimMod(PatchModKey, SkyrimRelease.SkyrimSE);
        var placed = new PlacedObject(new FormKey(PatchModKey, 0xB01), SkyrimRelease.SkyrimSE);
        placed.Owner.SetTo(OwnerKey);
        AddToCell(patch, placed);
        var handler = Assert.IsType<SimpleReflectionFormLinkPropertyHandler<
            IOwnerGetter,
            IPlacedObject,
            IPlacedObjectGetter>>(new PlacedObjectRecordHandler().PropertyHandlers["Owner"]);

        handler.SetValue(placed, null);

        Assert.Null(placed.Owner.FormKeyNullable);
        using var stream = new MemoryStream();
        patch.WriteToBinary(stream);
        stream.Position = 0;
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(stream, SkyrimRelease.SkyrimSE, PatchModKey);
        var written = Assert.Single(overlay.EnumerateMajorRecords<IPlacedObjectGetter>());
        Assert.Null(written.Owner.FormKeyNullable);
    }

    [Fact]
    public void SettingPlacedObjectOwnerStillWritesTheRequestedReference()
    {
        var placed = new PlacedObject(new FormKey(PatchModKey, 0xB03), SkyrimRelease.SkyrimSE);
        var handler = new SimpleReflectionFormLinkPropertyHandler<
            IOwnerGetter,
            IPlacedObject,
            IPlacedObjectGetter>("Owner");

        handler.SetValue(placed, new FormLinkNullable<IOwnerGetter>(OwnerKey));

        Assert.Equal(OwnerKey, placed.Owner.FormKeyNullable);
    }

    private static void AddToCell(SkyrimMod mod, PlacedObject placed)
    {
        var cell = new Cell(new FormKey(PatchModKey, 0xB00), SkyrimRelease.SkyrimSE);
        cell.Temporary.Add(placed);
        var subBlock = new CellSubBlock();
        subBlock.Cells.Add(cell);
        var block = new CellBlock();
        block.SubBlocks.Add(subBlock);
        mod.Cells.Records.Add(block);
    }
}

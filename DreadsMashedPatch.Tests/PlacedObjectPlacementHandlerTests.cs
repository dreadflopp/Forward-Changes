using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.PlacedObject;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Xunit;

namespace DreadsMashedPatch.Tests;

[CollectionDefinition("PlacedObjectPlacementHandlerTests", DisableParallelization = true)]
public sealed class PlacedObjectPlacementHandlerTestCollection;

[Collection("PlacedObjectPlacementHandlerTests")]
public sealed class PlacedObjectPlacementHandlerTests
{
    private static readonly ModKey SourceModKey = ModKey.FromNameAndExtension("PlacedSource.esp");
    private static readonly ModKey PatchModKey = ModKey.FromNameAndExtension("PlacedPatch.esp");

    [Fact]
    public void RecordHandlerUsesReadablePlacementFormatter()
    {
        var handler = Assert.IsType<PlacementHandler>(
            new PlacedObjectRecordHandler().PropertyHandlers["Placement"]);
        var placement = new Placement
        {
            Position = new P3Float(1.25f, -2.5f, -0f),
            Rotation = new P3Float(0.1f, 0.2f, 0.3f)
        };

        var formatted = handler.FormatValue(placement);

        Assert.Equal(
            "Position=(1.250000, -2.500000, 0.000000), RotationDegrees=(5.7296, 11.4592, 17.1887)",
            formatted);
    }

    [Fact]
    public void RecordHandlerUsesOneCohesivePlacementPath()
    {
        var handlers = new PlacedObjectRecordHandler().PropertyHandlers;

        Assert.Contains("Placement", handlers.Keys);
        Assert.DoesNotContain("Placement.Position", handlers.Keys);
        Assert.DoesNotContain("Placement.Rotation", handlers.Keys);
    }

    [Fact]
    public void PlacementEqualityIgnoresSubPrecisionRotationDifferences()
    {
        var handler = new PlacementHandler();
        var original = new Placement
        {
            Position = new P3Float(936.2212f, 76.75349f, 240f),
            Rotation = new P3Float(0f, 0f, 3.0500011f)
        };
        var roundedDifference = new Placement
        {
            Position = original.Position,
            Rotation = new P3Float(0f, 0f, 3.0500014f)
        };
        var visibleDifference = new Placement
        {
            Position = original.Position,
            Rotation = new P3Float(0f, 0f, 3.0500111f)
        };

        Assert.True(handler.AreValuesEqual(original, roundedDifference));
        Assert.False(handler.AreValuesEqual(original, visibleDifference));
    }

    [Fact]
    public void PlacementEqualityNormalizesEquivalentAnglesModuloFullTurn()
    {
        var handler = new PlacementHandler();
        var original = new Placement
        {
            Position = new P3Float(2002.1953125f, -568.9878540f, -5.7508850f),
            Rotation = new P3Float(-0.9324064f, -0.7878084f, 5.3921800f)
        };
        var winning = new Placement
        {
            Position = original.Position,
            Rotation = new P3Float(5.3507795f, 5.4953766f, 5.3921804f)
        };

        Assert.True(handler.AreValuesEqual(original, winning));
        Assert.Equal(
            "Position=(2002.195312, -568.987854, -5.750885), RotationDegrees=(306.5770, 314.8619, 308.9492)",
            handler.FormatValue(original));
    }

    [Fact]
    public void PlacementEqualityUsesXEditPositionPrecision()
    {
        var handler = new PlacementHandler();
        var original = new Placement
        {
            Position = new P3Float(1.0000001f, 2f, 3f),
            Rotation = new P3Float()
        };
        var sameDisplayedPosition = new Placement
        {
            Position = new P3Float(1.0000004f, 2f, 3f),
            Rotation = original.Rotation
        };
        var differentDisplayedPosition = new Placement
        {
            Position = new P3Float(1.0000013f, 2f, 3f),
            Rotation = original.Rotation
        };

        Assert.True(handler.AreValuesEqual(original, sameDisplayedPosition));
        Assert.False(handler.AreValuesEqual(original, differentDisplayedPosition));
    }

    [Fact]
    public void FormatterPreservesNullOutput()
    {
        Assert.Equal("null", new PlacementHandler().FormatValue(null));
    }

    [Fact]
    public void EnableParentOverlayCopiesOpaqueBytesWithoutWarning()
    {
        using var stream = CreatePlacedObjectPlugin();
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(
            stream,
            SkyrimRelease.SkyrimSE,
            SourceModKey);
        var source = Assert.Single(overlay.EnumerateMajorRecords<IPlacedObjectGetter>());
        var target = new PlacedObject(
            new FormKey(PatchModKey, 0x900),
            SkyrimRelease.SkyrimSE);
        var handler = Assert.IsType<ComplexReflectionPropertyHandler<
            IEnableParentGetter,
            IPlacedObject,
            IPlacedObjectGetter>>(
                new PlacedObjectRecordHandler().PropertyHandlers["EnableParent"]);
        LogCollector.Clear();

        handler.SetValue(target, handler.GetValue(source));

        Assert.NotNull(target.EnableParent);
        Assert.Equal(new byte[] { 7, 11, 13 }, target.EnableParent!.Unknown.ToArray());
        Assert.DoesNotContain(
            LogCollector.GetAll(),
            line => line.Contains("Skipped property 'Unknown'", StringComparison.Ordinal));
        LogCollector.Clear();
    }

    private static MemoryStream CreatePlacedObjectPlugin()
    {
        var source = new SkyrimMod(SourceModKey, SkyrimRelease.SkyrimSE);
        var placed = new PlacedObject(
            new FormKey(SourceModKey, 0x802),
            SkyrimRelease.SkyrimSE)
        {
            EnableParent = new EnableParent
            {
                Reference = new FormLink<IPlacedGetter>(new FormKey(SourceModKey, 0x803)),
                Flags = EnableParent.Flag.PopIn,
                Unknown = new MemorySlice<byte>([7, 11, 13])
            }
        };
        var cell = new Cell(
            new FormKey(SourceModKey, 0x801),
            SkyrimRelease.SkyrimSE);
        cell.Temporary.Add(placed);
        var subBlock = new CellSubBlock();
        subBlock.Cells.Add(cell);
        var block = new CellBlock();
        block.SubBlocks.Add(subBlock);
        source.Cells.Records.Add(block);

        var stream = new MemoryStream();
        source.WriteToBinary(stream);
        stream.Position = 0;
        return stream;
    }
}

using System.Drawing;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace DreadsMashedPatch.Tests;

[Collection("LogCollector")]
public sealed class OverlayAggregateCopyTests
{
    private static readonly ModKey SourceModKey = ModKey.FromNameAndExtension("AggregateSource.esp");
    private static readonly ModKey PatchModKey = ModKey.FromNameAndExtension("AggregatePatch.esp");

    [Fact]
    public void PlacedNpcActivateParentsCopiesOverlayRowsWithoutWarnings()
    {
        using var stream = CreatePlacedNpcPlugin();
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(stream, SkyrimRelease.SkyrimSE, SourceModKey);
        var source = Assert.Single(overlay.EnumerateMajorRecords<IPlacedNpcGetter>());
        var target = new PlacedNpc(new FormKey(PatchModKey, 0xA03), SkyrimRelease.SkyrimSE);
        var handler = Assert.IsType<GeneratedCopyReflectionPropertyHandler<
            IActivateParentsGetter,
            ActivateParents,
            IPlacedNpc,
            IPlacedNpcGetter>>(new PlacedNpcRecordHandler().PropertyHandlers["ActivateParents"]);
        LogCollector.Clear();

        handler.SetValue(target, handler.GetValue(source));

        var copied = Assert.Single(target.ActivateParents!.Parents);
        Assert.Equal(new FormKey(SourceModKey, 0xA02), copied.Reference.FormKey);
        Assert.Equal(1.25f, copied.Delay);
        Assert.Empty(LogCollector.GetAll());
        LogCollector.Clear();
    }

    [Fact]
    public void WeatherColorCopiesOverlayWithoutReflectingOverTimeOfDayIndexer()
    {
        using var stream = CreateWeatherPlugin();
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(stream, SkyrimRelease.SkyrimSE, SourceModKey);
        var source = Assert.Single(overlay.Weathers);
        var target = new Weather(new FormKey(PatchModKey, 0xA11), SkyrimRelease.SkyrimSE);
        var handler = Assert.IsType<GeneratedCopyReflectionPropertyHandler<
            IWeatherColorGetter,
            WeatherColor,
            IWeather,
            IWeatherGetter>>(new WeatherRecordHandler().PropertyHandlers["SkyUpperColor"]);
        LogCollector.Clear();

        handler.SetValue(target, handler.GetValue(source));

        Assert.Equal(Color.FromArgb(255, 10, 20, 30), target.SkyUpperColor.Sunrise);
        Assert.Equal(Color.FromArgb(255, 40, 50, 60), target.SkyUpperColor.Day);
        Assert.Empty(LogCollector.GetAll());
        LogCollector.Clear();
    }

    private static MemoryStream CreatePlacedNpcPlugin()
    {
        var source = new SkyrimMod(SourceModKey, SkyrimRelease.SkyrimSE);
        var placed = new PlacedNpc(new FormKey(SourceModKey, 0xA01), SkyrimRelease.SkyrimSE)
        {
            ActivateParents = new ActivateParents
            {
                Flags = ActivateParents.Flag.ParentActivateOnly
            }
        };
        placed.ActivateParents.Parents.Add(new ActivateParent
        {
            Reference = new FormLink<IPlacedGetter>(new FormKey(SourceModKey, 0xA02)),
            Delay = 1.25f
        });
        var cell = new Cell(new FormKey(SourceModKey, 0xA00), SkyrimRelease.SkyrimSE);
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

    private static MemoryStream CreateWeatherPlugin()
    {
        var source = new SkyrimMod(SourceModKey, SkyrimRelease.SkyrimSE);
        var weather = new Weather(new FormKey(SourceModKey, 0xA10), SkyrimRelease.SkyrimSE);
        weather.SkyUpperColor.Sunrise = Color.FromArgb(255, 10, 20, 30);
        weather.SkyUpperColor.Day = Color.FromArgb(255, 40, 50, 60);
        source.Weathers.Add(weather);

        var stream = new MemoryStream();
        source.WriteToBinary(stream);
        stream.Position = 0;
        return stream;
    }
}

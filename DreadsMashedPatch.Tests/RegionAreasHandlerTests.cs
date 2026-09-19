using ForwardChanges.PropertyHandlers.Region;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class RegionAreasHandlerTests
{
    private static readonly ModKey TestModKey = ModKey.FromNameAndExtension("RegionAreasTests.esp");

    [Fact]
    public void ReversedPolygonDirectionMatchesXEditNormalization()
    {
        var handler = new RegionAreasHandler();
        var forward = Area(
            new P2Float(101940.016f, 111101.54f),
            new P2Float(102015.41f, 93910.77f),
            new P2Float(115059.27f, 93910.77f),
            new P2Float(115059.27f, 111101.54f));
        var reversed = Area(Enumerable.Reverse(
            (IEnumerable<P2Float>)forward.RegionPointListData!).ToArray());

        Assert.True(handler.AreValuesEqual([forward], [reversed]));
    }

    [Fact]
    public void AChangedPointIsNotEqual()
    {
        var handler = new RegionAreasHandler();
        var original = Area(new P2Float(1, 2), new P2Float(3, 4));
        var changed = Area(new P2Float(1, 2), new P2Float(3, 5));

        Assert.False(handler.AreValuesEqual([original], [changed]));
    }

    [Fact]
    public void SetterDeepCopiesAllPointData()
    {
        var handler = new RegionAreasHandler();
        var source = Area(new P2Float(1, 2), new P2Float(3, 4));
        var target = CreateRegion(0xB00);

        handler.SetValue(target, [source]);

        var copied = Assert.Single(target.RegionAreas);
        Assert.Equal((uint)0, copied.EdgeFallOff);
        Assert.NotNull(copied.RegionPointListData);
        Assert.Equal(source.RegionPointListData, copied.RegionPointListData);
        Assert.NotSame(source.RegionPointListData, copied.RegionPointListData);
    }

    [Fact]
    public void SetterPreservesNullPointList()
    {
        var handler = new RegionAreasHandler();
        var target = CreateRegion(0xB01);

        handler.SetValue(target, [new RegionArea { EdgeFallOff = 7, RegionPointListData = null }]);

        var copied = Assert.Single(target.RegionAreas);
        Assert.Equal((uint)7, copied.EdgeFallOff);
        Assert.Null(copied.RegionPointListData);
    }

    [Fact]
    public void CopiedPointDataSurvivesSerialization()
    {
        var handler = new RegionAreasHandler();
        var target = CreateRegion(0xB02);
        handler.SetValue(target,
        [
            Area(
                new P2Float(101940.016f, 111101.54f),
                new P2Float(102015.41f, 93910.77f),
                new P2Float(115059.27f, 93910.77f),
                new P2Float(115059.27f, 111101.54f))
        ]);
        var mod = new SkyrimMod(TestModKey, SkyrimRelease.SkyrimSE);
        mod.Regions.Add(target);

        using var stream = new MemoryStream();
        mod.WriteToBinary(stream);
        stream.Position = 0;
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(stream, SkyrimRelease.SkyrimSE, TestModKey);
        var roundTripped = Assert.Single(overlay.Regions);
        var area = Assert.Single(roundTripped.RegionAreas);

        Assert.NotNull(area.RegionPointListData);
        Assert.Equal(4, area.RegionPointListData.Count);
        Assert.Equal(101940.016f, area.RegionPointListData[0].X);
        Assert.Equal(111101.54f, area.RegionPointListData[0].Y);
    }

    private static Region CreateRegion(uint id) =>
        new(new FormKey(TestModKey, id), SkyrimRelease.SkyrimSE);

    private static RegionArea Area(params P2Float[] points) =>
        new()
        {
            EdgeFallOff = 0,
            RegionPointListData = new ExtendedList<P2Float>(points)
        };
}

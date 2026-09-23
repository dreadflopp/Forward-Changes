using DreadsMashedPatch.PropertyHandlers.Npc;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;
using PlacedNpcPlacementHandler = DreadsMashedPatch.PropertyHandlers.PlacedNpc.PlacementHandler;

namespace DreadsMashedPatch.Tests;

public sealed class NpcPlacementHandlerTests
{
    [Fact]
    public void FactionEqualityIncludesRank()
    {
        var handler = new FactionHandler();
        var faction = new FormKey(TestModKey, 0x100);

        Assert.True(handler.AreValuesEqual(
            [new RankPlacement { Faction = new FormLink<IFactionGetter>(faction), Rank = 1 }],
            [new RankPlacement { Faction = new FormLink<IFactionGetter>(faction), Rank = 1 }]));
        Assert.False(handler.AreValuesEqual(
            [new RankPlacement { Faction = new FormLink<IFactionGetter>(faction), Rank = 1 }],
            [new RankPlacement { Faction = new FormLink<IFactionGetter>(faction), Rank = 2 }]));
    }

    [Fact]
    public void PerkEqualityIncludesRank()
    {
        var handler = new PerksHandler();
        var perk = new FormKey(TestModKey, 0x101);

        Assert.True(handler.AreValuesEqual(
            [new PerkPlacement { Perk = new FormLink<IPerkGetter>(perk), Rank = 1 }],
            [new PerkPlacement { Perk = new FormLink<IPerkGetter>(perk), Rank = 1 }]));
        Assert.False(handler.AreValuesEqual(
            [new PerkPlacement { Perk = new FormLink<IPerkGetter>(perk), Rank = 1 }],
            [new PerkPlacement { Perk = new FormLink<IPerkGetter>(perk), Rank = 2 }]));
    }

    [Fact]
    public void RecordHandlerUsesEpsilonAwarePlacementHandler()
    {
        Assert.IsType<PlacedNpcPlacementHandler>(
            new PlacedNpcRecordHandler().PropertyHandlers["Placement"]);
    }

    [Fact]
    public void PlacementEqualityIgnoresLoggedSubPrecisionRotationDifference()
    {
        var handler = new PlacedNpcPlacementHandler();
        var winning = new Placement
        {
            Position = new Noggog.P3Float(936.2212f, 76.75349f, 240f),
            Rotation = new Noggog.P3Float(0f, 0f, 3.0500011f)
        };
        var ussep = new Placement
        {
            Position = winning.Position,
            Rotation = new Noggog.P3Float(0f, 0f, 3.0500014f)
        };

        Assert.True(handler.AreValuesEqual(winning, ussep));
        Assert.Equal(
            "Position=(936.221191, 76.753487, 240.000000), RotationDegrees=(0.0000, 0.0000, 174.7522)",
            handler.FormatValue(winning));
    }

    private static readonly ModKey TestModKey = new("NpcPlacementTests.esp", ModType.Plugin);
}

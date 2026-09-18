using ForwardChanges.PropertyHandlers.Npc;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace ForwardChanges.Tests;

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

    private static readonly ModKey TestModKey = new("NpcPlacementTests.esp", ModType.Plugin);
}

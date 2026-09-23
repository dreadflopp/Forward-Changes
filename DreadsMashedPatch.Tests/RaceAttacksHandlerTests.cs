using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace DreadsMashedPatch.Tests;

[Collection("LogCollector")]
public sealed class RaceAttacksHandlerTests
{
    private static readonly ModKey SourceModKey = ModKey.FromNameAndExtension("RaceAttackSource.esp");
    private static readonly ModKey PatchModKey = ModKey.FromNameAndExtension("RaceAttackPatch.esp");

    [Fact]
    public void RaceUsesSharedSpecializedAttackHandler()
    {
        var handler = Assert.IsType<AttacksHandler>(new RaceRecordHandler().PropertyHandlers["Attacks"]);

        Assert.Equal(ListSemantics.SortedKeyed, handler.Semantics);
    }

    [Fact]
    public void CopiesAttackDataFromBinaryOverlayWithoutWarnings()
    {
        using var sourceStream = CreateSourcePlugin();
        using var sourceOverlay = SkyrimMod.CreateFromBinaryOverlay(
            sourceStream,
            SkyrimRelease.SkyrimSE,
            SourceModKey);
        var overlayRace = Assert.Single(sourceOverlay.Races);
        var handler = new AttacksHandler();
        var overlayAttacks = handler.GetValue(overlayRace);
        var target = new Race(new FormKey(PatchModKey, 0x901), SkyrimRelease.SkyrimSE);

        LogCollector.Clear();
        handler.SetValue(target, overlayAttacks);

        var copied = Assert.Single(target.Attacks);
        Assert.Equal("attackPowerStart", copied.AttackEvent);
        Assert.NotNull(copied.AttackData);
        Assert.Equal(1.25f, copied.AttackData.DamageMult);
        Assert.Equal(0.75f, copied.AttackData.Chance);
        Assert.Equal(AttackData.Flag.PowerAttack, copied.AttackData.Flags);
        Assert.Empty(LogCollector.GetAll());
    }

    private static MemoryStream CreateSourcePlugin()
    {
        var source = new SkyrimMod(SourceModKey, SkyrimRelease.SkyrimSE);
        var race = new Race(new FormKey(SourceModKey, 0x900), SkyrimRelease.SkyrimSE);
        race.Attacks.Add(new Attack
        {
            AttackEvent = "attackPowerStart",
            AttackData = new AttackData
            {
                DamageMult = 1.25f,
                Chance = 0.75f,
                Flags = AttackData.Flag.PowerAttack,
                AttackAngle = 0.5f,
                StrikeAngle = 0.25f,
                Stagger = 0.4f,
                Knockdown = 0.1f,
                RecoveryTime = 0.8f,
                StaminaMult = 1.5f
            }
        });
        source.Races.Add(race);

        var stream = new MemoryStream();
        source.WriteToBinary(stream);
        stream.Position = 0;
        return stream;
    }
}

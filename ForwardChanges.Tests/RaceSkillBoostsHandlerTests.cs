using ForwardChanges.PropertyHandlers.Race;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class RaceSkillBoostsHandlerTests
{
    private static readonly ModKey TestModKey = ModKey.FromNameAndExtension("RaceSkillBoostTests.esp");

    [Fact]
    public void ReorderedPhysicalSlotsHaveEqualSemanticValues()
    {
        var handler = new RaceSkillBoostsHandler();
        var original = CreateRace(0xA00,
            Boost(ActorValue.TwoHanded, 10),
            Boost(ActorValue.OneHanded, 5),
            Boost(ActorValue.Block, 5),
            Boost(ActorValue.Smithing, 5),
            Boost(ActorValue.Speech, 5),
            Boost(ActorValue.LightArmor, 5));
        var reordered = CreateRace(0xA01,
            new SkillBoost(),
            Boost(ActorValue.OneHanded, 5),
            Boost(ActorValue.TwoHanded, 10),
            Boost(ActorValue.Block, 5),
            Boost(ActorValue.Smithing, 5),
            Boost(ActorValue.LightArmor, 5),
            Boost(ActorValue.Speech, 5));

        Assert.True(handler.AreValuesEqual(handler.GetValue(original), handler.GetValue(reordered)));
    }

    [Fact]
    public void SpeechMovedFromSlotFourToSlotSixIsNotAnAddition()
    {
        var handler = new RaceSkillBoostsHandler();
        var original = CreateRace(0xA02,
            Boost(ActorValue.TwoHanded, 10),
            Boost(ActorValue.OneHanded, 5),
            Boost(ActorValue.Block, 5),
            Boost(ActorValue.Smithing, 5),
            Boost(ActorValue.Speech, 5),
            Boost(ActorValue.LightArmor, 5));
        var ussep = CreateRace(0xA03,
            new SkillBoost(),
            Boost(ActorValue.OneHanded, 5),
            Boost(ActorValue.TwoHanded, 10),
            Boost(ActorValue.Block, 5),
            Boost(ActorValue.Smithing, 5),
            Boost(ActorValue.LightArmor, 5),
            Boost(ActorValue.Speech, 5));

        var originalSpeech = Assert.Single(handler.GetValue(original)!, boost => boost.Skill == ActorValue.Speech);
        var ussepSpeech = Assert.Single(handler.GetValue(ussep)!, boost => boost.Skill == ActorValue.Speech);

        Assert.Equal((sbyte)5, originalSpeech.Boost);
        Assert.Equal((sbyte)5, ussepSpeech.Boost);
        Assert.True(handler.AreValuesEqual(handler.GetValue(original), handler.GetValue(ussep)));
    }

    [Fact]
    public void SameSkillWithDifferentBoostIsAValueChange()
    {
        var handler = new RaceSkillBoostsHandler();
        var five = CreateRace(0xA04, Boost(ActorValue.Speech, 5));
        var ten = CreateRace(0xA05, Boost(ActorValue.Speech, 10));

        Assert.False(handler.AreValuesEqual(handler.GetValue(five), handler.GetValue(ten)));
    }

    [Fact]
    public void SetterSortsActiveBoostsAndPadsAllSevenSlots()
    {
        var handler = new RaceSkillBoostsHandler();
        var race = CreateRace(0xA06);

        handler.SetValue(race,
        [
            Boost(ActorValue.Speech, 5),
            Boost(ActorValue.Block, 10)
        ]);

        Assert.Equal(ActorValue.Block, race.SkillBoost0.Skill);
        Assert.Equal((sbyte)10, race.SkillBoost0.Boost);
        Assert.Equal(ActorValue.Speech, race.SkillBoost1.Skill);
        Assert.Equal((sbyte)5, race.SkillBoost1.Boost);
        Assert.All(
            new[] { race.SkillBoost2, race.SkillBoost3, race.SkillBoost4, race.SkillBoost5, race.SkillBoost6 },
            boost =>
            {
                Assert.Equal(ActorValue.None, boost.Skill);
                Assert.Equal((sbyte)0, boost.Boost);
            });
    }

    [Fact]
    public void EmptySemanticValueClearsAllPhysicalSlots()
    {
        var handler = new RaceSkillBoostsHandler();
        var race = CreateRace(0xA07, Boost(ActorValue.Speech, 5));

        handler.SetValue(race, []);

        Assert.Empty(handler.GetValue(race)!);
        Assert.All(
            new[] { race.SkillBoost0, race.SkillBoost1, race.SkillBoost2, race.SkillBoost3,
                race.SkillBoost4, race.SkillBoost5, race.SkillBoost6 },
            boost => Assert.Equal(ActorValue.None, boost.Skill));
    }

    [Fact]
    public void RaceRegistersOnlyTheGroupedSkillBoostHandler()
    {
        var handlers = new ForwardChanges.RecordHandlers.RaceRecordHandler().PropertyHandlers;

        Assert.IsType<RaceSkillBoostsHandler>(handlers["SkillBoosts"]);
        Assert.DoesNotContain(Enumerable.Range(0, 7), index => handlers.ContainsKey($"SkillBoost{index}"));
    }

    private static Race CreateRace(uint id, params SkillBoost[] boosts)
    {
        var race = new Race(new FormKey(TestModKey, id), SkyrimRelease.SkyrimSE);
        var physical = boosts.Concat(Enumerable.Repeat(new SkillBoost(), 7)).Take(7).ToArray();
        race.SkillBoost0 = physical[0];
        race.SkillBoost1 = physical[1];
        race.SkillBoost2 = physical[2];
        race.SkillBoost3 = physical[3];
        race.SkillBoost4 = physical[4];
        race.SkillBoost5 = physical[5];
        race.SkillBoost6 = physical[6];
        return race;
    }

    private static SkillBoost Boost(ActorValue skill, sbyte amount) =>
        new()
        {
            Skill = skill,
            Boost = amount
        };
}

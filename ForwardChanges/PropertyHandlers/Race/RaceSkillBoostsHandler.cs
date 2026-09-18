using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyHandlers.Race;

/// <summary>
/// Treats the seven physical RACE skill-boost slots as the sorted keyed array
/// defined by xEdit. Skill is identity; Boost is the value. ActorValue.None
/// entries are fixed-width binary padding rather than semantic list entries.
/// </summary>
public sealed class RaceSkillBoostsHandler : AbstractListPropertyHandler<ISkillBoostGetter>
{
    private const int SlotCount = 7;

    public override string PropertyName => "SkillBoosts";

    public override ListSemantics Semantics => ListSemantics.SortedKeyed;

    public override List<ISkillBoostGetter>? GetValue(IMajorRecordGetter record)
    {
        if (record is not IRaceGetter race)
        {
            return null;
        }

        return GetPhysicalSlots(race)
            .Where(boost => boost.Skill != ActorValue.None)
            .ToList();
    }

    public override void SetValue(IMajorRecord record, List<ISkillBoostGetter>? value)
    {
        if (record is not IRace race)
        {
            return;
        }

        var activeBoosts = (value ?? [])
            .Where(boost => boost.Skill != ActorValue.None)
            .ToList();

        if (activeBoosts.Count > SlotCount)
        {
            throw new InvalidOperationException(
                $"RACE skill boosts contain {activeBoosts.Count} active entries; the format permits at most {SlotCount}.");
        }

        var duplicateSkill = activeBoosts
            .GroupBy(boost => boost.Skill)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateSkill != null)
        {
            throw new InvalidOperationException(
                $"RACE skill boosts contain duplicate skill {duplicateSkill.Key}.");
        }

        var physicalSlots = activeBoosts
            .OrderBy(boost => (int)boost.Skill)
            .Select(CopyBoost)
            .ToList();
        while (physicalSlots.Count < SlotCount)
        {
            physicalSlots.Add(new SkillBoost());
        }

        race.SkillBoost0 = physicalSlots[0];
        race.SkillBoost1 = physicalSlots[1];
        race.SkillBoost2 = physicalSlots[2];
        race.SkillBoost3 = physicalSlots[3];
        race.SkillBoost4 = physicalSlots[4];
        race.SkillBoost5 = physicalSlots[5];
        race.SkillBoost6 = physicalSlots[6];
    }

    protected override bool IsItemEqual(ISkillBoostGetter? left, ISkillBoostGetter? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left == null || right == null) return false;
        return left.Skill == right.Skill && left.Boost == right.Boost;
    }

    protected override bool IsItemIdentityEqual(ISkillBoostGetter? left, ISkillBoostGetter? right)
    {
        if (left == null || right == null) return left == null && right == null;
        return left.Skill == right.Skill;
    }

    protected override IReadOnlyList<object?> GetSortKey(ISkillBoostGetter item) => [item.Skill];

    protected override ISkillBoostGetter CopyItemForForwardContext(ISkillBoostGetter item) => CopyBoost(item);

    protected override string FormatItem(ISkillBoostGetter? item) =>
        item == null ? "null" : $"{item.Skill} {(item.Boost >= 0 ? "+" : string.Empty)}{item.Boost}";

    private static SkillBoost CopyBoost(ISkillBoostGetter boost) =>
        new()
        {
            Skill = boost.Skill,
            Boost = boost.Boost
        };

    private static IEnumerable<ISkillBoostGetter> GetPhysicalSlots(IRaceGetter race)
    {
        yield return race.SkillBoost0;
        yield return race.SkillBoost1;
        yield return race.SkillBoost2;
        yield return race.SkillBoost3;
        yield return race.SkillBoost4;
        yield return race.SkillBoost5;
        yield return race.SkillBoost6;
    }
}

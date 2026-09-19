using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.Perk;

public sealed class EffectsHandler : AbstractListPropertyHandler<IAPerkEffectGetter>
{
    public override string PropertyName => "Effects";

    public override ListSemantics Semantics => ListSemantics.SortedKeyed;

    public override List<IAPerkEffectGetter>? GetValue(IMajorRecordGetter record)
    {
        return record is IPerkGetter perk
            ? perk.Effects.Cast<IAPerkEffectGetter>().ToList()
            : null;
    }

    public override void SetValue(IMajorRecord record, List<IAPerkEffectGetter>? value)
    {
        if (record is not IPerk perk)
        {
            Console.WriteLine($"Error: Record does not implement IPerk for {PropertyName}");
            return;
        }

        perk.Effects.Clear();
        if (value == null)
        {
            return;
        }

        foreach (var effect in value)
        {
            perk.Effects.Add(effect.DeepCopy());
        }
    }

    protected override bool IsItemEqual(IAPerkEffectGetter? item1, IAPerkEffectGetter? item2)
    {
        if (ReferenceEquals(item1, item2)) return true;
        if (item1 == null || item2 == null) return false;

        return APerkEffectMixIn.Equals(item1, item2);
    }

    protected override bool IsItemIdentityEqual(IAPerkEffectGetter? left, IAPerkEffectGetter? right)
    {
        if (left == null || right == null) return left == null && right == null;
        return GetSortKey(left).SequenceEqual(GetSortKey(right));
    }

    protected override IReadOnlyList<object?> GetSortKey(IAPerkEffectGetter item)
    {
        (int Type, object? Data1, object? Data2) key = item switch
        {
            IPerkQuestEffectGetter quest => (0, quest.Quest.FormKey, quest.Stage),
            IPerkAbilityEffectGetter ability => (1, (object?)ability.Ability.FormKey, null),
            IAPerkEntryPointEffectGetter entry => (2, (object?)entry.EntryPoint, null),
            _ => (int.MaxValue, (object?)item.GetType().Name, null)
        };

        if (item is not IAPerkEntryPointEffectGetter entryPoint)
        {
            return [item.Rank, item.Priority, key.Type, key.Data1, key.Data2];
        }

        return
        [
            item.Rank,
            item.Priority,
            key.Type,
            key.Data1,
            GetFunctionParameterType(entryPoint),
            item.Flags.FragmentIndex
        ];
    }

    private static int GetFunctionParameterType(IAPerkEntryPointEffectGetter effect) => effect switch
    {
        IPerkEntryPointAbsoluteValueGetter => 0,
        IPerkEntryPointModifyValueGetter => 1,
        IPerkEntryPointModifyValuesGetter => 2,
        IPerkEntryPointAddRangeToValueGetter => 2,
        IPerkEntryPointModifyActorValueGetter => 2,
        IPerkEntryPointAddLeveledItemGetter => 3,
        IPerkEntryPointAddActivateChoiceGetter => 4,
        IPerkEntryPointSelectSpellGetter => 5,
        IPerkEntryPointSelectTextGetter => 6,
        IPerkEntryPointSetTextGetter => 7,
        _ => throw new InvalidOperationException($"Unsupported perk entry-point effect type {effect.GetType().Name}.")
    };
}

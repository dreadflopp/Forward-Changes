using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Xunit;
using IngestibleEffectsHandler = ForwardChanges.PropertyHandlers.Ingestible.EffectHandler;
using IngredientEffectsHandler = ForwardChanges.PropertyHandlers.Ingredient.EffectHandler;
using ObjectEffectEffectsHandler = ForwardChanges.PropertyHandlers.ObjectEffect.EffectsHandler;
using ScrollEffectsHandler = ForwardChanges.PropertyHandlers.Scroll.EffectsHandler;
using SpellEffectsHandler = ForwardChanges.PropertyHandlers.Spell.EffectsHandler;

namespace ForwardChanges.Tests;

public sealed class ExactOrderedEffectsTests
{
    [Fact]
    public void EveryMagicItemEffectHandlerUsesExactOrderedSemantics()
    {
        Assert.Equal(ListSemantics.ExactOrdered, new IngestibleEffectsHandler().Semantics);
        Assert.Equal(ListSemantics.ExactOrdered, new IngredientEffectsHandler().Semantics);
        Assert.Equal(ListSemantics.ExactOrdered, new ObjectEffectEffectsHandler().Semantics);
        Assert.Equal(ListSemantics.ExactOrdered, new ScrollEffectsHandler().Semantics);
        Assert.Equal(ListSemantics.ExactOrdered, new SpellEffectsHandler().Semantics);
    }

    [Fact]
    public void IndependentColumnReplacesAtomicValuesAtTheSamePositions()
    {
        var context = Context("A", "B");

        Handler.ProcessColumn(UssepModKey, ["A", "B+condition"], context);
        Handler.ProcessColumn(
            MysticismModKey,
            ["X", "A+condition", "Y", "Z", "W"],
            context);

        Assert.Equal(
            ["X", "A+condition", "Y", "Z", "W"],
            ActiveValues(context));
        Assert.DoesNotContain("B+condition", ActiveValues(context));
        Assert.All(
            context.ForwardValueContexts!.Where(item => !item.IsRemoved),
            item => Assert.Equal(MysticismModKey.ToString(), item.OwnerMod));
    }

    [Fact]
    public void ReversionOfAtomicPositionStillRequiresOwnerPermission()
    {
        var context = Context("A");

        Handler.ProcessColumn(UssepModKey, ["B"], context);
        Handler.ProcessColumn(MysticismModKey, ["A"], context);

        Assert.Equal(["B"], ActiveValues(context));
        Assert.Equal(
            UssepModKey.ToString(),
            Assert.Single(context.ForwardValueContexts!, item => !item.IsRemoved).OwnerMod);
    }

    [Fact]
    public void OwnerCanRemoveItsTrailingPositionalItem()
    {
        var context = Context("A", "B");

        Handler.ProcessColumn(UssepModKey, ["A", "B+condition"], context);
        Handler.ProcessColumn(UssepModKey, ["A"], context);

        Assert.Equal(["A"], ActiveValues(context));
        Assert.Contains(
            context.ForwardValueContexts!,
            item => item.IsRemoved
                && item.AlignmentRowId == 1
                && item.OwnerMod == UssepModKey.ToString());
    }

    [Fact]
    public void AtomicEffectComparisonIncludesEveryGeneratedEffectField()
    {
        var handler = new ObjectEffectEffectsHandler();
        var baseEffect = new FormKey(SkyrimModKey, 0x1234);
        var first = Effect(baseEffect, magnitude: 10, area: 20, duration: 30);
        var same = first.DeepCopy();
        var changed = first.DeepCopy();
        changed.Data!.Duration = 31;
        var changedCondition = first.DeepCopy();
        changedCondition.Conditions.Add(new ConditionFloat { ComparisonValue = 1 });

        Assert.True(handler.AreValuesEqual([first], [same]));
        Assert.False(handler.AreValuesEqual([first], [changed]));
        Assert.False(handler.AreValuesEqual([first], [changedCondition]));
    }

    private static readonly ModKey SkyrimModKey = new("Skyrim.esm", ModType.Master);
    private static readonly ModKey UssepModKey = new("Unofficial Patch.esp", ModType.Plugin);
    private static readonly ModKey MysticismModKey = new("MysticismMagic.esp", ModType.Plugin);
    private static readonly TestExactOrderedHandler Handler = new();

    private static ListPropertyContext<TestValue> Context(params string[] values)
    {
        var originals = values
            .Select((value, position) => Item(value, position, SkyrimModKey))
            .ToList();
        return new ListPropertyContext<TestValue>
        {
            OriginalValueContexts = originals,
            ForwardValueContexts = originals
                .Select(item => Item(item.Value.Value, item.AlignmentRowId!.Value, SkyrimModKey))
                .ToList()
        };
    }

    private static ListPropertyValueContext<TestValue> Item(
        string value,
        int position,
        ModKey owner)
        => new(new TestValue(value), owner.ToString())
        {
            AlignmentRowId = position,
            AlignmentOwnerMod = owner.ToString()
        };

    private static string[] ActiveValues(ListPropertyContext<TestValue> context)
        => context.ForwardValueContexts!
            .Where(item => !item.IsRemoved)
            .Select(item => item.Value.Value)
            .ToArray();

    private static Effect Effect(FormKey baseEffect, float magnitude, int area, int duration)
        => new()
        {
            BaseEffect = new FormLinkNullable<IMagicEffectGetter>(baseEffect),
            Data = new EffectData
            {
                Magnitude = magnitude,
                Area = area,
                Duration = duration
            }
        };

    private sealed record TestValue(string Value);

    private sealed class TestExactOrderedHandler : AbstractListPropertyHandler<TestValue>
    {
        public override string PropertyName => "Effects";
        public override ListSemantics Semantics => ListSemantics.ExactOrdered;

        public void ProcessColumn(
            ModKey modKey,
            IReadOnlyList<string> declaredItems,
            ListPropertyContext<TestValue> context)
            => ProcessExactOrderedItems(
                modKey.ToString(),
                new SkyrimMod(modKey, SkyrimRelease.SkyrimSE),
                declaredItems.Select(value => new TestValue(value)).ToList(),
                context,
                context.ForwardValueContexts!);

        public override void SetValue(IMajorRecord record, List<TestValue>? value)
            => throw new NotSupportedException();

        public override List<TestValue>? GetValue(IMajorRecordGetter record)
            => throw new NotSupportedException();

        protected override bool IsItemEqual(TestValue? item1, TestValue? item2)
            => item1 == item2;
    }
}

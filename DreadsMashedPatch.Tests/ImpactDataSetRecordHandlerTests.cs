using ForwardChanges.PropertyHandlers.ImpactDataSet;
using ForwardChanges.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class ImpactDataSetRecordHandlerTests
{
    private static readonly ModKey OriginalMod = ModKey.FromNameAndExtension("Skyrim.esm");
    private static readonly ModKey FirstMod = ModKey.FromNameAndExtension("First.esp");
    private static readonly ModKey SecondMod = ModKey.FromNameAndExtension("Second.esp");

    [Fact]
    public void ImpactsUsesMaterialKeyedHandler()
    {
        Assert.IsType<ImpactsHandler>(new ImpactDataSetRecordHandler().PropertyHandlers["Impacts"]);
    }

    [Fact]
    public void SameMaterialReplacesImpactWithoutCreatingDuplicate()
    {
        var material = Key(OriginalMod, 0x100);
        var originalImpact = Key(OriginalMod, 0x200);
        var replacementImpact = Key(FirstMod, 0x201);
        var context = CreateContext(new ImpactDataValue(material, originalImpact));

        context.Apply(
            FirstMod,
            [OriginalMod],
            [new ImpactDataValue(material, replacementImpact)]);

        var result = Assert.Single(context.GetForwardValues());
        Assert.Equal(material, result.Material);
        Assert.Equal(replacementImpact, result.Impact);
    }

    [Fact]
    public void ReplacementKeepsSlotAndNewMaterialsAppend()
    {
        var materialA = Key(OriginalMod, 0x100);
        var materialB = Key(OriginalMod, 0x101);
        var materialC = Key(FirstMod, 0x102);
        var context = CreateContext(
            new ImpactDataValue(materialA, Key(OriginalMod, 0x200)),
            new ImpactDataValue(materialB, Key(OriginalMod, 0x201)));

        context.Apply(
            FirstMod,
            [OriginalMod],
            [
                new ImpactDataValue(materialB, Key(FirstMod, 0x301)),
                new ImpactDataValue(materialA, Key(OriginalMod, 0x200)),
                new ImpactDataValue(materialC, Key(FirstMod, 0x302))
            ]);

        Assert.Equal(
            [materialA, materialB, materialC],
            context.GetForwardValues().Select(value => value.Material));
    }

    [Fact]
    public void UnrelatedModCannotRevertImpactToOriginal()
    {
        var material = Key(OriginalMod, 0x100);
        var originalImpact = Key(OriginalMod, 0x200);
        var replacementImpact = Key(FirstMod, 0x201);
        var context = CreateContext(new ImpactDataValue(material, originalImpact));

        context.Apply(
            FirstMod,
            [OriginalMod],
            [new ImpactDataValue(material, replacementImpact)]);
        context.Apply(
            SecondMod,
            [OriginalMod],
            [new ImpactDataValue(material, originalImpact)]);

        Assert.Equal(replacementImpact, Assert.Single(context.GetForwardValues()).Impact);
    }

    [Fact]
    public void MasterCanRevertImpactToOriginal()
    {
        var material = Key(OriginalMod, 0x100);
        var originalImpact = Key(OriginalMod, 0x200);
        var replacementImpact = Key(FirstMod, 0x201);
        var context = CreateContext(new ImpactDataValue(material, originalImpact));

        context.Apply(
            FirstMod,
            [OriginalMod],
            [new ImpactDataValue(material, replacementImpact)]);
        context.Apply(
            SecondMod,
            [OriginalMod, FirstMod],
            [new ImpactDataValue(material, originalImpact)]);

        Assert.Equal(originalImpact, Assert.Single(context.GetForwardValues()).Impact);
    }

    [Fact]
    public void ComparisonUsesMaterialAsKeyAndIgnoresOrder()
    {
        var handler = new ImpactsHandler();
        var materialA = Key(OriginalMod, 0x100);
        var materialB = Key(OriginalMod, 0x101);
        var impactA = Key(OriginalMod, 0x200);
        var impactB = Key(OriginalMod, 0x201);

        Assert.True(handler.AreValuesEqual(
            [Entry(materialA, impactA), Entry(materialB, impactB)],
            [Entry(materialB, impactB), Entry(materialA, impactA)]));
        Assert.False(handler.AreValuesEqual(
            [Entry(materialA, impactA)],
            [Entry(materialA, impactB)]));
    }

    [Fact]
    public void DuplicateMaterialIsRejectedAsAmbiguous()
    {
        var material = Key(OriginalMod, 0x100);

        Assert.False(ImpactsHandler.TryCreateValues(
            [Entry(material, Key(OriginalMod, 0x200)), Entry(material, Key(OriginalMod, 0x201))],
            out _,
            out var error));
        Assert.Contains("occurs more than once", error, StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateFallbackPreservesTheCompleteWinningList()
    {
        var material = Key(OriginalMod, 0x100);
        var winning = new[]
        {
            new ImpactDataValue(material, Key(OriginalMod, 0x200)),
            new ImpactDataValue(material, Key(FirstMod, 0x201)),
            new ImpactDataValue(Key(OriginalMod, 0x102), Key(SecondMod, 0x202))
        };
        var context = new ImpactDataMergeContext();
        context.Initialize([], OriginalMod, winning);

        context.FallbackToWinning();

        Assert.Equal(winning, context.GetForwardValues());
    }

    [Fact]
    public void SetValueWritesBothMaterialAndImpact()
    {
        var handler = new ImpactsHandler();
        var target = new ImpactDataSet(Key(SecondMod, 0x800), SkyrimRelease.SkyrimSE);
        var material = Key(OriginalMod, 0x100);
        var impact = Key(FirstMod, 0x200);

        handler.SetValue(target, [Entry(material, impact)]);

        var written = Assert.Single(target.Impacts);
        Assert.Equal(material, written.Material.FormKey);
        Assert.Equal(impact, written.Impact.FormKey);
    }

    private static ImpactDataMergeContext CreateContext(params ImpactDataValue[] original)
    {
        var context = new ImpactDataMergeContext();
        context.Initialize(original, OriginalMod, original);
        return context;
    }

    private static IImpactDataGetter Entry(FormKey material, FormKey impact)
    {
        var entry = new ImpactData();
        entry.Material.SetTo(material);
        entry.Impact.SetTo(impact);
        return entry;
    }

    private static FormKey Key(ModKey modKey, uint id) => new(modKey, id);
}

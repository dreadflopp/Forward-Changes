using DreadsMashedPatch.Contexts;
using DreadsMashedPatch.PropertyHandlers.General;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;
using NpcItemHandler = DreadsMashedPatch.PropertyHandlers.Npc.ItemHandler;

namespace DreadsMashedPatch.Tests;

public sealed class NpcItemExtraDataTests
{
    [Fact]
    public void AbsentCoedInBothColumnsRemainsAbsentAndDoesNotClaimAChange()
    {
        var forwardItem = ForwardItem(data: null, OriginalModKey);
        var processingMod = new SkyrimMod(PatchModKey, SkyrimRelease.SkyrimSE);
        var changes = new List<string>();

        var changed = Handler.ReconcileExtraData(
            PatchModKey.ToString(),
            processingMod,
            forwardItem,
            recordData: null,
            changes);

        Assert.False(changed);
        Assert.Null(forwardItem.Value.Data);
        Assert.Equal(OriginalModKey.ToString(), forwardItem.OwnerMod);
        Assert.Empty(changes);
    }

    [Fact]
    public void PresentCoedIsCopiedAsAWholeGroup()
    {
        var forwardItem = ForwardItem(data: null, PatchModKey);
        var processingMod = new SkyrimMod(PatchModKey, SkyrimRelease.SkyrimSE);
        var sourceData = ExtraData(0.75f);
        var changes = new List<string>();

        var changed = Handler.ReconcileExtraData(
            PatchModKey.ToString(),
            processingMod,
            forwardItem,
            sourceData,
            changes);

        Assert.True(changed);
        var copiedData = Assert.IsType<ExtraData>(forwardItem.Value.Data);
        Assert.NotSame(sourceData, copiedData);
        Assert.Equal(sourceData.ItemCondition, copiedData.ItemCondition);
        Assert.True(OwnerTargetUtility.AreEqual(sourceData.Owner, copiedData.Owner));
        Assert.Contains("extra data null -> present", changes);
    }

    [Fact]
    public void AbsentCoedClearsTheWholeForwardGroup()
    {
        var forwardItem = ForwardItem(ExtraData(0.5f), PatchModKey);
        var processingMod = new SkyrimMod(PatchModKey, SkyrimRelease.SkyrimSE);
        var changes = new List<string>();

        var changed = Handler.ReconcileExtraData(
            PatchModKey.ToString(),
            processingMod,
            forwardItem,
            recordData: null,
            changes);

        Assert.True(changed);
        Assert.Null(forwardItem.Value.Data);
        Assert.Contains("extra data present -> null", changes);
    }

    [Fact]
    public void ForwardingEqualityDetectsCoedPresenceDifference()
    {
        var winning = new List<ContainerEntry>
        {
            Entry(ItemFormKey, data: null)
        };
        var computed = new List<ContainerEntry>
        {
            Entry(ItemFormKey, ExtraData(0f))
        };

        Assert.False(Handler.AreValuesEqual(computed, winning));
    }

    [Fact]
    public void ForwardingEqualityIncludesCountConditionAndOwner()
    {
        var baseline = Entry(ItemFormKey, ExtraData(0.5f));
        var differentCount = Entry(ItemFormKey, ExtraData(0.5f), count: 2);
        var differentCondition = Entry(ItemFormKey, ExtraData(0.75f));
        var differentOwner = Entry(ItemFormKey, new ExtraData
        {
            ItemCondition = 0.5f,
            Owner = new UntypedOwner()
        });

        Assert.False(Handler.AreValuesEqual([baseline], [differentCount]));
        Assert.False(Handler.AreValuesEqual([baseline], [differentCondition]));
        Assert.False(Handler.AreValuesEqual([baseline], [differentOwner]));
        Assert.True(Handler.AreValuesEqual([baseline], [Entry(ItemFormKey, ExtraData(0.5f))]));
    }

    [Fact]
    public void ForwardingEqualityIsOrderIndependentAndDuplicateAware()
    {
        var first = Entry(ItemFormKey, data: null);
        var duplicateWithCoed = Entry(ItemFormKey, ExtraData(0.5f));
        var other = Entry(OtherItemFormKey, data: null);

        Assert.True(Handler.AreValuesEqual(
            [first, duplicateWithCoed, other],
            [other, duplicateWithCoed, first]));
        Assert.False(Handler.AreValuesEqual(
            [first, duplicateWithCoed, other],
            [other, first, first]));
    }

    private static ListPropertyValueContext<ContainerEntry> ForwardItem(
        ExtraData? data,
        ModKey ownerMod)
        => new(Entry(ItemFormKey, data), ownerMod.ToString());

    private static ContainerEntry Entry(FormKey item, ExtraData? data, int count = 1)
        => new()
        {
            Item = new ContainerItem
            {
                Item = new FormLink<IItemGetter>(item),
                Count = count
            },
            Data = data
        };

    private static ExtraData ExtraData(float condition)
        => new()
        {
            ItemCondition = condition,
            Owner = new UntypedOwner
            {
                OwnerData = new FormLink<ISkyrimMajorRecordGetter>(OwnerFormKey),
                VariableData = new FormLink<ISkyrimMajorRecordGetter>(VariableFormKey)
            }
        };

    private static readonly NpcItemHandler Handler = new();
    private static readonly ModKey OriginalModKey = new("Skyrim.esm", ModType.Master);
    private static readonly ModKey PatchModKey = new("Patch.esp", ModType.Plugin);
    private static readonly FormKey ItemFormKey = new(OriginalModKey, 0x04B586);
    private static readonly FormKey OtherItemFormKey = new(OriginalModKey, 0x03DED1);
    private static readonly FormKey OwnerFormKey = new(OriginalModKey, 0x012345);
    private static readonly FormKey VariableFormKey = new(OriginalModKey, 0x067890);
}

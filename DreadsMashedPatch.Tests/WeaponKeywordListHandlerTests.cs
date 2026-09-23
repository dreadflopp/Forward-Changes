using DreadsMashedPatch.Contexts;
using DreadsMashedPatch.PropertyHandlers.Weapon;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class WeaponKeywordListHandlerTests
{
    private static readonly ModKey Skyrim = ModKey.FromNameAndExtension("Skyrim.esm");
    private static readonly FormKey Sword = new(Skyrim, 0x01E711);
    private static readonly FormKey WarAxe = new(Skyrim, 0x01E712);
    private static readonly FormKey Battleaxe = new(Skyrim, 0x06D932);
    private static readonly FormKey ModKeyword = new(ModKey.FromNameAndExtension("Vigilant.esm"), 0x0C3DAA);

    [Fact]
    public void WeaponRecordsUseTheSpecializedKeywordHandler()
    {
        var handler = new RecordHandlers.WeaponRecordHandler().PropertyHandlers["Keywords"];

        Assert.IsType<WeaponKeywordListHandler>(handler);
    }

    [Fact]
    public void AcceptedSingleTypeAdditionSupersedesOtherConfiguredTypesAndOwnsRemovals()
    {
        const string currentMod = "keyword_test_2.esp";
        var types = new HashSet<FormKey> { Sword, WarAxe, Battleaxe };
        var items = new List<ListPropertyValueContext<IFormLinkGetter<IKeywordGetter>>>
        {
            Item(Sword, "keyword_test_1.esp"),
            Item(WarAxe, currentMod),
            Item(ModKeyword, "Vigilant.esm")
        };

        WeaponKeywordListHandler.ApplyExclusiveWeaponTypeRule(
            currentMod,
            enabled: true,
            types,
            [Link(WarAxe), Link(ModKeyword)],
            items);

        Assert.True(items[0].IsRemoved);
        Assert.Equal(currentMod, items[0].OwnerMod);
        Assert.False(items[1].IsRemoved);
        Assert.False(items[2].IsRemoved);
    }

    [Fact]
    public void ExplicitMultipleTypesArePreserved()
    {
        const string currentMod = "IntentionalMultiType.esp";
        var types = new HashSet<FormKey> { Sword, WarAxe, Battleaxe };
        var items = new List<ListPropertyValueContext<IFormLinkGetter<IKeywordGetter>>>
        {
            Item(Sword, "Earlier.esp"),
            Item(WarAxe, currentMod)
        };

        WeaponKeywordListHandler.ApplyExclusiveWeaponTypeRule(
            currentMod,
            enabled: true,
            types,
            [Link(Sword), Link(WarAxe)],
            items);

        Assert.All(items, item => Assert.False(item.IsRemoved));
    }

    [Fact]
    public void RetainedTypeDoesNotClaimUnrelatedRemovalAuthority()
    {
        var types = new HashSet<FormKey> { Sword, WarAxe };
        var items = new List<ListPropertyValueContext<IFormLinkGetter<IKeywordGetter>>>
        {
            Item(Sword, "Original.esm"),
            Item(WarAxe, "Earlier.esp")
        };

        WeaponKeywordListHandler.ApplyExclusiveWeaponTypeRule(
            "Current.esp",
            enabled: true,
            types,
            [Link(Sword)],
            items);

        Assert.All(items, item => Assert.False(item.IsRemoved));
    }

    [Fact]
    public void DisabledRuleLeavesNormalMergeResultUntouched()
    {
        const string currentMod = "Current.esp";
        var types = new HashSet<FormKey> { Sword, WarAxe };
        var items = new List<ListPropertyValueContext<IFormLinkGetter<IKeywordGetter>>>
        {
            Item(Sword, "Earlier.esp"),
            Item(WarAxe, currentMod)
        };

        WeaponKeywordListHandler.ApplyExclusiveWeaponTypeRule(
            currentMod,
            enabled: false,
            types,
            [Link(WarAxe)],
            items);

        Assert.All(items, item => Assert.False(item.IsRemoved));
    }

    private static IFormLinkGetter<IKeywordGetter> Link(FormKey formKey) =>
        new FormLink<IKeywordGetter>(formKey);

    private static ListPropertyValueContext<IFormLinkGetter<IKeywordGetter>> Item(
        FormKey formKey,
        string owner) => new(Link(formKey), owner);
}

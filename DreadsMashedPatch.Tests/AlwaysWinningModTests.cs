using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Synthesis.CLI;
using Xunit;

namespace DreadsMashedPatch.Tests;

[CollectionDefinition("AlwaysWinningModTests", DisableParallelization = true)]
public sealed class AlwaysWinningModTestCollection;

[Collection("AlwaysWinningModTests")]
public sealed class AlwaysWinningModTests
{
    private static readonly ModKey OriginalKey = ModKey.FromNameAndExtension("Skyrim.esm");
    private static readonly ModKey PriorityKey = ModKey.FromNameAndExtension("Priority.esp");
    private static readonly ModKey LaterKey = ModKey.FromNameAndExtension("Later.esp");
    private static readonly ModKey PatchKey = ModKey.FromNameAndExtension("Patch.esp");

    [Fact]
    public void OverwrittenPriorityModIsCopiedAsTheCompleteRecordSource()
    {
        var formKey = new FormKey(OriginalKey, 0x800);
        var original = CreateMod(OriginalKey);
        original.Keywords.Add(new Keyword(formKey, SkyrimRelease.SkyrimSE) { EditorID = "Original" });
        var priority = CreateMod(PriorityKey);
        priority.Keywords.Add(new Keyword(formKey, SkyrimRelease.SkyrimSE)
        {
            EditorID = "PriorityVersion",
            MajorRecordFlagsRaw = 0x4000
        });
        var later = CreateMod(LaterKey);
        later.Keywords.Add(new Keyword(formKey, SkyrimRelease.SkyrimSE)
        {
            EditorID = "LaterVersion",
            MajorRecordFlagsRaw = 0
        });

        PatcherSettings.Apply(new PatcherConfiguration
        {
            // Later.esp wins in load order, but Priority.esp occurs last in the
            // configured list and therefore has the stronger explicit priority.
            AlwaysWinningMods = [LaterKey.FileName.String, PriorityKey.FileName.String]
        });
        using var state = CreateState(original, priority, later);
        var winningContexts = state.LoadOrder.PriorityOrder
            .WinningContextOverrides<
                ISkyrimMod,
                ISkyrimModGetter,
                Mutagen.Bethesda.Plugins.Records.IMajorRecord,
                Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter>(state.LinkCache)
            .ToArray();

        new KeywordRecordHandler().Process(state, winningContexts);

        var result = Assert.Single(state.PatchMod.Keywords);
        Assert.Equal("PriorityVersion", result.EditorID);
        Assert.Equal(0x4000, result.MajorRecordFlagsRaw);
    }

    [Fact]
    public void NoPatchRecordIsCreatedWhenTheLastConfiguredSourceAlreadyWins()
    {
        var formKey = new FormKey(OriginalKey, 0x801);
        var original = CreateMod(OriginalKey);
        original.Keywords.Add(new Keyword(formKey, SkyrimRelease.SkyrimSE) { EditorID = "Original" });
        var priority = CreateMod(PriorityKey);
        priority.Keywords.Add(new Keyword(formKey, SkyrimRelease.SkyrimSE) { EditorID = "PriorityVersion" });
        var later = CreateMod(LaterKey);
        later.Keywords.Add(new Keyword(formKey, SkyrimRelease.SkyrimSE) { EditorID = "LaterVersion" });

        PatcherSettings.Apply(new PatcherConfiguration
        {
            AlwaysWinningMods = [PriorityKey.FileName.String, LaterKey.FileName.String]
        });
        using var state = CreateState(original, priority, later);
        var winningContexts = state.LoadOrder.PriorityOrder
            .WinningContextOverrides<
                ISkyrimMod,
                ISkyrimModGetter,
                Mutagen.Bethesda.Plugins.Records.IMajorRecord,
                Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter>(state.LinkCache)
            .ToArray();

        new KeywordRecordHandler().Process(state, winningContexts);

        Assert.Empty(state.PatchMod.Keywords);
    }

    private static SkyrimMod CreateMod(ModKey modKey) => new(modKey, SkyrimRelease.SkyrimSE);

#pragma warning disable CS0618
    private static SynthesisState<ISkyrimMod, ISkyrimModGetter> CreateState(params SkyrimMod[] mods)
    {
        var patchMod = CreateMod(PatchKey);
        var listings = mods
            .Cast<ISkyrimModGetter>()
            .Append(patchMod)
            .Select(mod => new ModListing<ISkyrimModGetter>(mod))
            .ToArray();
        var loadOrder = new LoadOrder<IModListing<ISkyrimModGetter>>(listings);
        var linkCache = loadOrder.ToImmutableLinkCache<ISkyrimMod, ISkyrimModGetter>();
        var arguments = new RunSynthesisMutagenPatcher
        {
            OutputPath = @"C:\Temp\PriorityPatch.esp",
            DataFolderPath = @"C:\Temp\Data",
            LoadOrderFilePath = @"C:\Temp\plugins.txt",
            GameRelease = GameRelease.SkyrimSE
        };

        return new SynthesisState<ISkyrimMod, ISkyrimModGetter>(
            arguments,
            listings.Select(listing => new LoadOrderListing(listing.ModKey, listing.Enabled)).ToArray(),
            loadOrder,
            linkCache,
            null!,
            patchMod,
            null,
            null,
            null,
            CancellationToken.None,
            null);
    }
#pragma warning restore CS0618
}

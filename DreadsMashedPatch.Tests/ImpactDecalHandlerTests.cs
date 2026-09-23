using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Impact;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Synthesis.CLI;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class ImpactDecalHandlerTests
{
    private static readonly ModKey OriginalModKey = new("Skyrim.esm", ModType.Master);
    private static readonly ModKey FixModKey = new("Unofficial Skyrim Modders Patch.esp", ModType.Plugin);
    private static readonly ModKey WinningModKey = new("Audio Overhaul Skyrim.esp", ModType.Plugin);
    private static readonly ModKey PatchModKey = new("DreadsMashedPatch.esp", ModType.Plugin);
    private const uint RecordId = 0x1234;

    [Fact]
    public void HandlerSplitsDecalIntoSafeLogicalProperties()
    {
        var handlers = new ImpactRecordHandler().PropertyHandlers;

        Assert.DoesNotContain("Decal", handlers.Keys);
        Assert.IsType<ImpactDecalPresenceHandler>(handlers["Decal.Presence"]);
        Assert.IsType<ImpactDecalBoundsHandler>(handlers["Decal.Bounds"]);
        Assert.IsType<ImpactDecalParallaxHandler>(handlers["Decal.Parallax"]);
        Assert.IsType<SimpleReflectionFlagPropertyHandler<Decal.Flag, IImpact, IImpactGetter>>(
            handlers["Decal.Flags"]);
        Assert.DoesNotContain("Decal.Unknown", handlers.Keys);
    }

    [Fact]
    public void LaterPartialDecalOverrideDoesNotEraseUnauthorizedBoundsFix()
    {
        var original = CreateImpact(OriginalModKey, minSize: 8, maxSize: 32, flags: (Decal.Flag)240);
        var fix = CreateImpact(FixModKey, minSize: 16, maxSize: 64, flags: 0);
        var winning = CreateImpact(WinningModKey, minSize: 8, maxSize: 32, flags: 0);
        var handler = new ImpactDecalBoundsHandler();
        var propertyContext = ((IPropertyHandler)handler).CreatePropertyContext();
        using var state = CreateState(
            CreateMod(OriginalModKey),
            CreateMod(FixModKey),
            CreateMod(WinningModKey));

        handler.InitializeContext(
            CreateContext(OriginalModKey, original),
            CreateContext(WinningModKey, winning),
            propertyContext);
        handler.UpdatePropertyContext(CreateContext(FixModKey, fix), state, propertyContext);
        handler.UpdatePropertyContext(CreateContext(WinningModKey, winning), state, propertyContext);

        Assert.Equal(
            new ImpactDecalBounds(16, 64, 16, 64),
            Assert.IsType<ImpactDecalBounds>(propertyContext.GetForwardValue()));
    }

    [Fact]
    public void ApplyingBoundsPreservesOtherDecalFields()
    {
        var impact = CreateImpact(WinningModKey, minSize: 8, maxSize: 32, flags: 0);
        var decal = impact.Decal!;
        var handler = new ImpactDecalBoundsHandler();

        handler.SetValue(impact, new ImpactDecalBounds(16, 64, 16, 64));

        Assert.Same(decal, impact.Decal);
        Assert.Equal(16, impact.Decal!.MinWidth);
        Assert.Equal(64, impact.Decal.MaxWidth);
        Assert.Equal(16, impact.Decal.MinHeight);
        Assert.Equal(64, impact.Decal.MaxHeight);
        Assert.Equal((Decal.Flag)0, impact.Decal.Flags);
        Assert.Equal(32, impact.Decal.Depth);
        Assert.Equal((ushort)15709, impact.Decal.Unknown);
    }

    [Fact]
    public void RemovingDecalSuppressesForwardedChildValues()
    {
        var impact = CreateImpact(WinningModKey, minSize: 8, maxSize: 32, flags: 0);
        var handler = new ImpactRecordHandler();

        handler.ApplyForwardedProperties(
            impact,
            new Dictionary<string, object?>
            {
                ["Decal.Presence"] = false,
                ["Decal.Bounds"] = new ImpactDecalBounds(16, 64, 16, 64)
            });

        Assert.Null(impact.Decal);
    }

    private static Impact CreateImpact(ModKey modKey, float minSize, float maxSize, Decal.Flag flags) =>
        new(new FormKey(modKey, RecordId), SkyrimRelease.SkyrimSE)
        {
            Decal = new Decal
            {
                MinWidth = minSize,
                MaxWidth = maxSize,
                MinHeight = minSize,
                MaxHeight = maxSize,
                Depth = 32,
                Shininess = 4,
                ParallaxScale = 1,
                ParallaxPasses = 4,
                Flags = flags,
                Unknown = 15709
            }
        };

    private static IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> CreateContext(
        ModKey modKey,
        IMajorRecord record) =>
        new ModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>(
            modKey,
            record,
            (_, _) => throw new NotSupportedException(),
            (_, _, _, _) => throw new NotSupportedException());

    private static SkyrimMod CreateMod(ModKey modKey) => new(modKey, SkyrimRelease.SkyrimSE);

#pragma warning disable CS0618 // SynthesisState remains useful as a complete test implementation of IPatcherState.
    private static SynthesisState<ISkyrimMod, ISkyrimModGetter> CreateState(params SkyrimMod[] mods)
    {
        var patchMod = CreateMod(PatchModKey);
        var listings = mods
            .Cast<ISkyrimModGetter>()
            .Append(patchMod)
            .Select(mod => new ModListing<ISkyrimModGetter>(mod))
            .ToArray();
        var loadOrder = new LoadOrder<IModListing<ISkyrimModGetter>>(listings);
        var linkCache = loadOrder.ToImmutableLinkCache<ISkyrimMod, ISkyrimModGetter>();
        var arguments = new RunSynthesisMutagenPatcher
        {
            OutputPath = @"C:\Temp\ImpactDecalPatch.esp",
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

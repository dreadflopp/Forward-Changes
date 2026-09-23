using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Synthesis.CLI;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class LocationReferenceConflictPolicyTests
{
    private static readonly ModKey OriginalModKey = new("Skyrim.esm", ModType.Master);
    private static readonly ModKey AddedModKey = new("AddsLocationReference.esp", ModType.Plugin);
    private static readonly ModKey LaterModKey = new("LaterOverride.esp", ModType.Plugin);
    private static readonly ModKey PatchModKey = new("LocationReferencePatch.esp", ModType.Plugin);
    private static readonly FormKey RecordKey = new(OriginalModKey, 0x1234);
    private static readonly FormKey LocationKey = new(OriginalModKey, 0x5678);

    [Fact]
    public void PlacedHandlersKeepLocationReferenceOnSharedConflictAwarePath()
    {
        Assert.IsType<SimpleReflectionFormLinkPropertyHandler<ILocationGetter, IPlacedObject, IPlacedObjectGetter>>(
            new PlacedObjectRecordHandler().PropertyHandlers["LocationReference"]);
        Assert.IsType<SimpleReflectionFormLinkPropertyHandler<ILocationGetter, IPlacedNpc, IPlacedNpcGetter>>(
            new PlacedNpcRecordHandler().PropertyHandlers["LocationReference"]);
    }

    [Fact]
    public void AddedLocationReferenceThatSurvivesMatchesWinningValue()
    {
        var original = CreatePlacedObject(OriginalModKey);
        var added = CreatePlacedObject(AddedModKey, LocationKey);
        var handler = new PlacedObjectRecordHandler().PropertyHandlers["LocationReference"];
        var propertyContext = handler.CreatePropertyContext();

        handler.InitializeContext(
            CreateContext(OriginalModKey, original),
            CreateContext(AddedModKey, added),
            propertyContext);
        handler.UpdatePropertyContext(
            CreateContext(AddedModKey, added),
            null!,
            propertyContext);

        Assert.True(handler.AreValuesEqual(
            propertyContext.GetForwardValue(),
            handler.GetValue(added)));
    }

    [Fact]
    public void AddedLocationReferenceRemainsCandidateWhenLaterOverrideOmitsIt()
    {
        var original = CreatePlacedObject(OriginalModKey);
        var added = CreatePlacedObject(AddedModKey, LocationKey);
        var later = CreatePlacedObject(LaterModKey);
        var handler = new PlacedObjectRecordHandler().PropertyHandlers["LocationReference"];
        var propertyContext = handler.CreatePropertyContext();
        using var state = CreateState(
            CreateMod(OriginalModKey),
            CreateMod(AddedModKey),
            CreateMod(LaterModKey));

        handler.InitializeContext(
            CreateContext(OriginalModKey, original),
            CreateContext(LaterModKey, later),
            propertyContext);
        handler.UpdatePropertyContext(
            CreateContext(AddedModKey, added),
            state,
            propertyContext);
        handler.UpdatePropertyContext(
            CreateContext(LaterModKey, later),
            state,
            propertyContext);

        Assert.True(handler.AreValuesEqual(
            propertyContext.GetForwardValue(),
            handler.GetValue(added)));
    }

    [Fact]
    public void LaterOverrideMayRemoveLocationReferenceWhenItMastersAddingMod()
    {
        var original = CreatePlacedObject(OriginalModKey);
        var added = CreatePlacedObject(AddedModKey, LocationKey);
        var later = CreatePlacedObject(LaterModKey);
        var laterMod = CreateMod(LaterModKey);
        laterMod.ModHeader.MasterReferences.Add(new MasterReference { Master = AddedModKey });
        var handler = new PlacedObjectRecordHandler().PropertyHandlers["LocationReference"];
        var propertyContext = handler.CreatePropertyContext();
        using var state = CreateState(
            CreateMod(OriginalModKey),
            CreateMod(AddedModKey),
            laterMod);

        handler.InitializeContext(
            CreateContext(OriginalModKey, original),
            CreateContext(LaterModKey, later),
            propertyContext);
        handler.UpdatePropertyContext(
            CreateContext(AddedModKey, added),
            state,
            propertyContext);
        handler.UpdatePropertyContext(
            CreateContext(LaterModKey, later),
            state,
            propertyContext);

        Assert.True(handler.AreValuesEqual(
            propertyContext.GetForwardValue(),
            handler.GetValue(later)));
    }

    [Fact]
    public void ApplyingLocationReferenceDoesNotAlterOtherPlacedObjectData()
    {
        var baseKey = new FormKey(OriginalModKey, 0x1111);
        var navMeshKey = new FormKey(OriginalModKey, 0x2222);
        var target = CreatePlacedObject(LaterModKey);
        target.Base.SetTo(baseKey);
        target.NavigationDoorLink = new NavigationDoorLink
        {
            NavMesh = new FormLink<INavigationMeshGetter>(navMeshKey),
            TeleportMarkerTriangle = 171,
            Unused = 0
        };
        target.Placement = new Placement
        {
            Position = new Noggog.P3Float(-2097.0454f, 366.64948f, 13.355549f),
            Rotation = new Noggog.P3Float(4.712391f, 0.2617994f, 4.712389f)
        };
        var navigationDoorLink = target.NavigationDoorLink;
        var placement = target.Placement;
        var handler = new PlacedObjectRecordHandler().PropertyHandlers["LocationReference"];

        handler.SetValue(target, new FormLinkNullable<ILocationGetter>(LocationKey));

        Assert.Equal(LocationKey, target.LocationReference.FormKey);
        Assert.Equal(baseKey, target.Base.FormKey);
        Assert.Same(navigationDoorLink, target.NavigationDoorLink);
        Assert.Equal(navMeshKey, target.NavigationDoorLink!.NavMesh.FormKey);
        Assert.Equal((short)171, target.NavigationDoorLink.TeleportMarkerTriangle);
        Assert.Same(placement, target.Placement);
        Assert.Equal(new Noggog.P3Float(-2097.0454f, 366.64948f, 13.355549f), target.Placement!.Position);
        Assert.Equal(new Noggog.P3Float(4.712391f, 0.2617994f, 4.712389f), target.Placement.Rotation);
    }

    private static PlacedObject CreatePlacedObject(ModKey modKey, FormKey? location = null)
    {
        var record = new PlacedObject(new FormKey(modKey, RecordKey.ID), SkyrimRelease.SkyrimSE);
        if (location is { } locationKey)
        {
            record.LocationReference.SetTo(locationKey);
        }

        return record;
    }

    private static IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> CreateContext(
        ModKey modKey,
        IMajorRecord record)
        => new ModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>(
            modKey,
            record,
            (_, _) => throw new NotSupportedException(),
            (_, _, _, _) => throw new NotSupportedException());

    private static SkyrimMod CreateMod(ModKey modKey) =>
        new(modKey, SkyrimRelease.SkyrimSE);

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
            OutputPath = @"C:\Temp\LocationReferencePatch.esp",
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

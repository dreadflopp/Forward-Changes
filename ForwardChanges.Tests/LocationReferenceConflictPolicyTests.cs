using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class LocationReferenceConflictPolicyTests
{
    private static readonly ModKey OriginalModKey = new("Skyrim.esm", ModType.Master);
    private static readonly ModKey AddedModKey = new("AddsLocationReference.esp", ModType.Plugin);
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
        var handler = new PlacedObjectRecordHandler().PropertyHandlers["LocationReference"];
        var propertyContext = handler.CreatePropertyContext();

        handler.InitializeContext(
            CreateContext(OriginalModKey, original),
            CreateContext(OriginalModKey, original),
            propertyContext);
        handler.UpdatePropertyContext(
            CreateContext(AddedModKey, added),
            null!,
            propertyContext);

        Assert.False(handler.AreValuesEqual(
            propertyContext.GetForwardValue(),
            handler.GetValue(original)));
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
}

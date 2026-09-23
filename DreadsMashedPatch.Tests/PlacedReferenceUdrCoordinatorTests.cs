using DreadsMashedPatch.Contexts;
using DreadsMashedPatch.Contexts.Interfaces;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class PlacedReferenceUdrCoordinatorTests
{
    private static readonly ModKey OriginalModKey = new("Skyrim.esm", ModType.Master);
    private static readonly ModKey UdrModKey = new("UnofficialFixes.esp", ModType.Plugin);
    private static readonly ModKey WinnerModKey = new("LaterPatch.esp", ModType.Plugin);

    [Fact]
    public void SelectedInitiallyDisabledOwnerSuppliesCompleteSafeUdrBundle()
    {
        var original = CreateNormal(OriginalModKey);
        var udr = CreateSafeUdr(UdrModKey);
        var winner = CreateNormal(WinnerModKey);
        var contexts = CreateContexts(winner, udr, original);
        var propertyContexts = CreateFlagContexts(isSet: true, UdrModKey);

        var result = PlacedReferenceUdrCoordinator.Coordinate(contexts, propertyContexts);

        Assert.Same(udr.Placement, result.ForwardValues["Placement"]);
        Assert.Same(udr.EnableParent, result.ForwardValues["EnableParent"]);
        Assert.Contains("safe UDR bundle", result.AuditMessage);
    }

    [Fact]
    public void ClearedInitiallyDisabledOwnerSuppliesCompleteNonUdrRestoration()
    {
        var original = CreateNormal(OriginalModKey);
        var udr = CreateSafeUdr(UdrModKey);
        var winner = CreateNormal(WinnerModKey);
        var contexts = CreateContexts(winner, udr, original);
        var propertyContexts = CreateFlagContexts(isSet: false, WinnerModKey);

        var result = PlacedReferenceUdrCoordinator.Coordinate(contexts, propertyContexts);

        Assert.Same(winner.Placement, result.ForwardValues["Placement"]);
        Assert.Null(result.ForwardValues["EnableParent"]);
        Assert.Contains("non-UDR restoration", result.AuditMessage);
    }

    [Fact]
    public void OrdinaryInitiallyDisabledStateIsNotTreatedAsSafeUdr()
    {
        var original = CreateNormal(OriginalModKey);
        var udr = CreateSafeUdr(UdrModKey);
        var winner = CreateNormal(WinnerModKey);
        winner.SkyrimMajorRecordFlags = SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled;
        var contexts = CreateContexts(winner, udr, original);
        var propertyContexts = CreateFlagContexts(isSet: true, WinnerModKey);

        var result = PlacedReferenceUdrCoordinator.Coordinate(contexts, propertyContexts);

        Assert.Empty(result.ForwardValues);
        Assert.Null(result.AuditMessage);
    }

    private static PlacedObject CreateNormal(ModKey modKey)
    {
        return new PlacedObject(new FormKey(modKey, 0xDB0BA), SkyrimRelease.SkyrimSE)
        {
            Placement = new Placement
            {
                Position = new P3Float(2002.1953125f, -568.9878540f, -5.7508850f),
                Rotation = new P3Float(-0.9324064f, -0.7878084f, 5.3921800f)
            }
        };
    }

    private static PlacedObject CreateSafeUdr(ModKey modKey)
    {
        var record = CreateNormal(modKey);
        record.SkyrimMajorRecordFlags = SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled;
        record.Placement!.Position = new P3Float(
            record.Placement.Position.X,
            record.Placement.Position.Y,
            -30000f);
        record.EnableParent = new EnableParent
        {
            Reference = new FormLink<IPlacedGetter>(Constants.Player.FormKey),
            Flags = EnableParent.Flag.SetEnableStateToOppositeOfParent
        };
        return record;
    }

    private static Dictionary<string, IPropertyContext> CreateFlagContexts(bool isSet, ModKey owner)
    {
        var flags = new FlagPropertyContext<SkyrimMajorRecord.SkyrimMajorRecordFlag>();
        flags.SetFlagContext(
            SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled,
            isSet,
            owner.ToString());
        return new Dictionary<string, IPropertyContext>
        {
            ["SkyrimMajorRecordFlags"] = flags
        };
    }

    private static IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] CreateContexts(
        params PlacedObject[] records)
    {
        return records
            .Select(record =>
                (IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>)
                new ModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>(
                    record.FormKey.ModKey,
                    record,
                    (_, _) => throw new NotSupportedException(),
                    (_, _, _, _) => throw new NotSupportedException()))
            .ToArray();
    }
}

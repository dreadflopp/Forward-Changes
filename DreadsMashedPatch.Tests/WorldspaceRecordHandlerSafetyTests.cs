using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Worldspace;
using ForwardChanges.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class WorldspaceRecordHandlerSafetyTests
{
    [Fact]
    public void UsesSingleHeaderFlagPathAndExcludesGeneratedOrIgnoredData()
    {
        var handlers = new WorldspaceRecordHandler().PropertyHandlers;

        Assert.Contains("MajorRecordFlagsRaw", handlers.Keys);
        Assert.DoesNotContain("SkyrimMajorRecordFlags", handlers.Keys);
        Assert.DoesNotContain("MajorFlags", handlers.Keys);
        Assert.DoesNotContain("MaxHeight", handlers.Keys);
        Assert.DoesNotContain("CanopyShadow", handlers.Keys);
        Assert.IsType<SimpleReflectionFlagPropertyHandler<Worldspace.Flag, IWorldspace, IWorldspaceGetter>>(
            handlers["Flags"]);
    }

    [Fact]
    public void GeneratedParentCopyPreservesOverlayWorldspaceLink()
    {
        using var stream = CreateWorldspacePlugin();
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(
            stream,
            SkyrimRelease.SkyrimSE,
            SourceModKey);
        var sourceChild = overlay.Worldspaces.Single(worldspace => worldspace.FormKey.ID == 0x801);
        var target = CreateWorldspace(PatchModKey, 0x901);
        var handler = new WorldspaceRecordHandler().PropertyHandlers["Parent"];

        handler.SetValue(target, handler.GetValue(sourceChild));

        Assert.NotNull(target.Parent);
        Assert.Equal(new FormKey(SourceModKey, 0x800), target.Parent!.Worldspace.FormKey);
        Assert.Equal(
            WorldspaceParent.Flag.UseLandData | WorldspaceParent.Flag.UseWaterData,
            target.Parent.Flags);

        var patch = new SkyrimMod(PatchModKey, SkyrimRelease.SkyrimSE);
        patch.Worldspaces.Add(target);
        using var output = new MemoryStream();
        patch.WriteToBinary(output);
        output.Position = 0;
        using var writtenOverlay = SkyrimMod.CreateFromBinaryOverlay(
            output,
            SkyrimRelease.SkyrimSE,
            PatchModKey);
        var written = Assert.Single(writtenOverlay.Worldspaces);

        Assert.NotNull(written.Parent);
        Assert.Equal(new FormKey(SourceModKey, 0x800), written.Parent!.Worldspace.FormKey);
        Assert.Equal(
            WorldspaceParent.Flag.UseLandData | WorldspaceParent.Flag.UseWaterData,
            written.Parent.Flags);
    }

    [Fact]
    public void GeneratedAggregateCopiesPreserveOverlayValues()
    {
        using var stream = CreateWorldspacePlugin();
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(
            stream,
            SkyrimRelease.SkyrimSE,
            SourceModKey);
        var sourceChild = overlay.Worldspaces.Single(worldspace => worldspace.FormKey.ID == 0x801);
        var target = CreateWorldspace(PatchModKey, 0x902);
        var handlers = new WorldspaceRecordHandler().PropertyHandlers;

        handlers["LandDefaults"].SetValue(target, handlers["LandDefaults"].GetValue(sourceChild));
        handlers["MapData"].SetValue(target, handlers["MapData"].GetValue(sourceChild));

        Assert.Equal(-1024f, target.LandDefaults!.DefaultLandHeight);
        Assert.Equal(128f, target.LandDefaults.DefaultWaterHeight);
        Assert.Equal(new P2Int(64, 32), target.MapData!.UsableDimensions);
        Assert.Equal(45000f, target.MapData.CameraMinHeight);
    }

    [Fact]
    public void LodDataIsOneAtomicStructure()
    {
        var handler = new LodDataHandler();
        var target = CreateWorldspace(PatchModKey, 0x903);
        var water = new FormKey(SourceModKey, 0x810);

        handler.SetValue(target, new WorldspaceLodDataValue(water, 512f));
        Assert.Equal(water, target.LodWater.FormKey);
        Assert.Equal(512f, target.LodWaterHeight);

        handler.SetValue(target, new WorldspaceLodDataValue(FormKey.Null, 256f));
        Assert.True(target.LodWater.IsNull);
        Assert.Equal(256f, target.LodWaterHeight);

        Assert.Throws<InvalidOperationException>(() =>
            handler.SetValue(target, new WorldspaceLodDataValue(water, null)));
    }

    [Fact]
    public void DistantLodAbsenceUsesRequiredSemanticDefault()
    {
        var handler = new DistantLodMultiplierHandler();
        var worldspace = CreateWorldspace(SourceModKey, 0x804);
        worldspace.DistantLodMultiplier = null;

        Assert.Equal(1f, handler.GetValue(worldspace));
        Assert.True(handler.AreValuesEqual(1f, handler.GetValue(worldspace)));

        handler.SetValue(worldspace, 1f);
        Assert.Equal(1f, worldspace.DistantLodMultiplier);
    }

    [Fact]
    public void WorldMapOffsetIsCopiedAsOneRequiredStructure()
    {
        var handler = new WorldMapOffsetHandler();
        var target = CreateWorldspace(PatchModKey, 0x905);
        var value = new WorldspaceMapOffsetValue(2.5f, new P3Float(10, 20, 30));

        handler.SetValue(target, value);

        Assert.Equal(2.5f, target.WorldMapOffsetScale);
        Assert.Equal(new P3Float(10, 20, 30), target.WorldMapCellOffset);
        Assert.Throws<InvalidOperationException>(() => handler.SetValue(target, null));
    }

    [Fact]
    public void UnknownWorldspaceFlagBitsSurviveNamedFlagWrites()
    {
        var handler = new WorldspaceRecordHandler();
        var target = CreateWorldspace(PatchModKey, 0x906);
        target.Flags = (Worldspace.Flag)0x04;

        handler.ApplyForwardedProperties(target, new Dictionary<string, object?>
        {
            ["Flags"] = Worldspace.Flag.CannotFastTravel
        });

        Assert.Equal(0x06, (int)target.Flags);
    }

    [Fact]
    public void InheritanceAndFixedDimensionFieldsAreAtomicTriggers()
    {
        var handler = new TestableWorldspaceRecordHandler();

        Assert.Equal(
            new HashSet<string>(StringComparer.Ordinal)
            {
                "Parent",
                "Climate",
                "Water",
                "LodData",
                "LandDefaults",
                "MapData",
                "Flags",
                "FixedDimensionsCenterCell"
            },
            handler.AtomicTriggers);

        var previous = CreateWorldspace(SourceModKey, 0x807);
        var current = CreateWorldspace(SourceModKey, 0x807);
        current.Parent = new WorldspaceParent();
        current.Parent.Worldspace.SetTo(new FormKey(SourceModKey, 0x800));

        Assert.Equal(["Parent"], handler.GetChangedTriggers(previous, current));
    }

    [Fact]
    public void CompositeHeaderHandlerOwnsWorldspaceMajorFlag()
    {
        var handler = new WorldspaceRecordHandler();
        var target = CreateWorldspace(PatchModKey, 0x908);

        handler.ApplyForwardedProperties(target, new Dictionary<string, object?>
        {
            ["MajorRecordFlagsRaw"] = (int)Worldspace.MajorFlag.CanNotWait
        });

        Assert.True(target.MajorFlags.HasFlag(Worldspace.MajorFlag.CanNotWait));
    }

    private static MemoryStream CreateWorldspacePlugin()
    {
        var mod = new SkyrimMod(SourceModKey, SkyrimRelease.SkyrimSE);
        var parent = CreateWorldspace(SourceModKey, 0x800);
        var child = CreateWorldspace(SourceModKey, 0x801);
        child.Parent = new WorldspaceParent
        {
            Flags = WorldspaceParent.Flag.UseLandData | WorldspaceParent.Flag.UseWaterData
        };
        child.Parent.Worldspace.SetTo(parent.FormKey);
        child.LandDefaults = new WorldspaceLandDefaults
        {
            DefaultLandHeight = -1024f,
            DefaultWaterHeight = 128f
        };
        child.MapData = new WorldspaceMap
        {
            UsableDimensions = new P2Int(64, 32),
            NorthwestCellCoords = new P2Int16(-10, 10),
            SoutheastCellCoords = new P2Int16(10, -10),
            CameraMinHeight = 45000f,
            CameraMaxHeight = 75000f,
            CameraInitialPitch = 45f
        };
        mod.Worldspaces.Add(parent);
        mod.Worldspaces.Add(child);

        var stream = new MemoryStream();
        mod.WriteToBinary(stream);
        stream.Position = 0;
        return stream;
    }

    private static Worldspace CreateWorldspace(ModKey modKey, uint id)
        => new(new FormKey(modKey, id), SkyrimRelease.SkyrimSE)
        {
            WorldMapOffsetScale = 1f,
            DistantLodMultiplier = 1f
        };

    private sealed class TestableWorldspaceRecordHandler : WorldspaceRecordHandler
    {
        public IReadOnlySet<string> AtomicTriggers => AtomicOwnershipTriggerProperties;

        public IReadOnlyList<string> GetChangedTriggers(
            IWorldspaceGetter previous,
            IWorldspaceGetter current) =>
            GetChangedAtomicOwnershipTriggerProperties(previous, current);
    }

    private static readonly ModKey SourceModKey = new("WorldspaceSource.esp", ModType.Plugin);
    private static readonly ModKey PatchModKey = new("WorldspacePatch.esp", ModType.Plugin);
}

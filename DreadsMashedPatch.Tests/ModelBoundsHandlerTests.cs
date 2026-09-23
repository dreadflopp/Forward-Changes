using DreadsMashedPatch.Contexts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Synthesis.CLI;
using Noggog;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class ModelBoundsHandlerTests
{
    private static readonly ModKey OriginalModKey = new("Skyrim.esm", ModType.Master);
    private static readonly ModKey ModelModKey = new("ModelMod.esp", ModType.Plugin);
    private static readonly ModKey BoundsModKey = new("BoundsMod.esp", ModType.Plugin);
    private static readonly ModKey PatchModKey = new("DreadsMashedPatch.esp", ModType.Plugin);

    [Fact]
    public void AllRecordHandlersWithMainModelsUseOneModelBoundsOwner()
    {
        var recordHandlers = new DreadsMashedPatch.RecordHandlers.Abstracts.AbstractRecordHandler[]
        {
            new ActivatorRecordHandler(), new AddonNodeRecordHandler(),
            new AlchemicalApparatusRecordHandler(), new AmmunitionRecordHandler(),
            new ArtObjectRecordHandler(), new BookRecordHandler(), new ContainerRecordHandler(),
            new DoorRecordHandler(), new ExplosionRecordHandler(), new FloraRecordHandler(),
            new FurnitureRecordHandler(), new GrassRecordHandler(), new HazardRecordHandler(),
            new IdleMarkerRecordHandler(), new IngestibleRecordHandler(), new IngredientRecordHandler(),
            new KeyRecordHandler(), new LeveledNpcRecordHandler(), new LightRecordHandler(),
            new MiscItemRecordHandler(), new MoveableStaticRecordHandler(), new ProjectileRecordHandler(),
            new ScrollRecordHandler(), new SoulGemRecordHandler(), new StaticRecordHandler(),
            new TalkingActivatorRecordHandler(), new TreeRecordHandler(), new WeaponRecordHandler()
        };

        foreach (var recordHandler in recordHandlers)
        {
            Assert.IsType<ModelBoundsHandler>(recordHandler.PropertyHandlers["ModelAndBounds"]);
            Assert.DoesNotContain("Model", recordHandler.PropertyHandlers.Keys);
            Assert.DoesNotContain("ObjectBounds", recordHandler.PropertyHandlers.Keys);
        }

        var armorHandlers = new ArmorRecordHandler().PropertyHandlers;
        Assert.IsType<ModelBoundsHandler>(armorHandlers["WorldModelAndBounds"]);
        Assert.DoesNotContain("WorldModel", armorHandlers.Keys);
        Assert.DoesNotContain("ObjectBounds", armorHandlers.Keys);
    }

    [Fact]
    public void BoundsOnlyChangeStillForwardsWhenModelDoesNotChange()
    {
        var original = CreateStatic("Meshes/Original.nif", 1);
        var boundsEdit = CreateStatic("Meshes/Original.nif", 2);

        var result = Resolve(original, (BoundsModKey, boundsEdit));

        Assert.Equal("Meshes/Original.nif", GetModelPath(result));
        Assert.Equal(Bounds(2).Second, result.Bounds!.Second);
    }

    [Fact]
    public void AcceptedModelFilenameChangeTakesBoundsFromTheSameMod()
    {
        var original = CreateStatic("Meshes/Original.nif", 1);
        var modelEdit = CreateStatic("Meshes/Replacer.nif", 7);

        var result = Resolve(original, (ModelModKey, modelEdit));

        Assert.Equal("Meshes/Replacer.nif", GetModelPath(result));
        Assert.Equal(Bounds(7).Second, result.Bounds!.Second);
    }

    [Fact]
    public void LaterExplicitBoundsChangeCanOverrideModelOwnedBounds()
    {
        var original = CreateStatic("Meshes/Original.nif", 1);
        var modelEdit = CreateStatic("Meshes/Replacer.nif", 7);
        var boundsEdit = CreateStatic("Meshes/Replacer.nif", 9);

        var result = Resolve(
            original,
            (ModelModKey, modelEdit),
            (BoundsModKey, boundsEdit));

        Assert.Equal("Meshes/Replacer.nif", GetModelPath(result));
        Assert.Equal(Bounds(9).Second, result.Bounds!.Second);
    }

    [Fact]
    public void ModelMetadataChangeDoesNotClaimBounds()
    {
        var original = CreateStatic("Meshes/Original.nif", 1, [1]);
        var boundsEdit = CreateStatic("Meshes/Original.nif", 7, [1]);
        var metadataEdit = CreateStatic("Meshes/Original.nif", 1, [2]);

        var result = Resolve(
            original,
            (BoundsModKey, boundsEdit),
            (ModelModKey, metadataEdit));

        Assert.Equal(new byte[] { 2 }, Assert.IsAssignableFrom<IModelGetter>(result.Model).Data!.Value.ToArray());
        Assert.Equal(Bounds(7).Second, result.Bounds!.Second);
    }

    private static ModelBoundsValue Resolve(
        Mutagen.Bethesda.Skyrim.Static original,
        params (ModKey ModKey, Mutagen.Bethesda.Skyrim.Static Record)[] overrides)
    {
        var handler = new ModelBoundsHandler();
        var context = handler.CreatePropertyContext();
        var originalContext = CreateContext(OriginalModKey, original);
        var winning = overrides.Length == 0
            ? originalContext
            : CreateContext(overrides[^1].ModKey, overrides[^1].Record);
        using var state = CreateState(
            CreateMod(OriginalModKey),
            overrides.Select(item => CreateMod(item.ModKey)).ToArray());

        handler.InitializeContext(originalContext, winning, context);
        foreach (var item in overrides)
        {
            handler.UpdatePropertyContext(CreateContext(item.ModKey, item.Record), state, context);
        }

        return Assert.IsType<ModelBoundsValue>(context.GetForwardValue());
    }

    private static Mutagen.Bethesda.Skyrim.Static CreateStatic(
        string modelPath,
        short boundsSize,
        byte[]? modelData = null) =>
        new(new FormKey(OriginalModKey, 0x1234), SkyrimRelease.SkyrimSE)
        {
            Model = new Model
            {
                File = new AssetLink<SkyrimModelAssetType>(modelPath),
                Data = modelData == null ? (MemorySlice<byte>?)null : new MemorySlice<byte>(modelData)
            },
            ObjectBounds = Bounds(boundsSize)
        };

    private static ObjectBounds Bounds(short size) => new()
    {
        First = new P3Int16((short)-size, (short)-size, (short)-size),
        Second = new P3Int16(size, size, size)
    };

    private static string GetModelPath(ModelBoundsValue value) =>
        Assert.IsAssignableFrom<IModelGetter>(value.Model).File.GivenPath;

    private static IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> CreateContext(
        ModKey modKey,
        IMajorRecord record) =>
        new ModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>(
            modKey,
            record,
            (_, _) => throw new NotSupportedException(),
            (_, _, _, _) => throw new NotSupportedException());

    private static SkyrimMod CreateMod(ModKey modKey) => new(modKey, SkyrimRelease.SkyrimSE);

#pragma warning disable CS0618
    private static SynthesisState<ISkyrimMod, ISkyrimModGetter> CreateState(
        SkyrimMod original,
        params SkyrimMod[] overrides)
    {
        var patchMod = CreateMod(PatchModKey);
        var listings = new[] { original }
            .Concat(overrides)
            .Append(patchMod)
            .Cast<ISkyrimModGetter>()
            .Select(mod => new ModListing<ISkyrimModGetter>(mod))
            .ToArray();
        var loadOrder = new LoadOrder<IModListing<ISkyrimModGetter>>(listings);
        var arguments = new RunSynthesisMutagenPatcher
        {
            OutputPath = @"C:\Temp\ModelBoundsPatch.esp",
            DataFolderPath = @"C:\Temp\Data",
            LoadOrderFilePath = @"C:\Temp\plugins.txt",
            GameRelease = GameRelease.SkyrimSE
        };

        return new SynthesisState<ISkyrimMod, ISkyrimModGetter>(
            arguments,
            listings.Select(listing => new LoadOrderListing(listing.ModKey, listing.Enabled)).ToArray(),
            loadOrder,
            loadOrder.ToImmutableLinkCache<ISkyrimMod, ISkyrimModGetter>(),
            null!, patchMod, null, null, null, CancellationToken.None, null);
    }
#pragma warning restore CS0618
}

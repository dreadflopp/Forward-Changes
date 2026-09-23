using DreadsMashedPatch.PropertyHandlers.Armor;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Mutagen.Bethesda.Synthesis;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class ArmorRecordHandlerTests
{
    [Fact]
    public void RegistersEveryArmorPilotProperty()
    {
        var handlers = new ArmorRecordHandler().PropertyHandlers;

        Assert.Contains("Armature", handlers.Keys);
        Assert.Contains("BodyTemplate.FirstPersonFlags", handlers.Keys);
        Assert.Contains("BodyTemplate.Flags", handlers.Keys);
        Assert.Contains("BodyTemplate.ArmorType", handlers.Keys);
        Assert.DoesNotContain("BodyTemplate.ActsLike44", handlers.Keys);
        Assert.Contains("RagdollConstraintTemplate", handlers.Keys);
        Assert.IsType<ModelBoundsHandler>(handlers["WorldModelAndBounds"]);
        Assert.DoesNotContain("WorldModel", handlers.Keys);
        Assert.DoesNotContain("ObjectBounds", handlers.Keys);
    }

    [Fact]
    public void UsesOneCompositeRecordHeaderFlagPath()
    {
        var recordHandler = new ArmorRecordHandler();
        var handlers = recordHandler.PropertyHandlers;

        Assert.Contains("MajorRecordFlagsRaw", handlers.Keys);
        Assert.DoesNotContain("MajorFlags", handlers.Keys);
        Assert.DoesNotContain("SkyrimMajorRecordFlags", handlers.Keys);

        var rawHandler = Assert.IsType<MajorRecordFlagsRawHandler>(handlers["MajorRecordFlagsRaw"]);
        Assert.False(rawHandler.AreValuesEqual(0, (int)Mutagen.Bethesda.Skyrim.Armor.MajorFlag.Shield));
        Assert.False(rawHandler.AreValuesEqual(0, (int)SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled));

        var armor = CreateArmor(0x100);
        var expected = (int)Mutagen.Bethesda.Skyrim.Armor.MajorFlag.Shield
            | (int)SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled
            | 0x4000;
        recordHandler.ApplyForwardedProperties(armor, new Dictionary<string, object?>
        {
            ["MajorRecordFlagsRaw"] = expected
        });

        Assert.Equal(expected, armor.MajorRecordFlagsRaw);
        Assert.True(armor.MajorFlags.HasFlag(Mutagen.Bethesda.Skyrim.Armor.MajorFlag.Shield));
        Assert.True(armor.SkyrimMajorRecordFlags.HasFlag(SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled));
    }

    [Fact]
    public void BodyTemplateHandlersCreateMissingAggregateAndSetSemanticLeaves()
    {
        var armor = CreateArmor(0x101);
        Assert.Null(armor.BodyTemplate);

        new SimpleReflectionFlagPropertyHandler<BipedObjectFlag, IArmor, IArmorGetter>("BodyTemplate.FirstPersonFlags")
            .SetValue(armor, BipedObjectFlag.Body | BipedObjectFlag.Hands);
        new SimpleReflectionFlagPropertyHandler<BodyTemplate.Flag, IArmor, IArmorGetter>("BodyTemplate.Flags")
            .SetValue(armor, BodyTemplate.Flag.ModulatesVoice | BodyTemplate.Flag.NonPlayable);
        new SimpleReflectionPropertyHandler<ArmorType, IArmor, IArmorGetter>("BodyTemplate.ArmorType")
            .SetValue(armor, ArmorType.HeavyArmor);

        Assert.NotNull(armor.BodyTemplate);
        Assert.Equal(BipedObjectFlag.Body | BipedObjectFlag.Hands, armor.BodyTemplate.FirstPersonFlags);
        Assert.Equal(BodyTemplate.Flag.ModulatesVoice | BodyTemplate.Flag.NonPlayable, armor.BodyTemplate.Flags);
        Assert.Equal(ArmorType.HeavyArmor, armor.BodyTemplate.ArmorType);
    }

    [Fact]
    public void FirstPersonFlagHandlerTracksAndForwardsUnnamedSlot44()
    {
        const uint slot44 = 1u << (44 - 30);
        var armor = CreateArmor(0x107);
        armor.BodyTemplate = new BodyTemplate
        {
            FirstPersonFlags = BipedObjectFlag.Head
        };
        var handler = Assert.IsType<SimpleReflectionFlagPropertyHandler<BipedObjectFlag, IArmor, IArmorGetter>>(
            new ArmorRecordHandler().PropertyHandlers["BodyTemplate.FirstPersonFlags"]);
        var requested = (BipedObjectFlag)((uint)BipedObjectFlag.Head | slot44);

        Assert.False(handler.AreValuesEqual(BipedObjectFlag.Head, requested));

        handler.SetValue(armor, requested);

        Assert.Equal((uint)requested, (uint)armor.BodyTemplate.FirstPersonFlags);
    }

    [Fact]
    public void ArmatureHandlerCopiesFormLinks()
    {
        var source = CreateArmor(0x102);
        var target = CreateArmor(0x103);
        var firstAddonKey = new FormKey(TestModKey, 0x200);
        var secondAddonKey = new FormKey(TestModKey, 0x201);
        source.Armature.Add(new FormLink<IArmorAddonGetter>(firstAddonKey));
        source.Armature.Add(new FormLink<IArmorAddonGetter>(secondAddonKey));
        var handler = Assert.IsType<AtomicFormLinkListPropertyHandler<IArmorAddonGetter, IArmor, IArmorGetter>>(
            new ArmorRecordHandler().PropertyHandlers["Armature"]);

        handler.SetValue(target, handler.GetValue(source));

        Assert.Equal([firstAddonKey, secondAddonKey], target.Armature.Select(link => link.FormKey));
        Assert.NotSame(source.Armature[0], target.Armature[0]);
        Assert.NotSame(source.Armature[1], target.Armature[1]);
    }

    [Fact]
    public void CompetingSingletonArmaturesUseTheLastDeclaredAtomicList()
    {
        var originalAddon = new FormKey(TestModKey, 0x210);
        var intermediateAddon = new FormKey(TestModKey, 0x211);
        var winningAddon = new FormKey(TestModKey, 0x212);
        var original = CreateArmorWithArmature(0x108, originalAddon);
        var intermediate = CreateArmorWithArmature(0x108, intermediateAddon);
        var winning = CreateArmorWithArmature(0x108, winningAddon);
        var handler = new ArmorRecordHandler().PropertyHandlers["Armature"];
        var propertyContext = handler.CreatePropertyContext();

        handler.InitializeContext(
            CreateContext(OriginalModKey, original),
            CreateContext(WinningModKey, winning),
            propertyContext);
        handler.UpdatePropertyContext(
            CreateContext(IntermediateModKey, intermediate),
            null!,
            propertyContext);
        handler.UpdatePropertyContext(
            CreateContext(WinningModKey, winning),
            null!,
            propertyContext);

        Assert.Equal([winningAddon], Assert.IsType<List<FormKey>>(propertyContext.GetForwardValue()));
    }

    [Fact]
    public void IntentionalMultiArmatureWinningListStaysIntact()
    {
        var originalAddon = new FormKey(TestModKey, 0x220);
        var bodyAddon = new FormKey(TestModKey, 0x221);
        var capeAddon = new FormKey(TestModKey, 0x222);
        var original = CreateArmorWithArmature(0x109, originalAddon);
        var winning = CreateArmorWithArmature(0x109, bodyAddon, capeAddon);
        var handler = new ArmorRecordHandler().PropertyHandlers["Armature"];
        var propertyContext = handler.CreatePropertyContext();

        handler.InitializeContext(
            CreateContext(OriginalModKey, original),
            CreateContext(WinningModKey, winning),
            propertyContext);
        handler.UpdatePropertyContext(
            CreateContext(WinningModKey, winning),
            null!,
            propertyContext);

        Assert.Equal([bodyAddon, capeAddon], Assert.IsType<List<FormKey>>(propertyContext.GetForwardValue()));
    }

    [Fact]
    public void WorldModelHandlerDeepCopiesModelsAndIcons()
    {
        var source = CreateArmor(0x104);
        var target = CreateArmor(0x105);
        source.WorldModel = new GenderedItem<ArmorModel?>(
            CreateArmorModel("Meshes\\Armor\\male.nif", "Textures\\Armor\\male.dds"),
            CreateArmorModel("Meshes\\Armor\\female.nif", "Textures\\Armor\\female.dds"));
        var handler = new WorldModelHandler();

        handler.SetValue(target, handler.GetValue(source));

        Assert.True(handler.AreValuesEqual(source.WorldModel, target.WorldModel));
        Assert.NotSame(source.WorldModel, target.WorldModel);
        Assert.NotSame(source.WorldModel!.Male, target.WorldModel!.Male);
        Assert.NotSame(source.WorldModel.Male!.Model, target.WorldModel.Male!.Model);
        Assert.NotSame(source.WorldModel.Male.Icons, target.WorldModel.Male.Icons);
    }

    [Fact]
    public void WorldModelHandlerFormatsModelsAndIcons()
    {
        var armor = CreateArmor(0x107);
        armor.WorldModel = new GenderedItem<ArmorModel?>(
            CreateArmorModel("Meshes\\Armor\\male.nif", "Textures\\Armor\\male.dds"),
            null);
        var handler = new WorldModelHandler();

        var formatted = handler.FormatValue(handler.GetValue(armor));

        Assert.Contains("Male: Model(File:", formatted);
        Assert.Contains("Armor\\male.nif", formatted);
        Assert.Contains("Armor\\male.dds", formatted);
        Assert.Contains("Female: null", formatted);
        Assert.DoesNotContain("GenderedItem`1", formatted);
    }

    [Fact]
    public void RagdollConstraintTemplateUsesScalarHandler()
    {
        var armor = CreateArmor(0x106);
        var handler = new SimpleReflectionPropertyHandler<string?, IArmor, IArmorGetter>("RagdollConstraintTemplate");

        handler.SetValue(armor, "Actors\\Character\\Ragdoll.hkx");

        Assert.Equal("Actors\\Character\\Ragdoll.hkx", handler.GetValue(armor));
    }

    private static readonly ModKey TestModKey = new("ArmorCoverageTests", ModType.Plugin);
    private static readonly ModKey OriginalModKey = new("Skyrim.esm", ModType.Master);
    private static readonly ModKey IntermediateModKey = new("Intermediate.esp", ModType.Plugin);
    private static readonly ModKey WinningModKey = new("Winning.esp", ModType.Plugin);

    private static Mutagen.Bethesda.Skyrim.Armor CreateArmor(uint id)
    {
        return new Mutagen.Bethesda.Skyrim.Armor(new FormKey(TestModKey, id), SkyrimRelease.SkyrimSE);
    }

    private static Mutagen.Bethesda.Skyrim.Armor CreateArmorWithArmature(
        uint id,
        params FormKey[] addonKeys)
    {
        var armor = CreateArmor(id);
        foreach (var addonKey in addonKeys)
        {
            armor.Armature.Add(new FormLink<IArmorAddonGetter>(addonKey));
        }

        return armor;
    }

    private static IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> CreateContext(
        ModKey modKey,
        IMajorRecord record)
        => new ModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>(
            modKey,
            record,
            (_, _) => throw new NotSupportedException(),
            (_, _, _, _) => throw new NotSupportedException());

    private static ArmorModel CreateArmorModel(string modelPath, string iconPath)
    {
        return new ArmorModel
        {
            Model = new Model
            {
                File = new AssetLink<SkyrimModelAssetType>(modelPath),
                Data = new byte[] { 1, 2, 3 }
            },
            Icons = new Icons
            {
                LargeIconFilename = new AssetLink<SkyrimTextureAssetType>(iconPath)
            }
        };
    }
}

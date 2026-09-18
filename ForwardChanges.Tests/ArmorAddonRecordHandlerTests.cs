using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Noggog;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class ArmorAddonRecordHandlerTests
{
    [Fact]
    public void RegistersWorldAndFirstPersonModelsByFieldAndGender()
    {
        var handlers = new ArmorAddonRecordHandler().PropertyHandlers;

        Assert.IsType<GenderedModelFileHandler<IArmorAddon, IArmorAddonGetter>>(handlers["FirstPersonModel.Male.File"]);
        Assert.IsType<GenderedModelAlternateTexturesHandler<IArmorAddon, IArmorAddonGetter>>(handlers["FirstPersonModel.Male.AlternateTextures"]);
        Assert.IsType<GenderedModelFileHandler<IArmorAddon, IArmorAddonGetter>>(handlers["FirstPersonModel.Female.File"]);
        Assert.IsType<GenderedModelAlternateTexturesHandler<IArmorAddon, IArmorAddonGetter>>(handlers["FirstPersonModel.Female.AlternateTextures"]);
        Assert.IsType<GenderedModelFileHandler<IArmorAddon, IArmorAddonGetter>>(handlers["WorldModel.Male.File"]);
        Assert.IsType<GenderedModelAlternateTexturesHandler<IArmorAddon, IArmorAddonGetter>>(handlers["WorldModel.Male.AlternateTextures"]);
        Assert.IsType<GenderedModelFileHandler<IArmorAddon, IArmorAddonGetter>>(handlers["WorldModel.Female.File"]);
        Assert.IsType<GenderedModelAlternateTexturesHandler<IArmorAddon, IArmorAddonGetter>>(handlers["WorldModel.Female.AlternateTextures"]);
        Assert.DoesNotContain("FirstPersonModel", handlers.Keys);
        Assert.DoesNotContain("WorldModel", handlers.Keys);
        Assert.DoesNotContain("FirstPersonModel.Male", handlers.Keys);
        Assert.DoesNotContain("FirstPersonModel.Female", handlers.Keys);
        Assert.DoesNotContain(handlers.Keys, key => key.EndsWith(".Data", StringComparison.Ordinal));
    }

    [Fact]
    public void FirstPersonModelHandlersSetOneGenderAndPreserveTheOther()
    {
        var armorAddon = CreateArmorAddon(0x2FF);
        armorAddon.FirstPersonModel = new GenderedItem<Model?>(
            CreateModel("Armor\\MaleOriginal.nif", [1], 1),
            CreateModel("Armor\\FemaleOriginal.nif", [2], 2));
        var handlers = new ArmorAddonRecordHandler().PropertyHandlers;

        var source = CreateArmorAddon(0x2FE);
        var forwardedFemale = CreateModel("Armor\\FemaleForwarded.nif", [3], 3);
        source.FirstPersonModel = new GenderedItem<Model?>(null, forwardedFemale);
        handlers["FirstPersonModel.Female.File"].SetValue(
            armorAddon,
            handlers["FirstPersonModel.Female.File"].GetValue(source));
        handlers["FirstPersonModel.Female.AlternateTextures"].SetValue(
            armorAddon,
            handlers["FirstPersonModel.Female.AlternateTextures"].GetValue(source));

        Assert.Equal("Meshes\\Armor\\MaleOriginal.nif", armorAddon.FirstPersonModel!.Male!.File.DataRelativePath.ToString());
        Assert.Equal("Meshes\\Armor\\FemaleForwarded.nif", armorAddon.FirstPersonModel.Female!.File.DataRelativePath.ToString());
        Assert.Equal(1, armorAddon.FirstPersonModel.Male.AlternateTextures![0].Index);
        Assert.Equal(3, armorAddon.FirstPersonModel.Female.AlternateTextures![0].Index);
        Assert.Equal(new byte[] { 3 }, armorAddon.FirstPersonModel.Female.Data!.Value.ToArray());
        Assert.NotSame(forwardedFemale, armorAddon.FirstPersonModel.Female);
        Assert.NotSame(forwardedFemale.AlternateTextures![0], armorAddon.FirstPersonModel.Female.AlternateTextures![0]);
    }

    [Fact]
    public void FirstPersonModelFileComparisonPreservesSerializedPrefixDifferences()
    {
        var handler = CreateFirstPersonFileHandler(MaleFemaleGender.Female);

        Assert.False(handler.AreValuesEqual(
            new ModelFileValue("Armor\\Female.nif", [1, 2]),
            new ModelFileValue("Meshes\\ARMOR\\FEMALE.NIF", [9, 9])));
    }

    [Fact]
    public void FirstPersonModelAlternateTextureComparisonUsesCompleteEntries()
    {
        var handler = CreateFirstPersonAlternateTexturesHandler(MaleFemaleGender.Female);
        var baseline = CreateModel("Armor\\Female.nif", [1, 2], 7).AlternateTextures;
        var differentIndex = CreateModel("Armor\\Female.nif", [1, 2], 8).AlternateTextures;
        var differentName = CreateModel("Armor\\Female.nif", [1, 2], 7).AlternateTextures;
        differentName![0].Name = "Hands";
        var differentTexture = CreateModel("Armor\\Female.nif", [1, 2], 7).AlternateTextures;
        differentTexture![0].NewTexture = new FormLink<ITextureSetGetter>(new FormKey(TestModKey, 0x901));

        Assert.False(handler.AreValuesEqual(baseline, differentIndex));
        Assert.False(handler.AreValuesEqual(baseline, differentName));
        Assert.False(handler.AreValuesEqual(baseline, differentTexture));
        Assert.True(handler.AreValuesEqual(null, new ExtendedList<AlternateTexture>()));
    }

    [Fact]
    public void NullAlternateTexturesDoNotCreateAnAbsentGenderedModel()
    {
        var armorAddon = CreateArmorAddon(0x2EF);
        var handler = CreateFirstPersonAlternateTexturesHandler(MaleFemaleGender.Female);

        handler.SetValue(armorAddon, null);

        Assert.Null(armorAddon.FirstPersonModel);
    }

    [Fact]
    public void RemovingTheLastGenderedModelClearsTheParentGroup()
    {
        var armorAddon = CreateArmorAddon(0x2EE);
        armorAddon.FirstPersonModel = new GenderedItem<Model?>(
            null,
            CreateModel("Armor\\Female.nif", [1], 4));
        var fileHandler = CreateFirstPersonFileHandler(MaleFemaleGender.Female);
        var textureHandler = CreateFirstPersonAlternateTexturesHandler(MaleFemaleGender.Female);

        fileHandler.SetValue(armorAddon, null);
        textureHandler.SetValue(armorAddon, null);

        Assert.Null(armorAddon.FirstPersonModel);
    }

    [Fact]
    public void AlternateTextureForwardingPreservesWinningFileAndData()
    {
        var armorAddon = CreateArmorAddon(0x2FD);
        armorAddon.FirstPersonModel = new GenderedItem<Model?>(
            null,
            CreateModel("Armor\\WinningFemale.nif", [4, 5, 6], alternateTextureIndex: null));
        var source = CreateArmorAddon(0x2FC);
        source.FirstPersonModel = new GenderedItem<Model?>(
            null,
            CreateModel("Armor\\IgnoredSourceFile.nif", [9, 9, 9], alternateTextureIndex: 12));
        var handler = CreateFirstPersonAlternateTexturesHandler(MaleFemaleGender.Female);

        handler.SetValue(armorAddon, handler.GetValue(source));

        Assert.Equal("Meshes\\Armor\\WinningFemale.nif", armorAddon.FirstPersonModel!.Female!.File.DataRelativePath.ToString());
        Assert.Equal(new byte[] { 4, 5, 6 }, armorAddon.FirstPersonModel.Female.Data!.Value.ToArray());
        Assert.Equal(12, armorAddon.FirstPersonModel.Female.AlternateTextures![0].Index);
    }

    [Fact]
    public void WorldModelUsesTheSameFileAndAlternateTexturePolicy()
    {
        var armorAddon = CreateArmorAddon(0x2FB);
        armorAddon.WorldModel = new GenderedItem<Model?>(
            CreateModel("Armor\\WinningMaleWorld.nif", [4], alternateTextureIndex: null),
            CreateModel("Armor\\FemaleWorld.nif", [5], alternateTextureIndex: 5));
        var source = CreateArmorAddon(0x2FA);
        source.WorldModel = new GenderedItem<Model?>(
            CreateModel("Armor\\IgnoredMaleWorld.nif", [9], alternateTextureIndex: 14),
            null);
        var handlers = new ArmorAddonRecordHandler().PropertyHandlers;

        handlers["WorldModel.Male.AlternateTextures"].SetValue(
            armorAddon,
            handlers["WorldModel.Male.AlternateTextures"].GetValue(source));

        Assert.Equal("Meshes\\Armor\\WinningMaleWorld.nif", armorAddon.WorldModel!.Male!.File.DataRelativePath.ToString());
        Assert.Equal(new byte[] { 4 }, armorAddon.WorldModel.Male.Data!.Value.ToArray());
        Assert.Equal(14, armorAddon.WorldModel.Male.AlternateTextures![0].Index);
        Assert.Equal("Meshes\\Armor\\FemaleWorld.nif", armorAddon.WorldModel.Female!.File.DataRelativePath.ToString());
    }

    [Fact]
    public void RegistersAndAppliesSimpleGenderedPropertiesIndependently()
    {
        var handlers = new ArmorAddonRecordHandler().PropertyHandlers;
        var armorAddon = CreateArmorAddon(0x2F9);
        armorAddon.Priority = new GenderedItem<byte>(1, 2);
        var maleSkin = new FormKey(TestModKey, 0x910);
        var femaleSkin = new FormKey(TestModKey, 0x911);
        armorAddon.SkinTexture = new GenderedItem<IFormLinkNullableGetter<ITextureSetGetter>>(
            new FormLinkNullable<ITextureSetGetter>(maleSkin),
            new FormLinkNullable<ITextureSetGetter>());

        handlers["Priority.Female"].SetValue(armorAddon, (byte)7);
        handlers["SkinTexture.Female"].SetValue(armorAddon, new FormLinkNullable<ITextureSetGetter>(femaleSkin));

        Assert.Equal((byte)1, armorAddon.Priority.Male);
        Assert.Equal((byte)7, armorAddon.Priority.Female);
        Assert.Equal(maleSkin, armorAddon.SkinTexture!.Male.FormKey);
        Assert.Equal(femaleSkin, armorAddon.SkinTexture.Female.FormKey);
        Assert.Contains("WeightSliderEnabled.Male", handlers.Keys);
        Assert.Contains("WeightSliderEnabled.Female", handlers.Keys);
        Assert.Contains("TextureSwapList.Male", handlers.Keys);
        Assert.Contains("TextureSwapList.Female", handlers.Keys);
        Assert.DoesNotContain("Priority", handlers.Keys);
        Assert.DoesNotContain("SkinTexture", handlers.Keys);
    }

    [Fact]
    public void RegistersExactBodyTemplateLeavesOnly()
    {
        var handlers = new ArmorAddonRecordHandler().PropertyHandlers;

        Assert.Contains("BodyTemplate.FirstPersonFlags", handlers.Keys);
        Assert.Contains("BodyTemplate.Flags", handlers.Keys);
        Assert.Contains("BodyTemplate.ArmorType", handlers.Keys);
        Assert.DoesNotContain("BodyTemplate.ActsLike44", handlers.Keys);

        Assert.DoesNotContain("BodyTemplateFirstPersonFlags", handlers.Keys);
        Assert.DoesNotContain("BodyTemplateModulatesVoice", handlers.Keys);
        Assert.DoesNotContain("BodyTemplateNonPlayable", handlers.Keys);
        Assert.DoesNotContain("BodyTemplateArmorType", handlers.Keys);
    }

    [Fact]
    public void BodyTemplateHandlersCreateAggregateAndSetSemanticLeaves()
    {
        var armorAddon = CreateArmorAddon(0x300);
        var handlers = new ArmorAddonRecordHandler().PropertyHandlers;
        Assert.Null(armorAddon.BodyTemplate);

        handlers["BodyTemplate.FirstPersonFlags"].SetValue(
            armorAddon,
            BipedObjectFlag.Body | BipedObjectFlag.Hands);
        handlers["BodyTemplate.ArmorType"].SetValue(armorAddon, ArmorType.LightArmor);

        Assert.NotNull(armorAddon.BodyTemplate);
        Assert.Equal(BipedObjectFlag.Body | BipedObjectFlag.Hands, armorAddon.BodyTemplate.FirstPersonFlags);
        Assert.Equal(ArmorType.LightArmor, armorAddon.BodyTemplate.ArmorType);
    }

    [Fact]
    public void BodyTemplateFlagHandlerPreservesUnknownBits()
    {
        const int unknownBit = 0x40;
        var armorAddon = CreateArmorAddon(0x301);
        armorAddon.BodyTemplate = new BodyTemplate
        {
            Flags = (BodyTemplate.Flag)unknownBit
        };
        var handler = Assert.IsType<SimpleReflectionFlagPropertyHandler<BodyTemplate.Flag, IArmorAddon, IArmorAddonGetter>>(
            new ArmorAddonRecordHandler().PropertyHandlers["BodyTemplate.Flags"]);

        handler.SetValue(armorAddon, BodyTemplate.Flag.NonPlayable);

        Assert.Equal(unknownBit | (int)BodyTemplate.Flag.NonPlayable, (int)armorAddon.BodyTemplate.Flags);
    }

    [Fact]
    public void FirstPersonFlagHandlerTracksAndForwardsUnnamedSlot44()
    {
        const uint slot44 = 1u << (44 - 30);
        var armorAddon = CreateArmorAddon(0x302);
        armorAddon.BodyTemplate = new BodyTemplate
        {
            FirstPersonFlags = BipedObjectFlag.Head
        };
        var handler = Assert.IsType<SimpleReflectionFlagPropertyHandler<BipedObjectFlag, IArmorAddon, IArmorAddonGetter>>(
            new ArmorAddonRecordHandler().PropertyHandlers["BodyTemplate.FirstPersonFlags"]);
        var requested = (BipedObjectFlag)((uint)BipedObjectFlag.Head | slot44);

        Assert.False(handler.AreValuesEqual(BipedObjectFlag.Head, requested));

        handler.SetValue(armorAddon, requested);

        Assert.Equal((uint)requested, (uint)armorAddon.BodyTemplate.FirstPersonFlags);
    }

    private static readonly ModKey TestModKey = new("ArmorAddonCoverageTests", ModType.Plugin);

    private static ArmorAddon CreateArmorAddon(uint id)
    {
        return new ArmorAddon(new FormKey(TestModKey, id), SkyrimRelease.SkyrimSE);
    }

    private static GenderedModelFileHandler<IArmorAddon, IArmorAddonGetter> CreateFirstPersonFileHandler(
        MaleFemaleGender gender)
        => new(
            "FirstPersonModel",
            gender,
            record => record.FirstPersonModel,
            (record, value) => record.FirstPersonModel = value);

    private static GenderedModelAlternateTexturesHandler<IArmorAddon, IArmorAddonGetter> CreateFirstPersonAlternateTexturesHandler(
        MaleFemaleGender gender)
        => new(
            "FirstPersonModel",
            gender,
            record => record.FirstPersonModel,
            (record, value) => record.FirstPersonModel = value);

    private static Model CreateModel(string path, byte[] data, int? alternateTextureIndex)
    {
        var model = new Model
        {
            File = new AssetLink<SkyrimModelAssetType>(path),
            Data = new MemorySlice<byte>(data)
        };

        if (alternateTextureIndex.HasValue)
        {
            model.AlternateTextures = new ExtendedList<AlternateTexture>
            {
                new()
                {
                    Name = "Body",
                    NewTexture = new FormLink<ITextureSetGetter>(new FormKey(TestModKey, 0x900)),
                    Index = alternateTextureIndex.Value
                }
            };
        }

        return model;
    }
}

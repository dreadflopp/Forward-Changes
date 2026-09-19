using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;
using StaticFlagsHandler = DreadsMashedPatch.PropertyHandlers.Static.FlagsHandler;
using StaticLodHandler = DreadsMashedPatch.PropertyHandlers.Static.LodHandler;
using StaticModelHandler = DreadsMashedPatch.PropertyHandlers.Static.ModelHandler;
using TextureSetFlagsHandler = DreadsMashedPatch.PropertyHandlers.TextureSet.FlagsHandler;

namespace DreadsMashedPatch.Tests;

public sealed class StaticAndTextureSetRecordHandlerTests
{
    [Fact]
    public void StaticUsesOneCompositeHeaderPathAndPreservesUnownedBitsWhenClearing()
    {
        var recordHandler = new StaticRecordHandler();
        var handlers = recordHandler.PropertyHandlers;

        Assert.IsType<MajorRecordFlagsRawHandler>(handlers["MajorRecordFlagsRaw"]);
        Assert.DoesNotContain("SkyrimMajorRecordFlags", handlers.Keys);
        Assert.DoesNotContain("MajorFlags", handlers.Keys);
        Assert.IsType<StaticFlagsHandler>(handlers["Flags"]);

        const int unownedBit = 0x01000000;
        var record = new Mutagen.Bethesda.Skyrim.Static(
            new FormKey(TestModKey, 0x500),
            SkyrimRelease.SkyrimSE)
        {
            MajorRecordFlagsRaw = unownedBit |
                (int)Mutagen.Bethesda.Skyrim.Static.MajorFlag.NeverFades |
                (int)SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled
        };

        recordHandler.ApplyForwardedProperties(record, new Dictionary<string, object?>
        {
            ["MajorRecordFlagsRaw"] = (int)SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled
        });

        Assert.Equal(
            unownedBit | (int)SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled,
            record.MajorRecordFlagsRaw);
        Assert.False(record.MajorFlags.HasFlag(Mutagen.Bethesda.Skyrim.Static.MajorFlag.NeverFades));
    }

    [Fact]
    public void StaticKeepsModelAndLodAsCohesiveAtomicProperties()
    {
        var handlers = new StaticRecordHandler().PropertyHandlers;

        Assert.IsType<StaticModelHandler>(handlers["Model"]);
        Assert.IsType<StaticLodHandler>(handlers["Lod"]);
        Assert.DoesNotContain("Model.File", handlers.Keys);
        Assert.DoesNotContain("Model.AlternateTextures", handlers.Keys);
        Assert.DoesNotContain("Lod.Level0", handlers.Keys);
        Assert.DoesNotContain("Lod.Level3", handlers.Keys);
    }

    [Fact]
    public void TextureSetUsesOneCompositeHeaderPathAndNullablePerBitDnamHandler()
    {
        var recordHandler = new TextureSetRecordHandler();
        var handlers = recordHandler.PropertyHandlers;

        Assert.IsType<MajorRecordFlagsRawHandler>(handlers["MajorRecordFlagsRaw"]);
        Assert.DoesNotContain("SkyrimMajorRecordFlags", handlers.Keys);
        var flagsHandler = Assert.IsType<TextureSetFlagsHandler>(handlers["Flags"]);
        Assert.True(flagsHandler.RequiresFullLoadOrderProcessing);
        Assert.False(flagsHandler.AreValuesEqual(null, (TextureSet.Flag)0));

        const int unownedBit = 0x01000000;
        var record = new TextureSet(
            new FormKey(TestModKey, 0x501),
            SkyrimRelease.SkyrimSE)
        {
            MajorRecordFlagsRaw = unownedBit |
                (int)SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled
        };

        recordHandler.ApplyForwardedProperties(record, new Dictionary<string, object?>
        {
            ["MajorRecordFlagsRaw"] = 0
        });

        Assert.Equal(unownedBit, record.MajorRecordFlagsRaw);
    }

    [Fact]
    public void TextureSetDnamHandlerPreservesUnknownBitsAndNullablePresence()
    {
        const TextureSet.Flag unknownBit = (TextureSet.Flag)0x8000;
        var handler = new TextureSetFlagsHandler();
        var record = new TextureSet(
            new FormKey(TestModKey, 0x502),
            SkyrimRelease.SkyrimSE)
        {
            Flags = unknownBit | TextureSet.Flag.NoSpecularMap
        };

        handler.SetValue(record, TextureSet.Flag.FaceGenTextures);

        Assert.Equal(unknownBit | TextureSet.Flag.FaceGenTextures, record.Flags);

        handler.SetValue(record, null);

        Assert.Null(record.Flags);
    }

    [Fact]
    public void TextureSetKeepsTheCompleteDecalPayloadAtomic()
    {
        var handler = new TextureSetRecordHandler().PropertyHandlers["Decal"];
        Assert.IsType<GeneratedCopyReflectionPropertyHandler<IDecalGetter, Decal, ITextureSet, ITextureSetGetter>>(handler);
        var source = new Decal
        {
            MinWidth = 1,
            MaxWidth = 2,
            MinHeight = 3,
            MaxHeight = 4,
            Depth = 5,
            Shininess = 6,
            ParallaxScale = 7,
            ParallaxPasses = 8,
            Flags = Decal.Flag.Parallax | Decal.Flag.AlphaTesting,
            Unknown = 9
        };
        var target = new TextureSet(
            new FormKey(TestModKey, 0x503),
            SkyrimRelease.SkyrimSE);

        handler.SetValue(target, source);

        Assert.NotNull(target.Decal);
        Assert.NotSame(source, target.Decal);
        Assert.Equal(source.MinWidth, target.Decal.MinWidth);
        Assert.Equal(source.MaxHeight, target.Decal.MaxHeight);
        Assert.Equal(source.ParallaxPasses, target.Decal.ParallaxPasses);
        Assert.Equal(source.Flags, target.Decal.Flags);
        Assert.Equal(source.Unknown, target.Decal.Unknown);
        Assert.DoesNotContain("Decal.Flags", new TextureSetRecordHandler().PropertyHandlers.Keys);
    }

    private static readonly ModKey TestModKey = new("StaticTextureSetTests.esp", ModType.Plugin);
}

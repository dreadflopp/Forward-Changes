using DreadsMashedPatch.PropertyHandlers.LeveledNpc;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class LeveledNpcRecordHandlerTests
{
    private static readonly ModKey SourceModKey = ModKey.FromNameAndExtension("LeveledNpcSource.esp");
    private static readonly ModKey PatchModKey = ModKey.FromNameAndExtension("LeveledNpcPatch.esp");

    [Fact]
    public void UnorderedComparisonPreservesDuplicateMultiplicity()
    {
        var handler = new EntriesHandler();
        var entryA = Entry(new FormKey(SourceModKey, 0x100), 10);
        var entryB = Entry(new FormKey(SourceModKey, 0x101), 10);

        Assert.True(handler.AreValuesEqual(
            [entryA, entryA, entryB],
            [entryB, entryA, entryA]));
        Assert.False(handler.AreValuesEqual(
            [entryA, entryA, entryB],
            [entryA, entryB, entryB]));
    }

    [Fact]
    public void SetterWritesEveryDuplicateOccurrence()
    {
        var handler = new EntriesHandler();
        var patch = new SkyrimMod(PatchModKey, SkyrimRelease.SkyrimSE);
        var target = new LeveledNpc(new FormKey(PatchModKey, 0x800), SkyrimRelease.SkyrimSE);
        patch.LeveledNpcs.Add(target);
        var duplicate = Entry(new FormKey(SourceModKey, 0x100), 10);

        handler.SetValue(target, [duplicate, duplicate, Entry(new FormKey(SourceModKey, 0x101), 20)]);

        Assert.Equal(3, target.Entries!.Count);
        Assert.Equal(
            2,
            target.Entries.Count(entry => entry.Data!.Reference.FormKey.Equals(duplicate.Data!.Reference.FormKey)));

        using var stream = new MemoryStream();
        patch.WriteToBinary(stream);
        stream.Position = 0;
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(stream, SkyrimRelease.SkyrimSE, PatchModKey);
        var writtenEntries = Assert.Single(overlay.LeveledNpcs).Entries!;

        Assert.Equal(3, writtenEntries.Count);
        Assert.Equal(
            2,
            writtenEntries.Count(entry => entry.Data!.Reference.FormKey.Equals(duplicate.Data!.Reference.FormKey)));
    }

    private static ILeveledNpcEntryGetter Entry(FormKey reference, short level)
    {
        return new LeveledNpcEntry
        {
            Data = new LeveledNpcEntryData
            {
                Level = level,
                Count = 1,
                Reference = new FormLink<INpcSpawnGetter>(reference)
            }
        };
    }
}

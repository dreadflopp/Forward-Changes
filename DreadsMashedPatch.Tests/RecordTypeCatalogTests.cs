using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class RecordTypeCatalogTests
{
    [Fact]
    public void DisabledRecordFamiliesAreNotEnumeratedByTheContextLoader()
    {
        Type[] enabledTypes = [typeof(IWeaponGetter)];

        Assert.True(Program.IsRecordTypeQueryEnabled(typeof(IWeaponGetter), enabledTypes));
        Assert.False(Program.IsRecordTypeQueryEnabled(typeof(INpcGetter), enabledTypes));
    }

    [Fact]
    public void SharedBaseQueryLoadsWhenOneDerivedRecordFamilyIsEnabled()
    {
        Type[] enabledTypes = [typeof(IGlobalIntGetter)];

        Assert.True(Program.IsRecordTypeQueryEnabled(typeof(IGlobalGetter), enabledTypes));
    }

    [Fact]
    public void QuestUsesTheXEditQuestSignature()
    {
        Assert.Equal("QUST", RecordTypeCatalog.GetSignature(typeof(IQuestGetter)));
    }

    [Fact]
    public void RecordDescriptionIncludesSignatureAndFriendlyType()
    {
        var quest = new Quest(
            new Mutagen.Bethesda.Plugins.FormKey(
                Mutagen.Bethesda.Plugins.ModKey.FromNameAndExtension("TypeTest.esp"),
                0x800),
            SkyrimRelease.SkyrimSE);

        Assert.Equal("QUST - Quest", RecordTypeCatalog.GetRecordDescription(quest));
    }

    [Fact]
    public void EverySupportedRecordTypeHasAFourCharacterSignature()
    {
        foreach (var recordType in Program.SupportedRecordTypes)
        {
            Assert.Equal(4, RecordTypeCatalog.GetSignature(recordType).Length);
        }
    }
}

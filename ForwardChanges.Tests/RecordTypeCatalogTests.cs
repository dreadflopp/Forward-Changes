using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class RecordTypeCatalogTests
{
    [Fact]
    public void QuestUsesTheXEditQuestSignature()
    {
        Assert.Equal("QUST", RecordTypeCatalog.GetSignature(typeof(IQuestGetter)));
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

using DreadsMashedPatch.PropertyHandlers.Quest;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class QuestAliasComparisonTests
{
    [Fact]
    public void LocationReferenceUsesSemanticFieldsAcrossDistinctInstances()
    {
        var first = CreateAlias();
        var second = CreateAlias();
        first.Location = new LocationAliasReference { AliasID = 7 };
        second.Location = new LocationAliasReference { AliasID = 7 };

        Assert.True(AreEqual(first, second));

        second.Location.AliasID = 8;
        Assert.False(AreEqual(first, second));
    }

    [Fact]
    public void ExternalReferenceUsesSemanticFieldsAcrossDistinctInstances()
    {
        var first = CreateAlias();
        var second = CreateAlias();
        first.External = new ExternalAliasReference { AliasID = 3 };
        second.External = new ExternalAliasReference { AliasID = 3 };

        Assert.True(AreEqual(first, second));

        second.External.AliasID = 4;
        Assert.False(AreEqual(first, second));
    }

    [Fact]
    public void NearAliasReferenceUsesSemanticFieldsAcrossDistinctInstances()
    {
        var first = CreateAlias();
        var second = CreateAlias();
        first.FindMatchingRefNearAlias = new FindMatchingRefNearAlias { AliasID = 11 };
        second.FindMatchingRefNearAlias = new FindMatchingRefNearAlias { AliasID = 11 };

        Assert.True(AreEqual(first, second));

        second.FindMatchingRefNearAlias.AliasID = 12;
        Assert.False(AreEqual(first, second));
    }

    [Fact]
    public void EventReferenceUsesSemanticFieldsAcrossDistinctInstances()
    {
        var first = CreateAlias();
        var second = CreateAlias();
        first.FindMatchingRefFromEvent = new FindMatchingRefFromEvent
        {
            EventData = GetEventDataConditionData.EventMember.CreatedObject
        };
        second.FindMatchingRefFromEvent = new FindMatchingRefFromEvent
        {
            EventData = GetEventDataConditionData.EventMember.CreatedObject
        };

        Assert.True(AreEqual(first, second));

        second.FindMatchingRefFromEvent.EventData = GetEventDataConditionData.EventMember.NewLocation;
        Assert.False(AreEqual(first, second));
    }

    private static QuestAlias CreateAlias() => new() { ID = 1 };

    private static bool AreEqual(QuestAlias first, QuestAlias second) =>
        new AliasesHandler().AreValuesEqual(
            new List<IQuestAliasGetter> { first },
            new List<IQuestAliasGetter> { second });
}

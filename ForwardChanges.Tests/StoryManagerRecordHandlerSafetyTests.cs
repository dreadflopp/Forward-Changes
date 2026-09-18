using ForwardChanges.Enums;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class StoryManagerRecordHandlerSafetyTests
{
    [Fact]
    public void AtomicConfigurationForwardingIsTheDefaultPolicy()
    {
        Assert.Equal(
            StoryManagerForwardingPolicy.AtomicOnConfigurationChange,
            PatcherSettings.StoryManagerPolicy);

        Assert.Equal(
            new HashSet<string>(StringComparer.Ordinal)
            {
                "Parent",
                "PreviousSibling",
                "Conditions",
                "Flags",
                "MaxConcurrentQuests"
            },
            new TestableBranchHandler().AtomicTriggers);

        Assert.Equal(
            new HashSet<string>(StringComparer.Ordinal)
            {
                "Parent",
                "PreviousSibling",
                "Conditions",
                "Flags",
                "MaxConcurrentQuests",
                "Type"
            },
            new TestableEventHandler().AtomicTriggers);

        Assert.Equal(
            new HashSet<string>(StringComparer.Ordinal)
            {
                "Parent",
                "PreviousSibling",
                "Conditions",
                "Flags",
                "QuestFlags",
                "MaxConcurrentQuests",
                "MaxNumQuestsToRun"
            },
            new TestableQuestNodeHandler().AtomicTriggers);
    }

    [Fact]
    public void StandardForwardingDisablesStoryManagerOwnershipBoundaries()
    {
        Assert.Empty(new TestableBranchHandler(
            StoryManagerForwardingPolicy.StandardForwarding).AtomicTriggers);
        Assert.Empty(new TestableEventHandler(
            StoryManagerForwardingPolicy.StandardForwarding).AtomicTriggers);
        Assert.Empty(new TestableQuestNodeHandler(
            StoryManagerForwardingPolicy.StandardForwarding).AtomicTriggers);
    }

    [Fact]
    public void ParentAndPreviousSiblingAreObservedAsOneAtomicTopology()
    {
        var handler = new TestableBranchHandler();
        var previous = CreateBranchNode();
        var reparented = CreateBranchNode();
        reparented.Parent.SetTo(new FormKey(ReferencedModKey, 0x200));
        var reordered = CreateBranchNode();
        reordered.PreviousSibling.SetTo(new FormKey(ReferencedModKey, 0x201));

        Assert.Equal(["Parent"], handler.GetChangedTriggers(previous, reparented));
        Assert.Equal(["PreviousSibling"], handler.GetChangedTriggers(previous, reordered));
        Assert.Contains("PreviousSibling", handler.PropertyHandlers.Keys);
    }

    [Fact]
    public void ConditionsAndEventTypeEstablishAtomicOwnership()
    {
        var branchHandler = new TestableBranchHandler();
        var previousBranch = CreateBranchNode();
        var conditionedBranch = CreateBranchNode();
        conditionedBranch.Conditions.Add(CreateCondition());

        var eventHandler = new TestableEventHandler();
        var previousEvent = CreateEventNode();
        var changedEvent = CreateEventNode();
        changedEvent.Type = StoryManagerEventNode.Types.ActorHelloEvent;

        Assert.Equal(
            ["Conditions"],
            branchHandler.GetChangedTriggers(previousBranch, conditionedBranch));
        Assert.Equal(
            ["Type"],
            eventHandler.GetChangedTriggers(previousEvent, changedEvent));
    }

    [Fact]
    public void QuestRowsRemainMergeableButEverySelectionControlIsAtomic()
    {
        var handler = new TestableQuestNodeHandler();

        Assert.DoesNotContain("Quests", handler.AtomicTriggers);
        Assert.Contains("Flags", handler.AtomicTriggers);
        Assert.Contains("QuestFlags", handler.AtomicTriggers);
        Assert.Contains("MaxConcurrentQuests", handler.AtomicTriggers);
        Assert.Contains("MaxNumQuestsToRun", handler.AtomicTriggers);

        var previous = CreateQuestNode();
        var withQuest = CreateQuestNode();
        withQuest.Quests.Add(CreateStoryManagerQuest(0x300));

        Assert.Empty(handler.GetChangedTriggers(previous, withQuest));
    }

    [Fact]
    public void ConfigurationChangeResetsQuestRowsToThatOverrideSnapshot()
    {
        var handler = new TestableQuestNodeHandler();
        var original = CreateQuestNode();
        original.Quests.Add(CreateStoryManagerQuest(0x300));
        var configured = CreateQuestNode();
        configured.QuestFlags = StoryManagerQuestNode.QuestFlag.NumQuestsToRun;
        configured.MaxNumQuestsToRun = 1;
        configured.Quests.Add(CreateStoryManagerQuest(0x301));

        var originalContext = CreateContext(OriginalModKey, original);
        var configuredContext = CreateContext(OverrideModKey, configured);
        handler.InitializeForTest(originalContext);

        var changed = handler.ResetForTest(originalContext, configuredContext);

        Assert.Contains("QuestFlags", changed);
        Assert.Contains("MaxNumQuestsToRun", changed);
        Assert.True(handler.PropertyHandlers["Quests"].AreValuesEqual(
            handler.PropertyHandlers["Quests"].GetValue(configured),
            handler.GetForwardValue("Quests")));
    }

    [Fact]
    public void OptionalStoryManagerFlagsPreservePresenceAndUnknownBits()
    {
        var branchHandler = new StoryManagerBranchNodeRecordHandler();
        var flagsHandler = Assert.IsType<SimpleReflectionNullableFlagPropertyHandler<
            AStoryManagerNode.Flag,
            IStoryManagerBranchNode,
            IStoryManagerBranchNodeGetter>>(branchHandler.PropertyHandlers["Flags"]);
        var branch = CreateBranchNode();

        Assert.Null(flagsHandler.GetValue(branch));

        branch.Flags = (AStoryManagerNode.Flag)0x4000;
        flagsHandler.SetValue(branch, AStoryManagerNode.Flag.Random);
        Assert.Equal(0x4001, (int)branch.Flags!.Value);

        flagsHandler.SetValue(branch, null);
        Assert.Null(branch.Flags);
    }

    private static StoryManagerBranchNode CreateBranchNode() =>
        new(new FormKey(OriginalModKey, 0x100), SkyrimRelease.SkyrimSE);

    private static StoryManagerEventNode CreateEventNode() =>
        new(new FormKey(OriginalModKey, 0x101), SkyrimRelease.SkyrimSE);

    private static StoryManagerQuestNode CreateQuestNode() =>
        new(new FormKey(OriginalModKey, 0x102), SkyrimRelease.SkyrimSE);

    private static StoryManagerQuest CreateStoryManagerQuest(uint id)
    {
        var quest = new StoryManagerQuest();
        quest.Quest.SetTo(new FormKey(ReferencedModKey, id));
        return quest;
    }

    private static ConditionFloat CreateCondition() => new()
    {
        ComparisonValue = 1,
        Data = new GetRandomPercentConditionData()
    };

    private static IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> CreateContext(
        ModKey modKey,
        IMajorRecord record) =>
        new ModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>(
            modKey,
            record,
            (_, _) => throw new NotSupportedException(),
            (_, _, _, _) => throw new NotSupportedException());

    private sealed class TestableBranchHandler(
        StoryManagerForwardingPolicy? policy = null)
        : StoryManagerBranchNodeRecordHandler(policy)
    {
        public IReadOnlySet<string> AtomicTriggers => AtomicOwnershipTriggerProperties;

        public IReadOnlyList<string> GetChangedTriggers(
            IStoryManagerBranchNodeGetter previous,
            IStoryManagerBranchNodeGetter current) =>
            GetChangedAtomicOwnershipTriggerProperties(previous, current);
    }

    private sealed class TestableEventHandler(
        StoryManagerForwardingPolicy? policy = null)
        : StoryManagerEventNodeRecordHandler(policy)
    {
        public IReadOnlySet<string> AtomicTriggers => AtomicOwnershipTriggerProperties;

        public IReadOnlyList<string> GetChangedTriggers(
            IStoryManagerEventNodeGetter previous,
            IStoryManagerEventNodeGetter current) =>
            GetChangedAtomicOwnershipTriggerProperties(previous, current);
    }

    private sealed class TestableQuestNodeHandler(
        StoryManagerForwardingPolicy? policy = null)
        : StoryManagerQuestNodeRecordHandler(policy)
    {
        public IReadOnlySet<string> AtomicTriggers => AtomicOwnershipTriggerProperties;

        public IReadOnlyList<string> GetChangedTriggers(
            IStoryManagerQuestNodeGetter previous,
            IStoryManagerQuestNodeGetter current) =>
            GetChangedAtomicOwnershipTriggerProperties(previous, current);

        public void InitializeForTest(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context) =>
            InitializePropertyContexts(context, context);

        public IReadOnlyList<string> ResetForTest(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> previous,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> current) =>
            ResetPropertyContextsIfAtomicOwnershipTriggered(previous, current);

        public object? GetForwardValue(string propertyName) =>
            PropertyContexts[propertyName].GetForwardValue();
    }

    private static readonly ModKey OriginalModKey = new("Skyrim.esm", ModType.Master);
    private static readonly ModKey OverrideModKey = new("StoryManagerOverhaul.esp", ModType.Plugin);
    private static readonly ModKey ReferencedModKey = new("Referenced.esp", ModType.Plugin);
}

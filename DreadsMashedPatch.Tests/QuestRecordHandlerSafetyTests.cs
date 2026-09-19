using ForwardChanges.Enums;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Quest;
using ForwardChanges.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Strings;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class QuestRecordHandlerSafetyTests
{
    private static readonly ModKey TestModKey = new("QuestSafety.esp", ModType.Plugin);

    [Fact]
    public void AtomicStructuralForwardingIsTheDefaultPolicy()
    {
        var handler = new TestableQuestRecordHandler();

        Assert.Equal(QuestForwardingPolicy.AtomicOnStructuralChange, PatcherSettings.QuestPolicy);
        Assert.Equal(QuestForwardingPolicy.AtomicOnStructuralChange, handler.ForwardingPolicy);
        Assert.Equal(
            new HashSet<string>(StringComparer.Ordinal)
            {
                "VirtualMachineAdapter.Presence",
                "VirtualMachineAdapter.Version",
                "VirtualMachineAdapter.ObjectFormat",
                "VirtualMachineAdapter.Scripts",
                "VirtualMachineAdapter.ExtraBindDataVersion",
                "VirtualMachineAdapter.FileName",
                "VirtualMachineAdapter.Fragments",
                "VirtualMachineAdapter.Aliases",
                "Type",
                "Event",
                "TextDisplayGlobals",
                "DialogConditions",
                "EventConditions",
                "Stages",
                "Objectives",
                "NextAliasID",
                "Aliases"
            },
            handler.AtomicTriggers);
    }

    [Fact]
    public void StandardForwardingDisablesStructuralOwnershipBoundary()
    {
        var handler = new TestableQuestRecordHandler(QuestForwardingPolicy.StandardForwarding);
        var previous = CreateQuest(0x800);
        var current = CreateQuest(0x800);
        current.Type = Quest.TypeEnum.SideQuest;

        Assert.Empty(handler.AtomicTriggers);
        Assert.Empty(handler.GetChangedTriggers(previous, current));
    }

    [Fact]
    public void StructuralChangeTriggersButDescriptiveChangeDoesNot()
    {
        var handler = new TestableQuestRecordHandler();
        var previous = CreateQuest(0x801);
        var structural = CreateQuest(0x801);
        structural.Type = Quest.TypeEnum.SideQuest;
        var descriptive = CreateQuest(0x801);
        descriptive.EditorID = "ChangedOnly";
        descriptive.Name = "Changed name";
        descriptive.Description = "Changed description";
        descriptive.Priority = 90;
        descriptive.Filter = "Changed filter";

        Assert.Equal(["Type"], handler.GetChangedTriggers(previous, structural));
        Assert.Empty(handler.GetChangedTriggers(previous, descriptive));
    }

    [Fact]
    public void StructuralChangeResetsUnchangedFieldsToTheCurrentOverride()
    {
        var handler = new TestableQuestRecordHandler();
        var original = CreateQuest(0x806);
        original.Priority = 10;
        original.Name = "Original";
        var current = CreateQuest(0x806);
        current.Priority = 80;
        current.Name = "Atomic owner";
        current.Type = Quest.TypeEnum.SideQuest;
        var originalContext = CreateContext(new ModKey("Skyrim.esm", ModType.Master), original);
        var currentContext = CreateContext(new ModKey("QuestOverhaul.esp", ModType.Plugin), current);

        handler.InitializeForTest(originalContext);
        var changed = handler.ResetForTest(originalContext, currentContext);

        Assert.Contains("Type", changed);
        Assert.Equal((byte)80, handler.GetForwardValue("Priority"));
        Assert.True(handler.PropertyHandlers["Name"].AreValuesEqual(
            handler.PropertyHandlers["Name"].GetValue(current),
            handler.GetForwardValue("Name")));
    }

    [Fact]
    public void QuestUsesTranslatedTextAndUnknownBitPreservingFlagHandlers()
    {
        var handler = new QuestRecordHandler();

        Assert.IsType<ComplexReflectionPropertyHandler<ITranslatedStringGetter, IQuest, IQuestGetter>>(
            handler.PropertyHandlers["Name"]);
        Assert.IsType<ComplexReflectionPropertyHandler<ITranslatedStringGetter, IQuest, IQuestGetter>>(
            handler.PropertyHandlers["Description"]);
        Assert.IsType<SimpleReflectionFlagPropertyHandler<Quest.Flag, IQuest, IQuestGetter>>(
            handler.PropertyHandlers["Flags"]);
        Assert.DoesNotContain("QuestFormVersion", handler.PropertyHandlers.Keys);

        var quest = CreateQuest(0x802);
        quest.Flags = (Quest.Flag)0x20;
        handler.ApplyForwardedProperties(quest, new Dictionary<string, object?>
        {
            ["Flags"] = Quest.Flag.RunOnce
        });

        Assert.Equal(0x120, (int)quest.Flags);
    }

    [Fact]
    public void ObjectiveWithMissingAliasIsRejectedBeforeMutation()
    {
        var handler = new QuestRecordHandler(QuestForwardingPolicy.StandardForwarding);
        var quest = CreateQuest(0x803);
        var objective = new QuestObjective { Index = 10 };
        objective.Targets.Add(new QuestObjectiveTarget { AliasID = 12 });

        var exception = Assert.Throws<InvalidOperationException>(() =>
            handler.ApplyForwardedProperties(quest, new Dictionary<string, object?>
            {
                ["Objectives"] = new List<IQuestObjectiveGetter> { objective }
            }));

        Assert.Contains("references missing alias ID 12", exception.Message);
        Assert.Empty(quest.Objectives);
    }

    [Fact]
    public void InvalidNextAliasIdIsRejectedBeforeMutation()
    {
        var handler = new QuestRecordHandler(QuestForwardingPolicy.StandardForwarding);
        var quest = CreateQuest(0x804);
        quest.Aliases.Add(new QuestAlias { ID = 5 });

        var exception = Assert.Throws<InvalidOperationException>(() =>
            handler.ApplyForwardedProperties(quest, new Dictionary<string, object?>
            {
                ["NextAliasID"] = (uint?)5
            }));

        Assert.Contains("must be greater", exception.Message);
        Assert.Null(quest.NextAliasID);
    }

    [Fact]
    public void FragmentWithMissingStageIsRejectedBeforeMutation()
    {
        var handler = new QuestRecordHandler(QuestForwardingPolicy.StandardForwarding);
        var quest = CreateQuest(0x805);
        var fragment = new QuestScriptFragment
        {
            Stage = 30,
            StageIndex = 0,
            ScriptName = "QF_Test",
            FragmentName = "Fragment_0"
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            handler.ApplyForwardedProperties(quest, new Dictionary<string, object?>
            {
                ["VirtualMachineAdapter.Presence"] = true,
                ["VirtualMachineAdapter.Fragments"] = new List<IQuestScriptFragmentGetter> { fragment }
            }));

        Assert.Contains("references missing stage 30", exception.Message);
        Assert.Null(quest.VirtualMachineAdapter);
    }

    [Fact]
    public void NestedConditionOrderAndComparisonValueAreSignificant()
    {
        var handler = new ObjectivesHandler();
        var first = CreateObjectiveWithConditions(1, 2);
        var reordered = CreateObjectiveWithConditions(2, 1);
        var changedValue = CreateObjectiveWithConditions(1, 3);

        Assert.False(handler.AreValuesEqual([first], [reordered]));
        Assert.False(handler.AreValuesEqual([first], [changedValue]));
    }

    private static QuestObjective CreateObjectiveWithConditions(float first, float second)
    {
        var objective = new QuestObjective { Index = 1 };
        var target = new QuestObjectiveTarget { AliasID = -1 };
        target.Conditions.Add(CreateCondition(first));
        target.Conditions.Add(CreateCondition(second));
        objective.Targets.Add(target);
        return objective;
    }

    private static ConditionFloat CreateCondition(float comparisonValue) => new()
    {
        ComparisonValue = comparisonValue,
        Data = new GetRandomPercentConditionData()
    };

    private static Quest CreateQuest(uint id) =>
        new(new FormKey(TestModKey, id), SkyrimRelease.SkyrimSE);

    private static IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> CreateContext(
        ModKey modKey,
        IMajorRecord record)
        => new ModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>(
            modKey,
            record,
            (_, _) => throw new NotSupportedException(),
            (_, _, _, _) => throw new NotSupportedException());

    private sealed class TestableQuestRecordHandler : QuestRecordHandler
    {
        public TestableQuestRecordHandler()
        {
        }

        public TestableQuestRecordHandler(QuestForwardingPolicy forwardingPolicy)
            : base(forwardingPolicy)
        {
        }

        public IReadOnlySet<string> AtomicTriggers => AtomicOwnershipTriggerProperties;

        public IReadOnlyList<string> GetChangedTriggers(IQuestGetter previous, IQuestGetter current) =>
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
}

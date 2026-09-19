using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.Enums;

namespace DreadsMashedPatch.RecordHandlers;

// Migration note:
// - Generalized: SMQN links, controls, and quest rows use shared semantic handlers.
// - Kept specialized: Conditions uses the shared polymorphic condition handler.
// - Intentionally non-migrated: none; Parent and PreviousSibling remain registered so topology changes are detected.
// - Coupled forwarding: topology, conditions, flags, and limits establish complete-node ownership by default;
//   quest rows remain mergeable while that configuration is stable.
// - Rationale: quest FormID is xEdit's row key, but selection controls and list contents must not be mixed
//   across a configuration boundary.
public class StoryManagerQuestNodeRecordHandler : AbstractRecordHandler
{
    private static readonly IReadOnlySet<string> ConfigurationPropertyNames = new HashSet<string>(StringComparer.Ordinal)
    {
        "Parent",
        "PreviousSibling",
        "Conditions",
        "Flags",
        "QuestFlags",
        "MaxConcurrentQuests",
        "MaxNumQuestsToRun"
    };

    private readonly StoryManagerForwardingPolicy _forwardingPolicy;

    public StoryManagerQuestNodeRecordHandler(StoryManagerForwardingPolicy? forwardingPolicy = null)
    {
        _forwardingPolicy = forwardingPolicy ?? PatcherSettings.StoryManagerPolicy;
    }

    public StoryManagerForwardingPolicy ForwardingPolicy => _forwardingPolicy;

    protected override IReadOnlySet<string> AtomicOwnershipTriggerProperties =>
        _forwardingPolicy == StoryManagerForwardingPolicy.AtomicOnConfigurationChange
            ? ConfigurationPropertyNames
            : EmptyAtomicOwnershipTriggerProperties;

    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Parent", new SimpleReflectionFormLinkPropertyHandler<IAStoryManagerNodeGetter, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("Parent") },
        { "PreviousSibling", new SimpleReflectionFormLinkPropertyHandler<IAStoryManagerNodeGetter, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("PreviousSibling") },
        { "Conditions", new ConditionsHandler<IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>(record => record.Conditions, record => record.Conditions) },
        { "Flags", new SimpleReflectionFlagPropertyHandler<AStoryManagerNode.Flag, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("Flags") },
        { "QuestFlags", new SimpleReflectionFlagPropertyHandler<StoryManagerQuestNode.QuestFlag, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("QuestFlags") },
        { "MaxConcurrentQuests", new SimpleReflectionPropertyHandler<uint?, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("MaxConcurrentQuests") },
        { "MaxNumQuestsToRun", new SimpleReflectionPropertyHandler<uint?, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("MaxNumQuestsToRun") },
        { "Quests", new SimpleReflectionListPropertyHandler<IStoryManagerQuestGetter, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("Quests", ListSemantics.AlignedOrdered, keySelector: quest => quest.Quest.FormKey) }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IStoryManagerQuestNodeGetter record)
        {
            throw new InvalidOperationException($"Expected IStoryManagerQuestNodeGetter but got {winningContext.Record.GetType()}");
        }

        return record
            .ToLink<IStoryManagerQuestNodeGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>(state.LinkCache)
            .ToArray();
    }
}

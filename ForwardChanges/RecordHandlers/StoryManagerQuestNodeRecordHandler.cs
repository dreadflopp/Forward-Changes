using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: SMQN shared node links/conditions plus quest node fields via generic handlers.
// - Kept specialized: none.
// - Rationale: property surface directly maps to existing form-link/list/scalar/flag handlers.
public class StoryManagerQuestNodeRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Parent", new SimpleReflectionFormLinkPropertyHandler<IAStoryManagerNodeGetter, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("Parent") },
        { "PreviousSibling", new SimpleReflectionFormLinkPropertyHandler<IAStoryManagerNodeGetter, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("PreviousSibling") },
        { "Conditions", new SimpleReflectionListPropertyHandler<IConditionGetter, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("Conditions", ListOrdering.None) },
        { "Flags", new SimpleReflectionFlagPropertyHandler<AStoryManagerNode.Flag, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("Flags") },
        { "QuestFlags", new SimpleReflectionFlagPropertyHandler<StoryManagerQuestNode.QuestFlag, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("QuestFlags") },
        { "MaxConcurrentQuests", new SimpleReflectionPropertyHandler<uint?, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("MaxConcurrentQuests") },
        { "MaxNumQuestsToRun", new SimpleReflectionPropertyHandler<uint?, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("MaxNumQuestsToRun") },
        { "Quests", new SimpleReflectionListPropertyHandler<IStoryManagerQuestGetter, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>("Quests", ListOrdering.None) }
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

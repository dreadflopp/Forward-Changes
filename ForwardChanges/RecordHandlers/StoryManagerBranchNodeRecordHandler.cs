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
// - Generalized: SMBN shared node links/conditions and scalar fields via generic handlers.
// - Kept specialized: none.
// - Rationale: property surface directly maps to existing form-link/list/scalar handlers.
public class StoryManagerBranchNodeRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Parent", new SimpleReflectionFormLinkPropertyHandler<IAStoryManagerNodeGetter, IStoryManagerBranchNode, IStoryManagerBranchNodeGetter>("Parent") },
        { "PreviousSibling", new SimpleReflectionFormLinkPropertyHandler<IAStoryManagerNodeGetter, IStoryManagerBranchNode, IStoryManagerBranchNodeGetter>("PreviousSibling") },
        { "Conditions", new SimpleReflectionListPropertyHandler<IConditionGetter, IStoryManagerBranchNode, IStoryManagerBranchNodeGetter>("Conditions", ListOrdering.None) },
        { "Flags", new SimpleReflectionPropertyHandler<AStoryManagerNode.Flag?, IStoryManagerBranchNode, IStoryManagerBranchNodeGetter>("Flags") },
        { "MaxConcurrentQuests", new SimpleReflectionPropertyHandler<uint?, IStoryManagerBranchNode, IStoryManagerBranchNodeGetter>("MaxConcurrentQuests") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IStoryManagerBranchNodeGetter record)
        {
            throw new InvalidOperationException($"Expected IStoryManagerBranchNodeGetter but got {winningContext.Record.GetType()}");
        }

        return record
            .ToLink<IStoryManagerBranchNodeGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IStoryManagerBranchNode, IStoryManagerBranchNodeGetter>(state.LinkCache)
            .ToArray();
    }
}

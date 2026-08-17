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
// - Generalized: SMEN shared node links/conditions and scalar fields via generic handlers.
// - Kept specialized: none.
// - Rationale: property surface directly maps to existing form-link/list/scalar handlers.
public class StoryManagerEventNodeRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Parent", new SimpleReflectionFormLinkPropertyHandler<IAStoryManagerNodeGetter, IStoryManagerEventNode, IStoryManagerEventNodeGetter>("Parent") },
        { "PreviousSibling", new SimpleReflectionFormLinkPropertyHandler<IAStoryManagerNodeGetter, IStoryManagerEventNode, IStoryManagerEventNodeGetter>("PreviousSibling") },
        { "Conditions", new SimpleReflectionListPropertyHandler<IConditionGetter, IStoryManagerEventNode, IStoryManagerEventNodeGetter>("Conditions", ListOrdering.None) },
        { "Flags", new SimpleReflectionPropertyHandler<AStoryManagerNode.Flag?, IStoryManagerEventNode, IStoryManagerEventNodeGetter>("Flags") },
        { "MaxConcurrentQuests", new SimpleReflectionPropertyHandler<uint?, IStoryManagerEventNode, IStoryManagerEventNodeGetter>("MaxConcurrentQuests") },
        { "Type", new SimpleReflectionPropertyHandler<StoryManagerEventNode.Types?, IStoryManagerEventNode, IStoryManagerEventNodeGetter>("Type") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IStoryManagerEventNodeGetter record)
        {
            throw new InvalidOperationException($"Expected IStoryManagerEventNodeGetter but got {winningContext.Record.GetType()}");
        }

        return record
            .ToLink<IStoryManagerEventNodeGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IStoryManagerEventNode, IStoryManagerEventNodeGetter>(state.LinkCache)
            .ToArray();
    }
}

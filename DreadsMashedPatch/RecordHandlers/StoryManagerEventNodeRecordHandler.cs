using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers;

// Migration note:
// - Generalized: SMEN links and scalar fields use shared semantic handlers.
// - Kept specialized: Conditions uses the shared polymorphic condition handler.
// - Intentionally non-migrated: none; Parent and PreviousSibling remain registered so topology changes are detected.
// - Coupled forwarding: every topology, condition, flag, concurrency, or event-type change establishes
//   complete-node ownership.
// - Rationale: these fields jointly define the event branch and cannot safely be recombined independently;
//   independent configuration forwarding is intentionally unavailable.
public class StoryManagerEventNodeRecordHandler : AbstractRecordHandler
{
    private static readonly IReadOnlySet<string> ConfigurationPropertyNames = new HashSet<string>(StringComparer.Ordinal)
    {
        "Parent",
        "PreviousSibling",
        "Conditions",
        "Flags",
        "MaxConcurrentQuests",
        "Type"
    };

    protected override IReadOnlySet<string> AtomicOwnershipTriggerProperties =>
        ConfigurationPropertyNames;

    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Parent", new SimpleReflectionFormLinkPropertyHandler<IAStoryManagerNodeGetter, IStoryManagerEventNode, IStoryManagerEventNodeGetter>("Parent") },
        { "PreviousSibling", new SimpleReflectionFormLinkPropertyHandler<IAStoryManagerNodeGetter, IStoryManagerEventNode, IStoryManagerEventNodeGetter>("PreviousSibling") },
        { "Conditions", new ConditionsHandler<IStoryManagerEventNode, IStoryManagerEventNodeGetter>(record => record.Conditions, record => record.Conditions) },
        { "Flags", new SimpleReflectionNullableFlagPropertyHandler<AStoryManagerNode.Flag, IStoryManagerEventNode, IStoryManagerEventNodeGetter>("Flags", preserveUnknownBits: true) },
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

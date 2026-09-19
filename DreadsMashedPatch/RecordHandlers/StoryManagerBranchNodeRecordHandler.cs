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
using DreadsMashedPatch.Enums;

namespace DreadsMashedPatch.RecordHandlers;

// Migration note:
// - Generalized: SMBN links and scalar fields use shared semantic handlers.
// - Kept specialized: Conditions uses the shared polymorphic condition handler.
// - Intentionally non-migrated: none; Parent and PreviousSibling remain registered so topology changes are detected.
// - Coupled forwarding: every topology or behavior change establishes complete-node ownership by default.
// - Rationale: parent/sibling links, conditions, flags, and concurrency form one behavior-graph decision.
public class StoryManagerBranchNodeRecordHandler : AbstractRecordHandler
{
    private static readonly IReadOnlySet<string> ConfigurationPropertyNames = new HashSet<string>(StringComparer.Ordinal)
    {
        "Parent",
        "PreviousSibling",
        "Conditions",
        "Flags",
        "MaxConcurrentQuests"
    };

    private readonly StoryManagerForwardingPolicy _forwardingPolicy;

    public StoryManagerBranchNodeRecordHandler(StoryManagerForwardingPolicy? forwardingPolicy = null)
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
        { "Parent", new SimpleReflectionFormLinkPropertyHandler<IAStoryManagerNodeGetter, IStoryManagerBranchNode, IStoryManagerBranchNodeGetter>("Parent") },
        { "PreviousSibling", new SimpleReflectionFormLinkPropertyHandler<IAStoryManagerNodeGetter, IStoryManagerBranchNode, IStoryManagerBranchNodeGetter>("PreviousSibling") },
        { "Conditions", new ConditionsHandler<IStoryManagerBranchNode, IStoryManagerBranchNodeGetter>(record => record.Conditions, record => record.Conditions) },
        { "Flags", new SimpleReflectionNullableFlagPropertyHandler<AStoryManagerNode.Flag, IStoryManagerBranchNode, IStoryManagerBranchNodeGetter>("Flags", preserveUnknownBits: true) },
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

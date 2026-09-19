using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Strings;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.Enums;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.Quest;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Abstracts;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: translated text and DNAM flags use semantic handlers; unnamed DNAM bits are preserved.
// - Specialized: VMAD, stages, objectives, and aliases retain their format-aware handlers and validation.
// - Intentionally excluded: Unknown is opaque; QuestFormVersion is marked cpIgnore by xEdit; VMAD Versioning is serializer state.
// - Coupled forwarding: structural graph changes establish a complete QUEST ownership boundary by default.
// - Rationale: aliases, objectives, stages, fragments, conditions, and event data cross-reference one another and
//   must not be independently combined into a graph that never existed in any source plugin.
public class QuestRecordHandler : AbstractRecordHandler
{
    private const string VirtualMachineAdapterPrefix = "VirtualMachineAdapter.";
    private const string VirtualMachineAdapterPresence = "VirtualMachineAdapter.Presence";

    private static readonly IReadOnlySet<string> StructuralPropertyNames = new HashSet<string>(StringComparer.Ordinal)
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
    };

    private readonly QuestForwardingPolicy _forwardingPolicy;

    public QuestRecordHandler()
        : this(PatcherSettings.QuestPolicy)
    {
    }

    public QuestRecordHandler(QuestForwardingPolicy forwardingPolicy)
    {
        _forwardingPolicy = forwardingPolicy;
    }

    public QuestForwardingPolicy ForwardingPolicy => _forwardingPolicy;

    protected override IReadOnlySet<string> AtomicOwnershipTriggerProperties =>
        _forwardingPolicy == QuestForwardingPolicy.AtomicOnStructuralChange
            ? StructuralPropertyNames
            : EmptyAtomicOwnershipTriggerProperties;

    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Name", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IQuest, IQuestGetter>("Name") },
        { "VirtualMachineAdapter.Presence", new QuestVirtualMachineAdapterPresenceHandler() },
        { "VirtualMachineAdapter.Version", new SimpleReflectionPropertyHandler<short, IQuest, IQuestGetter>("VirtualMachineAdapter.Version") },
        { "VirtualMachineAdapter.ObjectFormat", new SimpleReflectionPropertyHandler<ushort, IQuest, IQuestGetter>("VirtualMachineAdapter.ObjectFormat") },
        { "VirtualMachineAdapter.Scripts", new QuestScriptsHandler() },
        { "VirtualMachineAdapter.ExtraBindDataVersion", new SimpleReflectionPropertyHandler<byte, IQuest, IQuestGetter>("VirtualMachineAdapter.ExtraBindDataVersion") },
        { "VirtualMachineAdapter.FileName", new SimpleReflectionPropertyHandler<string, IQuest, IQuestGetter>("VirtualMachineAdapter.FileName") },
        { "VirtualMachineAdapter.Fragments", new QuestScriptFragmentHandler() },
        { "VirtualMachineAdapter.Aliases", new QuestFragmentAliasHandler() },
        { "Flags", new SimpleReflectionFlagPropertyHandler<Quest.Flag, IQuest, IQuestGetter>("Flags", preserveUnknownBits: true) },
        { "Priority", new SimpleReflectionPropertyHandler<byte, IQuest, IQuestGetter>("Priority") },
        { "Type", new SimpleReflectionPropertyHandler<Quest.TypeEnum, IQuest, IQuestGetter>("Type") },
        { "Event", new SimpleReflectionPropertyHandler<RecordType?, IQuest, IQuestGetter>("Event") },
        { "TextDisplayGlobals", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IGlobalGetter>, IQuest, IQuestGetter>("TextDisplayGlobals", ListSemantics.AlignedOrdered) },
        { "Filter", new SimpleReflectionPropertyHandler<string, IQuest, IQuestGetter>("Filter") },
        { "NextAliasID", new SimpleReflectionPropertyHandler<uint?, IQuest, IQuestGetter>("NextAliasID") },
        { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IQuest, IQuestGetter>("Description") },
        { "DialogConditions", new DialogConditionsHandler() },
        { "EventConditions", new EventConditionsHandler() },
        { "Stages", new StagesHandler() },
        { "Objectives", new ObjectivesHandler() },
        { "Aliases", new AliasesHandler() }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IQuestGetter questRecord)
        {
            throw new InvalidOperationException($"Expected IQuestGetter but got {winningContext.Record.GetType()}");
        }
        var contexts = questRecord
            .ToLink<IQuestGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IQuest, IQuestGetter>(state.LinkCache)
            .ToArray();

        return contexts!;
    }

    /// <summary>
    /// Applies the VMAD parent decision before any independently computed child values.
    /// If absence wins, child writes are suppressed so they cannot recreate a partial VMAD.
    /// </summary>
    public override void ApplyForwardedProperties(
        IMajorRecord record,
        Dictionary<string, object?> propertiesToForward)
    {
        if (record is not IQuest quest)
        {
            throw new InvalidOperationException($"Expected IQuest but got {record.GetType()}");
        }

        var hasPresenceDecision = propertiesToForward.TryGetValue(
            VirtualMachineAdapterPresence,
            out var presenceValue);
        var shouldBePresent = hasPresenceDecision
            ? presenceValue is bool present
                ? present
                : throw new InvalidOperationException(
                    $"{VirtualMachineAdapterPresence} expected a Boolean value but got {presenceValue?.GetType().Name ?? "null"}")
            : quest.VirtualMachineAdapter != null;

        // Rebuild the dictionary so presence is always applied before child fields,
        // regardless of the caller's insertion order. The base implementation mutates
        // its dictionary while coordinating flags, so do not pass the caller's instance.
        var coordinatedProperties = new Dictionary<string, object?>();
        if (hasPresenceDecision)
        {
            coordinatedProperties[VirtualMachineAdapterPresence] = shouldBePresent;
        }

        foreach (var (propertyName, value) in propertiesToForward)
        {
            if (propertyName == VirtualMachineAdapterPresence)
            {
                continue;
            }

            if (!shouldBePresent && propertyName.StartsWith(VirtualMachineAdapterPrefix, StringComparison.Ordinal))
            {
                if (LogCollector.IsDetailedMode)
                {
                    Console.WriteLine(
                        $"[{propertyName}] Skipping computed child value because {VirtualMachineAdapterPresence} is Absent");
                }
                continue;
            }

            coordinatedProperties[propertyName] = value;
        }

        // Materialize and validate the complete proposed graph before touching the patch record.
        // This prevents a failed validation from leaving a partially written QUEST override.
        if (coordinatedProperties.Keys.Any(StructuralPropertyNames.Contains))
        {
            var proposedQuest = quest.DeepCopy();
            base.ApplyForwardedProperties(
                proposedQuest,
                new Dictionary<string, object?>(coordinatedProperties));
            ValidateProposedQuestGraph(proposedQuest);
        }

        base.ApplyForwardedProperties(record, coordinatedProperties);
    }

    private static void ValidateProposedQuestGraph(IQuestGetter quest)
    {
        ValidateAliases(quest);
        ValidateStagesAndObjectives(quest);
        ValidateVirtualMachineAdapter(quest);
    }

    private static void ValidateAliases(IQuestGetter quest)
    {
        var aliasIds = new HashSet<uint>();
        foreach (var alias in quest.Aliases)
        {
            if (!aliasIds.Add(alias.ID))
            {
                throw new InvalidOperationException(
                    $"Quest {quest.FormKey} contains duplicate alias ID {alias.ID}.");
            }
        }

        if (quest.NextAliasID is uint nextAliasId && aliasIds.Count > 0)
        {
            var greatestAliasId = aliasIds.Max();
            if (nextAliasId <= greatestAliasId)
            {
                throw new InvalidOperationException(
                    $"Quest {quest.FormKey} NextAliasID {nextAliasId} must be greater than existing alias ID {greatestAliasId}.");
            }
        }

        foreach (var objective in quest.Objectives)
        {
            foreach (var target in objective.Targets)
            {
                ValidateAliasReference(
                    quest,
                    aliasIds,
                    target.AliasID,
                    $"objective {objective.Index} target");
            }
        }

        if (quest.VirtualMachineAdapter == null)
        {
            return;
        }

        for (var index = 0; index < quest.VirtualMachineAdapter.Aliases.Count; index++)
        {
            ValidateAliasReference(
                quest,
                aliasIds,
                quest.VirtualMachineAdapter.Aliases[index].Property.Alias,
                $"VMAD alias entry {index}");
        }
    }

    private static void ValidateAliasReference(
        IQuestGetter quest,
        IReadOnlySet<uint> aliasIds,
        int aliasId,
        string source)
    {
        // Negative values are format sentinels for "no alias".
        if (aliasId >= 0 && !aliasIds.Contains((uint)aliasId))
        {
            throw new InvalidOperationException(
                $"Quest {quest.FormKey} {source} references missing alias ID {aliasId}.");
        }
    }

    private static void ValidateStagesAndObjectives(IQuestGetter quest)
    {
        var stagesByIndex = new Dictionary<ushort, IQuestStageGetter>();
        foreach (var stage in quest.Stages)
        {
            if (!stagesByIndex.TryAdd(stage.Index, stage))
            {
                throw new InvalidOperationException(
                    $"Quest {quest.FormKey} contains duplicate stage index {stage.Index}.");
            }
        }

        var objectiveIndexes = new HashSet<ushort>();
        foreach (var objective in quest.Objectives)
        {
            if (!objectiveIndexes.Add(objective.Index))
            {
                throw new InvalidOperationException(
                    $"Quest {quest.FormKey} contains duplicate objective index {objective.Index}.");
            }
        }

        var adapter = quest.VirtualMachineAdapter;
        if (adapter == null)
        {
            return;
        }

        foreach (var fragment in adapter.Fragments)
        {
            if (!stagesByIndex.TryGetValue(fragment.Stage, out var stage))
            {
                throw new InvalidOperationException(
                    $"Quest {quest.FormKey} VMAD fragment references missing stage {fragment.Stage}.");
            }

            if (fragment.StageIndex < 0 || fragment.StageIndex >= stage.LogEntries.Count)
            {
                throw new InvalidOperationException(
                    $"Quest {quest.FormKey} VMAD fragment for stage {fragment.Stage} references missing log entry {fragment.StageIndex}.");
            }
        }
    }

    private static void ValidateVirtualMachineAdapter(IQuestGetter quest)
    {
        var adapter = quest.VirtualMachineAdapter;
        if (adapter == null)
        {
            return;
        }

        var scripts = adapter.Scripts.Concat(
            adapter.Aliases.SelectMany(alias => alias.Scripts));
        var containsObjectProperties = scripts.Any(script =>
            script.Properties.Any(property =>
                property is IScriptObjectPropertyGetter or IScriptObjectListPropertyGetter));
        if (containsObjectProperties && adapter.ObjectFormat is not (1 or 2))
        {
            throw new InvalidOperationException(
                $"Quest {quest.FormKey} VMAD contains object script properties but has unsupported ObjectFormat {adapter.ObjectFormat}; expected 1 or 2.");
        }
    }
}

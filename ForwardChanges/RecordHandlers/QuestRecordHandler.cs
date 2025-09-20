using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.Quest;
using ForwardChanges.PropertyHandlers.General;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ForwardChanges.RecordHandlers;

public class QuestRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        // General properties
        { "EditorID", new EditorIDHandler() },
        { "Name", new NameHandler() },
        
        // Quest-specific properties (VirtualMachineAdapter components)
        { "QuestScripts", new QuestScriptsHandler() },
        { "QuestScriptFragments", new QuestScriptFragmentHandler() },
        { "QuestFragmentAliases", new QuestFragmentAliasHandler() },
        { "Flags", new FlagsHandler() },
        { "Priority", new PriorityHandler() },
        { "QuestFormVersion", new QuestFormVersionHandler() },
        { "Unknown", new UnknownHandler() },
        { "Type", new TypeHandler() },
        { "Event", new QuestEventHandler() },
        { "TextDisplayGlobals", new TextDisplayGlobalsHandler() },
        { "Filter", new FilterHandler() },
        { "NextAliasID", new NextAliasIDHandler() },
        { "Description", new DescriptionHandler() },
        
        // List properties
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

    public override IMajorRecord GetOverrideRecord(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        return winningContext.GetOrAddAsOverride(state.PatchMod);
    }

    public override void ApplyForwardedProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward)
    {
        foreach (var (propertyName, value) in propertiesToForward)
        {
            if (PropertyHandlers.TryGetValue(propertyName, out var handler))
            {
                Console.WriteLine($"[{propertyName}] Applying value: {handler.FormatValue(value)}, Type: {value?.GetType()}");
                handler.SetValue(record, value);
            }
        }
    }
}

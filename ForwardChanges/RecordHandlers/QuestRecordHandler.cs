using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.Quest;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Abstracts;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ForwardChanges.RecordHandlers;

public class QuestRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Name", new NameHandler() },
        { "QuestScripts", new QuestScriptsHandler() },
        { "QuestScriptFragments", new QuestScriptFragmentHandler() },
        { "QuestFragmentAliases", new QuestFragmentAliasHandler() },
        { "Flags", new FlagsHandler() },
        { "Priority", new SimpleReflectionPropertyHandler<byte, IQuest, IQuestGetter>("Priority") },
        { "QuestFormVersion", new SimpleReflectionPropertyHandler<byte, IQuest, IQuestGetter>("QuestFormVersion") },
        { "Unknown", new SimpleReflectionPropertyHandler<int, IQuest, IQuestGetter>("Unknown") },
        { "Type", new SimpleReflectionPropertyHandler<Quest.TypeEnum, IQuest, IQuestGetter>("Type") },
        { "Event", new SimpleReflectionPropertyHandler<RecordType?, IQuest, IQuestGetter>("Event") },
        { "TextDisplayGlobals", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IGlobalGetter>, IQuest, IQuestGetter>("TextDisplayGlobals", ListOrdering.None) },
        { "Filter", new SimpleReflectionPropertyHandler<string, IQuest, IQuestGetter>("Filter") },
        { "NextAliasID", new SimpleReflectionPropertyHandler<uint?, IQuest, IQuestGetter>("NextAliasID") },
        { "Description", new DescriptionHandler() },
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

    // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
    // The base class automatically handles flag property coordination
}

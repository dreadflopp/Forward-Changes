using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.DialogTopic;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class DialogTopicRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "Priority", new PriorityHandler() },
            { "Branch", new BranchHandler() },
            { "Quest", new QuestHandler() },
            { "TopicFlags", new TopicFlagsHandler() },
            { "Category", new CategoryHandler() },
            { "Subtype", new SubtypeHandler() },
            { "SubtypeName", new SubtypeNameHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IDialogTopicGetter dialogTopicRecord)
            {
                throw new InvalidOperationException($"Expected IDialogTopicGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = dialogTopicRecord
                .ToLink<IDialogTopicGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IDialogTopic, IDialogTopicGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
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
            { "Priority", new SimpleReflectionPropertyHandler<float, IDialogTopic, IDialogTopicGetter>("Priority", 0.001f) },
            { "Branch", new SimpleReflectionFormLinkPropertyHandler<IDialogBranchGetter, IDialogTopic, IDialogTopicGetter>("Branch") },
            { "Quest", new SimpleReflectionFormLinkPropertyHandler<IQuestGetter, IDialogTopic, IDialogTopicGetter>("Quest") },
            { "TopicFlags", new TopicFlagsHandler() },
            { "Category", new SimpleReflectionPropertyHandler<DialogTopic.CategoryEnum, IDialogTopic, IDialogTopicGetter>("Category") },
            { "Subtype", new SimpleReflectionPropertyHandler<DialogTopic.SubtypeEnum, IDialogTopic, IDialogTopicGetter>("Subtype") },
            { "SubtypeName", new SimpleReflectionPropertyHandler<RecordType, IDialogTopic, IDialogTopicGetter>("SubtypeName") }
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
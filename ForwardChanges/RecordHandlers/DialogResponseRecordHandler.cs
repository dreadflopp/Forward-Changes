using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.DialogResponse;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class DialogResponseRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() },
            { "Flags", new FlagsHandler() },
            { "MajorFlags", new MajorFlagsHandler() },
            { "ResetHours", new ResetHoursHandler() },
            { "Topic", new TopicHandler() },
            { "PreviousDialog", new PreviousDialogHandler() },
            { "FavorLevel", new FavorLevelHandler() },
            { "LinkTo", new LinkToHandler() },
            { "ResponseData", new ResponseDataHandler() },
            { "Responses", new ResponsesHandler() },
            { "Conditions", new ConditionsHandler() },

            { "Prompt", new PromptHandler() },
            { "Speaker", new SpeakerHandler() },
            { "WalkAwayTopic", new WalkAwayTopicHandler() },
            { "AudioOutputOverride", new AudioOutputOverrideHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IDialogResponsesGetter dialogResponseRecord)
            {
                throw new InvalidOperationException($"Expected IDialogResponsesGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = dialogResponseRecord
                .ToLink<IDialogResponsesGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IDialogResponses, IDialogResponsesGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
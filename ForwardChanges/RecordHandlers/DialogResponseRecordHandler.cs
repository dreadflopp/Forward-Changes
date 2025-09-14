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
                    try
                    {
                        Console.WriteLine($"[{propertyName}] Applying value: {handler.FormatValue(value)}, Type: {value?.GetType()}");
                        handler.SetValue(record, value);
                    }
                    catch (Exception ex)
                    {
                        // Property doesn't exist on this dialog response type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on dialog response {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}
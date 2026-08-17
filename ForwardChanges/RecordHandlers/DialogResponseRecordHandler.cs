using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
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
            { "ResetHours", new SimpleReflectionPropertyHandler<float, IDialogResponses, IDialogResponsesGetter>("Flags.ResetHours") },
            { "Topic", new SimpleReflectionFormLinkPropertyHandler<IDialogTopicGetter, IDialogResponses, IDialogResponsesGetter>("Topic") },
            { "PreviousDialog", new SimpleReflectionFormLinkPropertyHandler<IDialogResponsesGetter, IDialogResponses, IDialogResponsesGetter>("PreviousDialog") },
            { "FavorLevel", new SimpleReflectionPropertyHandler<FavorLevel?, IDialogResponses, IDialogResponsesGetter>("FavorLevel") },
            { "LinkTo", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IDialogGetter>, IDialogResponses, IDialogResponsesGetter>("LinkTo") },
            { "ResponseData", new SimpleReflectionFormLinkPropertyHandler<IDialogResponsesGetter, IDialogResponses, IDialogResponsesGetter>("ResponseData") },
            { "Responses", new ResponsesHandler(normalizeTrailingWhitespace: true) },
            { "Conditions", new ConditionsHandler() },

            { "Prompt", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IDialogResponses, IDialogResponsesGetter>("Prompt") },
            { "Speaker", new SimpleReflectionFormLinkPropertyHandler<INpcGetter, IDialogResponses, IDialogResponsesGetter>("Speaker") },
            { "WalkAwayTopic", new SimpleReflectionFormLinkPropertyHandler<IDialogTopicGetter, IDialogResponses, IDialogResponsesGetter>("WalkAwayTopic") },
            { "AudioOutputOverride", new SimpleReflectionFormLinkPropertyHandler<ISoundOutputModelGetter, IDialogResponses, IDialogResponsesGetter>("AudioOutputOverride") }
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
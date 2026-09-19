using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using DreadsMashedPatch.PropertyHandlers.DialogResponse;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using System;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: DATA uses the shared binary deep-copy handler.
    // - Kept specialized: response, condition, script, and flag handlers retain merge and record-specific behavior.
    // - Intentionally excluded: PreviousDialog is runtime/structural linkage; UnknownData is an opaque SCHR/QNAM/NEXT payload.
    // - Rationale: excluded fields remain from the winning override instead of being synthesized across plugins.
    public class DialogResponseRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "DATA", new SimpleReflectionBinaryDataPropertyHandler<IDialogResponses, IDialogResponsesGetter>("DATA") },
            { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() },
            { "Flags", new FlagsHandler() },
            { "MajorFlags", new MajorFlagsHandler() },
            { "ResetHours", new SimpleReflectionPropertyHandler<float, IDialogResponses, IDialogResponsesGetter>("Flags.ResetHours") },
            { "Topic", new SimpleReflectionFormLinkPropertyHandler<IDialogTopicGetter, IDialogResponses, IDialogResponsesGetter>("Topic") },
            { "FavorLevel", new SimpleReflectionPropertyHandler<FavorLevel?, IDialogResponses, IDialogResponsesGetter>("FavorLevel") },
            { "LinkTo", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IDialogTopicGetter>, IDialogResponses, IDialogResponsesGetter>("LinkTo", ListSemantics.AlignedOrdered) },
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

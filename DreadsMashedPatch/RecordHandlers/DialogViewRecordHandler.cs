using System;
using Noggog;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.DialogView;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: Quest, ENAM, DNAM via reflection handlers.
    // - Kept specialized: Branches and TNAMs via dedicated list/binary-slice handlers.
    // - Rationale: preserve precise list mutation and binary sequence semantics.
    public class DialogViewRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Quest", new SimpleReflectionFormLinkPropertyHandler<IQuestGetter, IDialogView, IDialogViewGetter>("Quest") },
            { "Branches", new BranchesHandler() },
            { "TNAMs", new TNAMsHandler() },
            { "ENAM", new SimpleReflectionBinaryDataPropertyHandler<IDialogView, IDialogViewGetter>("ENAM") },
            { "DNAM", new SimpleReflectionBinaryDataPropertyHandler<IDialogView, IDialogViewGetter>("DNAM") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IDialogViewGetter dialogView)
            {
                throw new InvalidOperationException($"Expected IDialogViewGetter but got {winningContext.Record.GetType()}");
            }

            return dialogView
                .ToLink<IDialogViewGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IDialogView, IDialogViewGetter>(state.LinkCache)
                .ToArray();
        }
    }
}

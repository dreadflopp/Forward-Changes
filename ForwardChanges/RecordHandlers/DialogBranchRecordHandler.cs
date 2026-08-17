using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.DialogBranch;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: Quest, Category, StartingTopic via reflection handlers.
    // - Kept specialized: Flags via project-approved flag handler.
    // - Rationale: preserve explicit enum flag handling policy.
    public class DialogBranchRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Quest", new SimpleReflectionFormLinkPropertyHandler<IQuestGetter, IDialogBranch, IDialogBranchGetter>("Quest") },
            { "Category", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.DialogBranch.CategoryType?, IDialogBranch, IDialogBranchGetter>("Category") },
            { "Flags", new FlagsHandler() },
            { "StartingTopic", new SimpleReflectionFormLinkPropertyHandler<IDialogTopicGetter, IDialogBranch, IDialogBranchGetter>("StartingTopic") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IDialogBranchGetter dialogBranch)
            {
                throw new InvalidOperationException($"Expected IDialogBranchGetter but got {winningContext.Record.GetType()}");
            }

            return dialogBranch
                .ToLink<IDialogBranchGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IDialogBranch, IDialogBranchGetter>(state.LinkCache)
                .ToArray();
        }
    }
}

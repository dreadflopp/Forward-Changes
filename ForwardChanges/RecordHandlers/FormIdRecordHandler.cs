using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.FormList;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: inherited FormList record-header flags use the project-standard coordinated flag handlers.
    // - Kept specialized: Items retains FormList ownership merging and progressive aligned-slot ordering.
    // - Rationale: FormLists are mergeable collections whose order can be observed through indexed access;
    //   Mutagen also exposes both flag views on IFormList/IFormListGetter, so generic reflection is not used for flags.
    public class FormIdRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Items", new FormIdsHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IFormListGetter formListRecord)
            {
                throw new InvalidOperationException($"Expected IFormListGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = formListRecord
                .ToLink<IFormListGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IFormList, IFormListGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}

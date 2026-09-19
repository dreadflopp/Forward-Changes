using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: ImpactDataSet and Tag via reflection handlers.
    // - Kept specialized: none.
    // - Rationale: record is simple scalar/formlink surface with stable shared handlers.
    public class FootstepRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ImpactDataSet", new SimpleReflectionFormLinkPropertyHandler<IImpactDataSetGetter, IFootstep, IFootstepGetter>("ImpactDataSet") },
            { "Tag", new SimpleReflectionPropertyHandler<string, IFootstep, IFootstepGetter>("Tag") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IFootstepGetter footstep)
            {
                throw new InvalidOperationException($"Expected IFootstepGetter but got {winningContext.Record.GetType()}");
            }

            return footstep
                .ToLink<IFootstepGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IFootstep, IFootstepGetter>(state.LinkCache)
                .ToArray();
        }
    }
}

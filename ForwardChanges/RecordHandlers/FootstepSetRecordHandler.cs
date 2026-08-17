using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: all footstep list properties with shared list reflection handlers.
    // - Kept specialized: none.
    // - Rationale: homogeneous formlink lists fit existing generic list handling.
    public class FootstepSetRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "WalkForwardFootsteps", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IFootstepGetter>, IFootstepSet, IFootstepSetGetter>("WalkForwardFootsteps", ListOrdering.None) },
            { "RunForwardFootsteps", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IFootstepGetter>, IFootstepSet, IFootstepSetGetter>("RunForwardFootsteps", ListOrdering.None) },
            { "WalkForwardAlternateFootsteps", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IFootstepGetter>, IFootstepSet, IFootstepSetGetter>("WalkForwardAlternateFootsteps", ListOrdering.None) },
            { "RunForwardAlternateFootsteps", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IFootstepGetter>, IFootstepSet, IFootstepSetGetter>("RunForwardAlternateFootsteps", ListOrdering.None) },
            { "WalkForwardAlternateFootsteps2", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IFootstepGetter>, IFootstepSet, IFootstepSetGetter>("WalkForwardAlternateFootsteps2", ListOrdering.None) }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IFootstepSetGetter footstepSet)
            {
                throw new InvalidOperationException($"Expected IFootstepSetGetter but got {winningContext.Record.GetType()}");
            }

            return footstepSet
                .ToLink<IFootstepSetGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IFootstepSet, IFootstepSetGetter>(state.LinkCache)
                .ToArray();
        }
    }
}

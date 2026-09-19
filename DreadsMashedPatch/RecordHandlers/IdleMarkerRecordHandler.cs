using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.IdleMarker;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: bounds, timer, animations, model, and major flags.
    // - Kept specialized: nullable Flags via dedicated flag handler.
    // - Rationale: keep nullable flag behavior explicit while reusing shared handlers.
    public class IdleMarkerRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Flags", new FlagsHandler() },
            { "IdleTimer", new SimpleReflectionPropertyHandler<float?, IIdleMarker, IIdleMarkerGetter>("IdleTimer", 0.001f) },
            { "Animations", new AtomicReflectionListPropertyHandler<IFormLinkGetter<IIdleAnimationGetter>, IIdleMarker, IIdleMarkerGetter>("Animations", true) },
            { "Model", new ModelHandler() },
            { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.IdleMarker.MajorFlag, IIdleMarker, IIdleMarkerGetter>("MajorFlags") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IIdleMarkerGetter idleMarker)
            {
                throw new InvalidOperationException($"Expected IIdleMarkerGetter but got {winningContext.Record.GetType()}");
            }

            return idleMarker
                .ToLink<IIdleMarkerGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IIdleMarker, IIdleMarkerGetter>(state.LinkCache)
                .ToArray();
        }
    }
}

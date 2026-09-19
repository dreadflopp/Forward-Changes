using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: UnloadEvent via reflection.
    // - Kept specialized: Model uses existing modeled-asset handler.
    // - Rationale: matches project pattern of shared model handling plus minimal record-specific fields.
    public class AnimatedObjectRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Model", new ModelHandler() },
            { "UnloadEvent", new SimpleReflectionPropertyHandler<string, IAnimatedObject, IAnimatedObjectGetter>("UnloadEvent") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IAnimatedObjectGetter animatedObject)
            {
                throw new InvalidOperationException($"Expected IAnimatedObjectGetter but got {winningContext.Record.GetType()}");
            }

            return animatedObject
                .ToLink<IAnimatedObjectGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IAnimatedObject, IAnimatedObjectGetter>(state.LinkCache)
                .ToArray();
        }
    }
}

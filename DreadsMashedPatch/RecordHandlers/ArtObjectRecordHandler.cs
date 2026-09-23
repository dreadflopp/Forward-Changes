using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: Type enum via reflection.
    // - Kept specialized: ObjectBounds and Model via shared handlers.
    // - Rationale: small record surface; existing shared handlers cover model/bounds semantics.
    public class ArtObjectRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ModelAndBounds", new ModelBoundsHandler() },
            { "Type", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.ArtObject.TypeEnum?, IArtObject, IArtObjectGetter>("Type") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IArtObjectGetter artObject)
            {
                throw new InvalidOperationException($"Expected IArtObjectGetter but got {winningContext.Record.GetType()}");
            }

            return artObject
                .ToLink<IArtObjectGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IArtObject, IArtObjectGetter>(state.LinkCache)
                .ToArray();
        }
    }
}

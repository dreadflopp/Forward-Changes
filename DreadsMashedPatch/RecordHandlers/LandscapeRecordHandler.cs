using System;
using Noggog;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.PropertyHandlers.Landscape;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: flags, height map, layers, and textures via reflection handlers.
    // - Kept specialized: vertex normal/color arrays.
    // - Rationale: overlay IReadOnlyArray2d values require typed mutable Array2d copies.
    public class LandscapeRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Flags", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.Landscape.Flag?, ILandscape, ILandscapeGetter>("Flags") },
            { "VertexNormals", new LandscapeArray2dHandler(vertexNormals: true) },
            { "VertexHeightMap", new ComplexReflectionPropertyHandler<ILandscapeVertexHeightMapGetter, ILandscape, ILandscapeGetter>("VertexHeightMap") },
            { "VertexColors", new LandscapeArray2dHandler(vertexNormals: false) },
            { "Layers", new SimpleReflectionListPropertyHandler<IBaseLayerGetter, ILandscape, ILandscapeGetter>("Layers", ListSemantics.Unordered) },
            { "Textures", new SimpleReflectionListPropertyHandler<IFormLinkGetter<ILandscapeTextureGetter>, ILandscape, ILandscapeGetter>("Textures", ListSemantics.Unordered, canBeNull: true) }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ILandscapeGetter landscapeRecord)
            {
                throw new InvalidOperationException($"Expected ILandscapeGetter but got {winningContext.Record.GetType()}");
            }

            return landscapeRecord
                .ToLink<ILandscapeGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ILandscape, ILandscapeGetter>(state.LinkCache)
                .ToArray();
        }
    }
}

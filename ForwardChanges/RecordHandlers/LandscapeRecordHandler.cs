using System;
using Noggog;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: flags, vertex arrays, height map, layers, and textures via reflection handlers.
    // - Kept specialized: none.
    // - Rationale: the record is a structured composition of nested value objects and lists.
    public class LandscapeRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Flags", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.Landscape.Flag?, ILandscape, ILandscapeGetter>("Flags") },
            { "VertexNormals", new ComplexReflectionPropertyHandler<IReadOnlyArray2d<P3UInt8>, ILandscape, ILandscapeGetter>("VertexNormals") },
            { "VertexHeightMap", new ComplexReflectionPropertyHandler<ILandscapeVertexHeightMapGetter, ILandscape, ILandscapeGetter>("VertexHeightMap") },
            { "VertexColors", new ComplexReflectionPropertyHandler<IReadOnlyArray2d<P3UInt8>, ILandscape, ILandscapeGetter>("VertexColors") },
            { "Layers", new SimpleReflectionListPropertyHandler<IBaseLayerGetter, ILandscape, ILandscapeGetter>("Layers") },
            { "Textures", new SimpleReflectionListPropertyHandler<IFormLinkGetter<ILandscapeTextureGetter>, ILandscape, ILandscapeGetter>("Textures", ListOrdering.None, canBeNull: true) }
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
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
    // - Generalized: ENAM and nested image-space sections via reflection handlers.
    // - Kept specialized: none.
    // - Rationale: the record is a small composition of binary data plus nested value objects.
    public class ImageSpaceRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ENAM", new SimpleReflectionBinaryDataPropertyHandler<IImageSpace, IImageSpaceGetter>("ENAM") },
            { "Hdr", new ComplexReflectionPropertyHandler<IImageSpaceHdrGetter, IImageSpace, IImageSpaceGetter>("Hdr") },
            { "Cinematic", new ComplexReflectionPropertyHandler<IImageSpaceCinematicGetter, IImageSpace, IImageSpaceGetter>("Cinematic") },
            { "Tint", new ComplexReflectionPropertyHandler<IImageSpaceTintGetter, IImageSpace, IImageSpaceGetter>("Tint") },
            { "DepthOfField", new ComplexReflectionPropertyHandler<IImageSpaceDepthOfFieldGetter, IImageSpace, IImageSpaceGetter>("DepthOfField") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IImageSpaceGetter imageSpace)
            {
                throw new InvalidOperationException($"Expected IImageSpaceGetter but got {winningContext.Record.GetType()}");
            }

            return imageSpace
                .ToLink<IImageSpaceGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IImageSpace, IImageSpaceGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
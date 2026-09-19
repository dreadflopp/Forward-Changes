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
    // - Generalized: scalar fields, form links, and decal via reflection handlers.
    // - Kept specialized: none.
    // - Intentionally excluded: Unknown is outside the semantic conflict surface.
    // - Rationale: the interface surface is small and matches existing reflection-safe patterns.
    public class ImpactRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Model", new ModelHandler() },
            { "Duration", new SimpleReflectionPropertyHandler<float, IImpact, IImpactGetter>("Duration") },
            { "Orientation", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.Impact.OrientationType, IImpact, IImpactGetter>("Orientation") },
            { "AngleThreshold", new SimpleReflectionPropertyHandler<float, IImpact, IImpactGetter>("AngleThreshold") },
            { "PlacementRadius", new SimpleReflectionPropertyHandler<float, IImpact, IImpactGetter>("PlacementRadius") },
            { "SoundLevel", new SimpleReflectionPropertyHandler<SoundLevel, IImpact, IImpactGetter>("SoundLevel") },
            { "NoDecalData", new SimpleReflectionPropertyHandler<bool, IImpact, IImpactGetter>("NoDecalData") },
            { "Result", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.Impact.ResultType, IImpact, IImpactGetter>("Result") },
            { "Decal", new ComplexReflectionPropertyHandler<IDecalGetter, IImpact, IImpactGetter>("Decal") },
            { "TextureSet", new SimpleReflectionFormLinkPropertyHandler<ITextureSetGetter, IImpact, IImpactGetter>("TextureSet") },
            { "SecondaryTextureSet", new SimpleReflectionFormLinkPropertyHandler<ITextureSetGetter, IImpact, IImpactGetter>("SecondaryTextureSet") },
            { "Sound1", new SimpleReflectionFormLinkPropertyHandler<ISoundGetter, IImpact, IImpactGetter>("Sound1") },
            { "Sound2", new SimpleReflectionFormLinkPropertyHandler<ISoundGetter, IImpact, IImpactGetter>("Sound2") },
            { "Hazard", new SimpleReflectionFormLinkPropertyHandler<IHazardGetter, IImpact, IImpactGetter>("Hazard") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IImpactGetter impact)
            {
                throw new InvalidOperationException($"Expected IImpactGetter but got {winningContext.Record.GetType()}");
            }

            return impact
                .ToLink<IImpactGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IImpact, IImpactGetter>(state.LinkCache)
                .ToArray();
        }
    }
}

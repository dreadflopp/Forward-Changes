using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.TextureSet;
using DreadsMashedPatch.PropertyHandlers.Interfaces;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Independent: TX00-TX07 texture slots remain separately forwardable Skyrim fields.
    // - Kept specialized/atomic: the complete DODT decal payload remains one cohesive value.
    // - Flag decision: the raw record-header handler is the sole header storage path; nullable
    //   TXST DNAM presence and its individual bits are handled together by the typed flag handler.
    // - Rationale: texture slots can be intentionally supplied independently, while partial decal
    //   geometry/settings and scalar flag replacement can create invalid or lost combinations.
    public class TextureSetRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler(typeof(SkyrimMajorRecord.SkyrimMajorRecordFlag)) },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Diffuse", new DiffuseHandler() },
            { "NormalOrGloss", new NormalOrGlossHandler() },
            { "EnvironmentMaskOrSubsurfaceTint", new EnvironmentMaskOrSubsurfaceTintHandler() },
            { "GlowOrDetailMap", new GlowOrDetailMapHandler() },
            { "Height", new HeightHandler() },
            { "Environment", new EnvironmentHandler() },
            { "Multilayer", new MultilayerHandler() },
            { "BacklightMaskOrSpecular", new BacklightMaskOrSpecularHandler() },
            { "Decal", new GeneratedCopyReflectionPropertyHandler<IDecalGetter, Decal, ITextureSet, ITextureSetGetter>(
                "Decal",
                value => value.DeepCopy(),
                DecalMixIn.Equals) },
            { "Flags", new FlagsHandler() },
        };


        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ITextureSetGetter textureSetRecord)
                throw new InvalidOperationException($"Expected ITextureSetGetter but got {winningContext.Record.GetType()}");

            return textureSetRecord
                .ToLink<ITextureSetGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ITextureSet, ITextureSetGetter>(state.LinkCache)
                .ToArray();
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}

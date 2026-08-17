using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.TextureSet;
using ForwardChanges.PropertyHandlers.Interfaces;
using TextureSetFlag = Mutagen.Bethesda.Skyrim.TextureSet.Flag;

namespace ForwardChanges.RecordHandlers
{
    public class TextureSetRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Diffuse", new DiffuseHandler() },
            { "NormalOrGloss", new NormalOrGlossHandler() },
            { "EnvironmentMaskOrSubsurfaceTint", new EnvironmentMaskOrSubsurfaceTintHandler() },
            { "GlowOrDetailMap", new GlowOrDetailMapHandler() },
            { "Height", new HeightHandler() },
            { "Environment", new EnvironmentHandler() },
            { "Multilayer", new MultilayerHandler() },
            { "BacklightMaskOrSpecular", new BacklightMaskOrSpecularHandler() },
            { "Decal", new ComplexReflectionPropertyHandler<IDecalGetter, ITextureSet, ITextureSetGetter>("Decal") },
            { "Flags", new SimpleReflectionPropertyHandler<TextureSetFlag?, ITextureSet, ITextureSetGetter>("Flags") },
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

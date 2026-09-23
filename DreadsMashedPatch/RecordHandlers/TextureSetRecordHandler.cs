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
    // - Kept specialized/atomic: TX00-TX07 plus typed DNAM flags form one authored texture
    //   definition; the complete DODT decal payload remains a separate cohesive value.
    // - Independent: ObjectBounds remains independently forwardable.
    // - Flag decision: TextureDefinition delegates DNAM presence/bit copying and equality to the
    //   project-approved typed nullable flag handler rather than generic reflection.
    // - Rationale: DNAM changes how several texture slots are interpreted, and independently
    //   combining channels can create a texture set no source plugin authored.
    public class TextureSetRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler(typeof(SkyrimMajorRecord.SkyrimMajorRecordFlag)) },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "TextureDefinition", new TextureDefinitionHandler() },
            { "Decal", new GeneratedCopyReflectionPropertyHandler<IDecalGetter, Decal, ITextureSet, ITextureSetGetter>(
                "Decal",
                value => value.DeepCopy(),
                DecalMixIn.Equals) },
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

using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;
using Noggog;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: links, scalar fields, flags, and grass list via reflection handlers.
    // - Kept specialized: none.
    // - Rationale: the surface is compact and fits the generic link/list handlers.
    public class LandscapeTextureRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "TextureSet", new SimpleReflectionFormLinkPropertyHandler<ITextureSetGetter, ILandscapeTexture, ILandscapeTextureGetter>("TextureSet") },
            { "MaterialType", new SimpleReflectionFormLinkPropertyHandler<IMaterialTypeGetter, ILandscapeTexture, ILandscapeTextureGetter>("MaterialType") },
            { "HavokFriction", new SimpleReflectionPropertyHandler<byte, ILandscapeTexture, ILandscapeTextureGetter>("HavokFriction") },
            { "HavokRestitution", new SimpleReflectionPropertyHandler<byte, ILandscapeTexture, ILandscapeTextureGetter>("HavokRestitution") },
            { "TextureSpecularExponent", new SimpleReflectionPropertyHandler<byte, ILandscapeTexture, ILandscapeTextureGetter>("TextureSpecularExponent") },
            { "Grasses", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IGrassGetter>, ILandscapeTexture, ILandscapeTextureGetter>("Grasses", ListSemantics.SortedKeyed) },
            { "Flags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.LandscapeTexture.Flag, ILandscapeTexture, ILandscapeTextureGetter>("Flags") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ILandscapeTextureGetter landscapeTexture)
            {
                throw new InvalidOperationException($"Expected ILandscapeTextureGetter but got {winningContext.Record.GetType()}");
            }

            return landscapeTexture
                .ToLink<ILandscapeTextureGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ILandscapeTexture, ILandscapeTextureGetter>(state.LinkCache)
                .ToArray();
        }
    }
}

using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.EffectShader;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: EFSH texture fields share serialized asset-path comparison and copying behavior.
    // - Kept specialized: EffectShaderData remains one typed aggregate handler for its coupled DATA payload.
    // - Intentionally excluded: DATADataTypeState is serializer layout state, not an editable semantic field.
    public class EffectShaderRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "FillTexture", new FillTextureHandler() },
            { "ParticleShaderTexture", new ParticleShaderTextureHandler() },
            { "HolesTexture", new HolesTextureHandler() },
            { "MembranePaletteTexture", new MembranePaletteTextureHandler() },
            { "ParticlePaletteTexture", new ParticlePaletteTextureHandler() },
            { "EffectShaderData", new EffectShaderDataHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IEffectShaderGetter effectShaderRecord)
            {
                throw new InvalidOperationException($"Expected IEffectShaderGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = effectShaderRecord
                .ToLink<IEffectShaderGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IEffectShader, IEffectShaderGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}

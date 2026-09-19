using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim.Assets;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;

namespace DreadsMashedPatch.RecordHandlers;

// Migration note:
// - Generalized: SPGD scalar, vector, asset-link, and enum fields use existing handlers.
// - Kept specialized: none.
// - Intentionally excluded: DATADataTypeState is Mutagen serialization state, not an xEdit field.
// - Rationale: semantic fields are forwarded while the winning record retains its binary DATA layout.
public class ShaderParticleGeometryRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "GravityVelocity", new SimpleReflectionPropertyHandler<float, IShaderParticleGeometry, IShaderParticleGeometryGetter>("GravityVelocity") },
        { "RotationVelocity", new SimpleReflectionPropertyHandler<float, IShaderParticleGeometry, IShaderParticleGeometryGetter>("RotationVelocity") },
        { "ParticleSizeX", new SimpleReflectionPropertyHandler<float, IShaderParticleGeometry, IShaderParticleGeometryGetter>("ParticleSizeX") },
        { "ParticleSizeY", new SimpleReflectionPropertyHandler<float, IShaderParticleGeometry, IShaderParticleGeometryGetter>("ParticleSizeY") },
        { "CenterOffsetMin", new SimpleReflectionPropertyHandler<float, IShaderParticleGeometry, IShaderParticleGeometryGetter>("CenterOffsetMin") },
        { "CenterOffsetMax", new SimpleReflectionPropertyHandler<float, IShaderParticleGeometry, IShaderParticleGeometryGetter>("CenterOffsetMax") },
        { "InitialRotationRange", new SimpleReflectionPropertyHandler<float, IShaderParticleGeometry, IShaderParticleGeometryGetter>("InitialRotationRange") },
        { "NumSubtexturesX", new SimpleReflectionPropertyHandler<uint, IShaderParticleGeometry, IShaderParticleGeometryGetter>("NumSubtexturesX") },
        { "NumSubtexturesY", new SimpleReflectionPropertyHandler<uint, IShaderParticleGeometry, IShaderParticleGeometryGetter>("NumSubtexturesY") },
        { "Type", new SimpleReflectionPropertyHandler<ShaderParticleGeometry.TypeEnum, IShaderParticleGeometry, IShaderParticleGeometryGetter>("Type") },
        { "BoxSize", new SimpleReflectionPropertyHandler<uint, IShaderParticleGeometry, IShaderParticleGeometryGetter>("BoxSize") },
        { "ParticleDensity", new SimpleReflectionPropertyHandler<float, IShaderParticleGeometry, IShaderParticleGeometryGetter>("ParticleDensity") },
        { "ParticleTexture", new SimpleReflectionAssetLinkPropertyHandler<SkyrimTextureAssetType, IShaderParticleGeometry, IShaderParticleGeometryGetter>("ParticleTexture") },
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IShaderParticleGeometryGetter shaderParticleGeometryRecord)
        {
            throw new InvalidOperationException($"Expected IShaderParticleGeometryGetter but got {winningContext.Record.GetType()}");
        }

        return shaderParticleGeometryRecord
            .ToLink<IShaderParticleGeometryGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IShaderParticleGeometry, IShaderParticleGeometryGetter>(state.LinkCache)
            .ToArray();
    }
}

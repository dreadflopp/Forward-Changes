using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers;

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
        { "ParticleTexture", new SimpleReflectionPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>?, IShaderParticleGeometry, IShaderParticleGeometryGetter>("ParticleTexture") },
        { "DATADataTypeState", new SimpleReflectionPropertyHandler<ShaderParticleGeometry.DATADataType, IShaderParticleGeometry, IShaderParticleGeometryGetter>("DATADataTypeState") }
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
using System;
using System.Collections.Generic;
using System.Drawing;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;
using Noggog;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: WATR scalar/binary/link fields via reflection handlers.
// - Kept specialized: none.
// - Rationale: mutable surface is broad but directly representable with existing handler set.
public class WaterRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Name", new NameHandler() },
        { "UnusedNoisemaps", new SimpleReflectionListPropertyHandler<string, IWater, IWaterGetter>("UnusedNoisemaps", ListOrdering.None) },
        { "Opacity", new SimpleReflectionPropertyHandler<byte, IWater, IWaterGetter>("Opacity") },
        { "Flags", new SimpleReflectionPropertyHandler<Water.Flag?, IWater, IWaterGetter>("Flags") },
        { "MNAM", new SimpleReflectionBinaryDataPropertyHandler<IWater, IWaterGetter>("MNAM") },
        { "Material", new SimpleReflectionFormLinkPropertyHandler<IMaterialTypeGetter, IWater, IWaterGetter>("Material") },
        { "OpenSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IWater, IWaterGetter>("OpenSound") },
        { "Spell", new SimpleReflectionFormLinkPropertyHandler<ISpellGetter, IWater, IWaterGetter>("Spell") },
        { "ImageSpace", new SimpleReflectionFormLinkPropertyHandler<IImageSpaceGetter, IWater, IWaterGetter>("ImageSpace") },
        { "DamagePerSecond", new SimpleReflectionPropertyHandler<ushort?, IWater, IWaterGetter>("DamagePerSecond") },
        { "Unknown", new SimpleReflectionBinaryDataPropertyHandler<IWater, IWaterGetter>("Unknown") },
        { "SpecularSunPower", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("SpecularSunPower") },
        { "WaterReflectivity", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("WaterReflectivity") },
        { "WaterFresnel", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("WaterFresnel") },
        { "Unknown2", new SimpleReflectionPropertyHandler<int, IWater, IWaterGetter>("Unknown2") },
        { "FogAboveWaterDistanceNearPlane", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("FogAboveWaterDistanceNearPlane") },
        { "FogAboveWaterDistanceFarPlane", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("FogAboveWaterDistanceFarPlane") },
        { "ShallowColor", new SimpleReflectionPropertyHandler<Color, IWater, IWaterGetter>("ShallowColor") },
        { "DeepColor", new SimpleReflectionPropertyHandler<Color, IWater, IWaterGetter>("DeepColor") },
        { "ReflectionColor", new SimpleReflectionPropertyHandler<Color, IWater, IWaterGetter>("ReflectionColor") },
        { "Unknown3", new SimpleReflectionBinaryDataPropertyHandler<IWater, IWaterGetter>("Unknown3") },
        { "DisplacementStartingSize", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("DisplacementStartingSize") },
        { "DisplacementFoce", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("DisplacementFoce") },
        { "DisplacementVelocity", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("DisplacementVelocity") },
        { "DisplacementFalloff", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("DisplacementFalloff") },
        { "DisplacementDampner", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("DisplacementDampner") },
        { "Unknown4", new SimpleReflectionPropertyHandler<int, IWater, IWaterGetter>("Unknown4") },
        { "NoiseFalloff", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("NoiseFalloff") },
        { "NoiseLayerOneWindDirection", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("NoiseLayerOneWindDirection") },
        { "NoiseLayerTwoWindDirection", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("NoiseLayerTwoWindDirection") },
        { "NoiseLayerThreeWindDirection", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("NoiseLayerThreeWindDirection") },
        { "NoiseLayerOneWindSpeed", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("NoiseLayerOneWindSpeed") },
        { "NoiseLayerTwoWindSpeed", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("NoiseLayerTwoWindSpeed") },
        { "NoiseLayerThreeWindSpeed", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("NoiseLayerThreeWindSpeed") },
        { "Unknown5", new SimpleReflectionBinaryDataPropertyHandler<IWater, IWaterGetter>("Unknown5") },
        { "FogAboveWaterAmount", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("FogAboveWaterAmount") },
        { "Unknown6", new SimpleReflectionPropertyHandler<int, IWater, IWaterGetter>("Unknown6") },
        { "FogUnderWaterAmount", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("FogUnderWaterAmount") },
        { "FogUnderWaterDistanceNearPlane", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("FogUnderWaterDistanceNearPlane") },
        { "FogUnderWaterDistanceFarPlane", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("FogUnderWaterDistanceFarPlane") },
        { "WaterRefractionMagnitude", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("WaterRefractionMagnitude") },
        { "SpecularPower", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("SpecularPower") },
        { "Unknown7", new SimpleReflectionPropertyHandler<int, IWater, IWaterGetter>("Unknown7") },
        { "SpecularRadius", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("SpecularRadius") },
        { "SpecularBrightness", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("SpecularBrightness") },
        { "NoiseLayerOneUvScale", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("NoiseLayerOneUvScale") },
        { "NoiseLayerTwoUvScale", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("NoiseLayerTwoUvScale") },
        { "NoiseLayerThreeUvScale", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("NoiseLayerThreeUvScale") },
        { "NoiseLayerOneAmplitudeScale", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("NoiseLayerOneAmplitudeScale") },
        { "NoiseLayerTwoAmplitudeScale", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("NoiseLayerTwoAmplitudeScale") },
        { "NoiseLayerThreeAmplitudeScale", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("NoiseLayerThreeAmplitudeScale") },
        { "WaterReflectionMagnitude", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("WaterReflectionMagnitude") },
        { "SpecularSunSparkleMagnitude", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("SpecularSunSparkleMagnitude") },
        { "SpecularSunSpecularMagnitude", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("SpecularSunSpecularMagnitude") },
        { "DepthReflections", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("DepthReflections") },
        { "DepthRefraction", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("DepthRefraction") },
        { "DepthNormals", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("DepthNormals") },
        { "DepthSpecularLighting", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("DepthSpecularLighting") },
        { "SpecularSunSparklePower", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("SpecularSunSparklePower") },
        { "NoiseFlowmapScale", new SimpleReflectionPropertyHandler<float, IWater, IWaterGetter>("NoiseFlowmapScale") },
        { "GNAM", new SimpleReflectionBinaryDataPropertyHandler<IWater, IWaterGetter>("GNAM") },
        { "LinearVelocity", new SimpleReflectionPropertyHandler<P3Float?, IWater, IWaterGetter>("LinearVelocity") },
        { "AngularVelocity", new SimpleReflectionPropertyHandler<P3Float?, IWater, IWaterGetter>("AngularVelocity") },
        { "NoiseLayerOneTexture", new SimpleReflectionPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>?, IWater, IWaterGetter>("NoiseLayerOneTexture") },
        { "NoiseLayerTwoTexture", new SimpleReflectionPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>?, IWater, IWaterGetter>("NoiseLayerTwoTexture") },
        { "NoiseLayerThreeTexture", new SimpleReflectionPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>?, IWater, IWaterGetter>("NoiseLayerThreeTexture") },
        { "FlowNormalsNoiseTexture", new SimpleReflectionPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>?, IWater, IWaterGetter>("FlowNormalsNoiseTexture") },
        { "DNAMDataTypeState", new SimpleReflectionPropertyHandler<Water.DNAMDataType, IWater, IWaterGetter>("DNAMDataTypeState") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IWaterGetter waterRecord)
        {
            throw new InvalidOperationException($"Expected IWaterGetter but got {winningContext.Record.GetType()}");
        }

        return waterRecord
            .ToLink<IWaterGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IWater, IWaterGetter>(state.LinkCache)
            .ToArray();
    }
}
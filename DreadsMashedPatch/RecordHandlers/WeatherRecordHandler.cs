using System;
using System.Collections.Generic;
using System.Drawing;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.PropertyHandlers.Weather;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using Noggog;

namespace DreadsMashedPatch.RecordHandlers;

// Migration note:
// - Generalized: WTHR scalar/binary/link fields plus CloudTextures/Clouds arrays via dedicated handlers.
// - Kept specialized: generated Weather aggregate copying preserves indexed TimeOfDay members without reflecting over indexers.
// - Intentionally excluded: NAM0DataTypeState is serialization state; Unknown is outside the semantic conflict surface.
// - Rationale: semantic fields are forwarded while the winning record retains its binary NAM0 layout.
public class WeatherRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "DNAM", new SimpleReflectionBinaryDataPropertyHandler<IWeather, IWeatherGetter>("DNAM") },
        { "CNAM", new SimpleReflectionBinaryDataPropertyHandler<IWeather, IWeatherGetter>("CNAM") },
        { "ANAM", new SimpleReflectionBinaryDataPropertyHandler<IWeather, IWeatherGetter>("ANAM") },
        { "BNAM", new SimpleReflectionBinaryDataPropertyHandler<IWeather, IWeatherGetter>("BNAM") },
        { "LNAM", new SimpleReflectionBinaryDataPropertyHandler<IWeather, IWeatherGetter>("LNAM") },
        { "Precipitation", new SimpleReflectionFormLinkPropertyHandler<IShaderParticleGeometryGetter, IWeather, IWeatherGetter>("Precipitation") },
        { "VisualEffect", new SimpleReflectionFormLinkPropertyHandler<IVisualEffectGetter, IWeather, IWeatherGetter>("VisualEffect") },
        { "ONAM", new SimpleReflectionBinaryDataPropertyHandler<IWeather, IWeatherGetter>("ONAM") },
        { "CloudTextures", new CloudTexturesHandler() },
        { "Clouds", new CloudLayersHandler() },
        { "SkyUpperColor", WeatherColorHandler("SkyUpperColor") },
        { "FogNearColor", WeatherColorHandler("FogNearColor") },
        { "UnknownColor", WeatherColorHandler("UnknownColor") },
        { "AmbientColor", WeatherColorHandler("AmbientColor") },
        { "SunlightColor", WeatherColorHandler("SunlightColor") },
        { "SunColor", WeatherColorHandler("SunColor") },
        { "StarsColor", WeatherColorHandler("StarsColor") },
        { "SkyLowerColor", WeatherColorHandler("SkyLowerColor") },
        { "HorizonColor", WeatherColorHandler("HorizonColor") },
        { "EffectLightingColor", WeatherColorHandler("EffectLightingColor") },
        { "CloudLodDiffuseColor", WeatherColorHandler("CloudLodDiffuseColor") },
        { "CloudLodAmbientColor", WeatherColorHandler("CloudLodAmbientColor") },
        { "FogFarColor", WeatherColorHandler("FogFarColor") },
        { "SkyStaticsColor", WeatherColorHandler("SkyStaticsColor") },
        { "WaterMultiplierColor", WeatherColorHandler("WaterMultiplierColor") },
        { "SunGlareColor", WeatherColorHandler("SunGlareColor") },
        { "MoonGlareColor", WeatherColorHandler("MoonGlareColor") },
        { "FogDistanceDayNear", new SimpleReflectionPropertyHandler<float, IWeather, IWeatherGetter>("FogDistanceDayNear") },
        { "FogDistanceDayFar", new SimpleReflectionPropertyHandler<float, IWeather, IWeatherGetter>("FogDistanceDayFar") },
        { "FogDistanceNightNear", new SimpleReflectionPropertyHandler<float, IWeather, IWeatherGetter>("FogDistanceNightNear") },
        { "FogDistanceNightFar", new SimpleReflectionPropertyHandler<float, IWeather, IWeatherGetter>("FogDistanceNightFar") },
        { "FogDistanceDayPower", new SimpleReflectionPropertyHandler<float, IWeather, IWeatherGetter>("FogDistanceDayPower") },
        { "FogDistanceNightPower", new SimpleReflectionPropertyHandler<float, IWeather, IWeatherGetter>("FogDistanceNightPower") },
        { "FogDistanceDayMax", new SimpleReflectionPropertyHandler<float, IWeather, IWeatherGetter>("FogDistanceDayMax") },
        { "FogDistanceNightMax", new SimpleReflectionPropertyHandler<float, IWeather, IWeatherGetter>("FogDistanceNightMax") },
        { "WindSpeed", new SimpleReflectionPropertyHandler<Percent, IWeather, IWeatherGetter>("WindSpeed") },
        { "TransDelta", new SimpleReflectionPropertyHandler<float, IWeather, IWeatherGetter>("TransDelta") },
        { "SunGlare", new SimpleReflectionPropertyHandler<Percent, IWeather, IWeatherGetter>("SunGlare") },
        { "SunDamage", new SimpleReflectionPropertyHandler<Percent, IWeather, IWeatherGetter>("SunDamage") },
        { "PrecipitationBeginFadeIn", new SimpleReflectionPropertyHandler<Percent, IWeather, IWeatherGetter>("PrecipitationBeginFadeIn") },
        { "PrecipitationEndFadeOut", new SimpleReflectionPropertyHandler<Percent, IWeather, IWeatherGetter>("PrecipitationEndFadeOut") },
        { "ThunderLightningBeginFadeIn", new SimpleReflectionPropertyHandler<Percent, IWeather, IWeatherGetter>("ThunderLightningBeginFadeIn") },
        { "ThunderLightningEndFadeOut", new SimpleReflectionPropertyHandler<Percent, IWeather, IWeatherGetter>("ThunderLightningEndFadeOut") },
        { "ThunderLightningFrequency", new SimpleReflectionPropertyHandler<Percent, IWeather, IWeatherGetter>("ThunderLightningFrequency") },
        { "Flags", new SimpleReflectionFlagPropertyHandler<Weather.Flag, IWeather, IWeatherGetter>("Flags") },
        { "LightningColor", new SimpleReflectionPropertyHandler<Color, IWeather, IWeatherGetter>("LightningColor") },
        { "VisualEffectBegin", new SimpleReflectionPropertyHandler<Percent, IWeather, IWeatherGetter>("VisualEffectBegin") },
        { "VisualEffectEnd", new SimpleReflectionPropertyHandler<Percent, IWeather, IWeatherGetter>("VisualEffectEnd") },
        { "WindDirection", new SimpleReflectionPropertyHandler<float, IWeather, IWeatherGetter>("WindDirection") },
        { "WindDirectionRange", new SimpleReflectionPropertyHandler<float, IWeather, IWeatherGetter>("WindDirectionRange") },
        { "Sounds", new SimpleReflectionListPropertyHandler<IWeatherSoundGetter, IWeather, IWeatherGetter>("Sounds", ListSemantics.SortedKeyed, keySelector: sound => sound.Type) },
        { "SkyStatics", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IStaticGetter>, IWeather, IWeatherGetter>("SkyStatics", ListSemantics.SortedKeyed) },
        { "ImageSpaces", new GeneratedCopyReflectionPropertyHandler<IWeatherImageSpacesGetter, WeatherImageSpaces, IWeather, IWeatherGetter>("ImageSpaces", value => value.DeepCopy(), WeatherImageSpacesMixIn.Equals) },
        { "VolumetricLighting", new GeneratedCopyReflectionPropertyHandler<IWeatherVolumetricLightingGetter, WeatherVolumetricLighting, IWeather, IWeatherGetter>("VolumetricLighting", value => value.DeepCopy(), WeatherVolumetricLightingMixIn.Equals) },
        { "DirectionalAmbientLightingColors", new GeneratedCopyReflectionPropertyHandler<IWeatherAmbientColorSetGetter, WeatherAmbientColorSet, IWeather, IWeatherGetter>("DirectionalAmbientLightingColors", value => value.DeepCopy(), WeatherAmbientColorSetMixIn.Equals) },
        { "NAM2", new SimpleReflectionBinaryDataPropertyHandler<IWeather, IWeatherGetter>("NAM2") },
        { "NAM3", new SimpleReflectionBinaryDataPropertyHandler<IWeather, IWeatherGetter>("NAM3") },
        { "Aurora", new SimpleReflectionModelPropertyHandler<IWeather, IWeatherGetter>("Aurora") },
        { "SunGlareLensFlare", new SimpleReflectionFormLinkPropertyHandler<ILensFlareGetter, IWeather, IWeatherGetter>("SunGlareLensFlare") },
    };

    private static GeneratedCopyReflectionPropertyHandler<IWeatherColorGetter, WeatherColor, IWeather, IWeatherGetter>
        WeatherColorHandler(string propertyName) =>
        new(propertyName, value => value.DeepCopy(), WeatherColorMixIn.Equals);

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IWeatherGetter weatherRecord)
        {
            throw new InvalidOperationException($"Expected IWeatherGetter but got {winningContext.Record.GetType()}");
        }

        return weatherRecord
            .ToLink<IWeatherGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IWeather, IWeatherGetter>(state.LinkCache)
            .ToArray();
    }
}

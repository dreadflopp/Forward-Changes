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
// - Generalized: WTHR scalar/binary/link/complex fields plus CloudTextures/Clouds arrays via dedicated handlers.
// - Kept specialized: none.
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
        { "SkyUpperColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("SkyUpperColor") },
        { "FogNearColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("FogNearColor") },
        { "UnknownColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("UnknownColor") },
        { "AmbientColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("AmbientColor") },
        { "SunlightColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("SunlightColor") },
        { "SunColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("SunColor") },
        { "StarsColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("StarsColor") },
        { "SkyLowerColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("SkyLowerColor") },
        { "HorizonColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("HorizonColor") },
        { "EffectLightingColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("EffectLightingColor") },
        { "CloudLodDiffuseColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("CloudLodDiffuseColor") },
        { "CloudLodAmbientColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("CloudLodAmbientColor") },
        { "FogFarColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("FogFarColor") },
        { "SkyStaticsColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("SkyStaticsColor") },
        { "WaterMultiplierColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("WaterMultiplierColor") },
        { "SunGlareColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("SunGlareColor") },
        { "MoonGlareColor", new ComplexReflectionPropertyHandler<IWeatherColorGetter, IWeather, IWeatherGetter>("MoonGlareColor") },
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
        { "ImageSpaces", new ComplexReflectionPropertyHandler<IWeatherImageSpacesGetter, IWeather, IWeatherGetter>("ImageSpaces") },
        { "VolumetricLighting", new ComplexReflectionPropertyHandler<IWeatherVolumetricLightingGetter, IWeather, IWeatherGetter>("VolumetricLighting") },
        { "DirectionalAmbientLightingColors", new ComplexReflectionPropertyHandler<IWeatherAmbientColorSetGetter, IWeather, IWeatherGetter>("DirectionalAmbientLightingColors") },
        { "NAM2", new SimpleReflectionBinaryDataPropertyHandler<IWeather, IWeatherGetter>("NAM2") },
        { "NAM3", new SimpleReflectionBinaryDataPropertyHandler<IWeather, IWeatherGetter>("NAM3") },
        { "Aurora", new SimpleReflectionModelPropertyHandler<IWeather, IWeatherGetter>("Aurora") },
        { "SunGlareLensFlare", new SimpleReflectionFormLinkPropertyHandler<ILensFlareGetter, IWeather, IWeatherGetter>("SunGlareLensFlare") },
    };

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

using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Climate;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: times, volatility, moons, phase length via reflection handlers.
    // - Kept specialized: WeatherTypes list + sun textures + model via dedicated/shared handlers.
    // - Rationale: preserve complex list/asset semantics while keeping scalar fields simple.
    public class ClimateRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "WeatherTypes", new WeatherTypesHandler() },
            { "SunTexture", new SunTextureHandler() },
            { "SunGlareTexture", new SunGlareTextureHandler() },
            { "Model", new ModelHandler() },
            { "SunriseBegin", new SimpleReflectionPropertyHandler<TimeOnly, IClimate, IClimateGetter>("SunriseBegin") },
            { "SunriseEnd", new SimpleReflectionPropertyHandler<TimeOnly, IClimate, IClimateGetter>("SunriseEnd") },
            { "SunsetBegin", new SimpleReflectionPropertyHandler<TimeOnly, IClimate, IClimateGetter>("SunsetBegin") },
            { "SunsetEnd", new SimpleReflectionPropertyHandler<TimeOnly, IClimate, IClimateGetter>("SunsetEnd") },
            { "Volatility", new SimpleReflectionPropertyHandler<byte, IClimate, IClimateGetter>("Volatility") },
            { "Moons", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.Climate.Moon, IClimate, IClimateGetter>("Moons") },
            { "PhaseLength", new SimpleReflectionPropertyHandler<byte, IClimate, IClimateGetter>("PhaseLength") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IClimateGetter climate)
            {
                throw new InvalidOperationException($"Expected IClimateGetter but got {winningContext.Record.GetType()}");
            }

            return climate
                .ToLink<IClimateGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IClimate, IClimateGetter>(state.LinkCache)
                .ToArray();
        }
    }
}

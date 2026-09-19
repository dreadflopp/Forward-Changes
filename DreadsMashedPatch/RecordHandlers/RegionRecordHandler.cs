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
using DreadsMashedPatch.PropertyHandlers.Region;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers;

// Migration note:
// - Generalized: REGN map/worldspace and non-area region substructures via reflection handlers.
// - Kept specialized: Region Areas are an atomic ordered array whose nested polygon points require
//   typed copying and xEdit's direction normalization.
// - Rationale: generic reflection cannot assign overlay IReadOnlyList<P2Float> values to Mutagen's
//   mutable ExtendedList<P2Float>, and xEdit normalizes reversed polygon point sequences after load.
public class RegionRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "MapColor", new SimpleReflectionPropertyHandler<Color?, IRegion, IRegionGetter>("MapColor") },
        { "Worldspace", new SimpleReflectionFormLinkPropertyHandler<IWorldspaceGetter, IRegion, IRegionGetter>("Worldspace") },
        { "RegionAreas", new RegionAreasHandler() },
        { "Objects", new GeneratedCopyReflectionPropertyHandler<IRegionObjectsGetter, RegionObjects, IRegion, IRegionGetter>("Objects", value => value.DeepCopy(), (left, right) => left.Equals(right)) },
        { "Weather", new GeneratedCopyReflectionPropertyHandler<IRegionWeatherGetter, RegionWeather, IRegion, IRegionGetter>("Weather", value => value.DeepCopy(), (left, right) => left.Equals(right)) },
        { "Map", new GeneratedCopyReflectionPropertyHandler<IRegionMapGetter, RegionMap, IRegion, IRegionGetter>("Map", value => value.DeepCopy(), (left, right) => left.Equals(right)) },
        { "Land", new GeneratedCopyReflectionPropertyHandler<IRegionLandGetter, RegionLand, IRegion, IRegionGetter>("Land", value => value.DeepCopy(), (left, right) => left.Equals(right)) },
        { "Grasses", new GeneratedCopyReflectionPropertyHandler<IRegionGrassesGetter, RegionGrasses, IRegion, IRegionGetter>("Grasses", value => value.DeepCopy(), (left, right) => left.Equals(right)) },
        { "Sounds", new GeneratedCopyReflectionPropertyHandler<IRegionSoundsGetter, RegionSounds, IRegion, IRegionGetter>("Sounds", value => value.DeepCopy(), (left, right) => left.Equals(right)) },
        { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Region.MajorFlag, IRegion, IRegionGetter>("MajorFlags") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IRegionGetter regionRecord)
        {
            throw new InvalidOperationException($"Expected IRegionGetter but got {winningContext.Record.GetType()}");
        }

        return regionRecord
            .ToLink<IRegionGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IRegion, IRegionGetter>(state.LinkCache)
            .ToArray();
    }
}

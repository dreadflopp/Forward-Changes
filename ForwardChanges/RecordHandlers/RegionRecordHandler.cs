using System;
using System.Collections.Generic;
using System.Drawing;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: REGN map/worldspace/region substructures via reflection handlers.
// - Kept specialized: none.
// - Rationale: mutable surface maps directly to existing list/complex/form-link handlers.
public class RegionRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "MapColor", new SimpleReflectionPropertyHandler<Color?, IRegion, IRegionGetter>("MapColor") },
        { "Worldspace", new SimpleReflectionFormLinkPropertyHandler<IWorldspaceGetter, IRegion, IRegionGetter>("Worldspace") },
        { "RegionAreas", new SimpleReflectionListPropertyHandler<IRegionAreaGetter, IRegion, IRegionGetter>("RegionAreas", ListOrdering.None) },
        { "Objects", new ComplexReflectionPropertyHandler<IRegionObjectsGetter, IRegion, IRegionGetter>("Objects") },
        { "Weather", new ComplexReflectionPropertyHandler<IRegionWeatherGetter, IRegion, IRegionGetter>("Weather") },
        { "Map", new ComplexReflectionPropertyHandler<IRegionMapGetter, IRegion, IRegionGetter>("Map") },
        { "Land", new ComplexReflectionPropertyHandler<IRegionLandGetter, IRegion, IRegionGetter>("Land") },
        { "Grasses", new ComplexReflectionPropertyHandler<IRegionGrassesGetter, IRegion, IRegionGetter>("Grasses") },
        { "Sounds", new ComplexReflectionPropertyHandler<IRegionSoundsGetter, IRegion, IRegionGetter>("Sounds") },
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
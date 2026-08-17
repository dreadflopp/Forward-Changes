using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Worldspace;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers
{
    public class WorldspaceRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "MajorFlags", new MajorFlagsHandler() },
            { "Name", new NameHandler() },
            { "MaxHeight", new ComplexReflectionPropertyHandler<IWorldspaceMaxHeightGetter, IWorldspace, IWorldspaceGetter>("MaxHeight") },
            { "Location", new SimpleReflectionFormLinkPropertyHandler<ILocationGetter, IWorldspace, IWorldspaceGetter>("Location") },
            { "Water", new SimpleReflectionFormLinkPropertyHandler<IWaterGetter, IWorldspace, IWorldspaceGetter>("Water") },
            { "LodWater", new SimpleReflectionFormLinkPropertyHandler<IWaterGetter, IWorldspace, IWorldspaceGetter>("LodWater") },
            { "LodWaterHeight", new SimpleReflectionPropertyHandler<float?, IWorldspace, IWorldspaceGetter>("LodWaterHeight") },
            { "Music", new SimpleReflectionFormLinkPropertyHandler<IMusicTypeGetter, IWorldspace, IWorldspaceGetter>("Music") },
            { "ObjectBoundsMin", new SimpleReflectionPropertyHandler<P2Float, IWorldspace, IWorldspaceGetter>("ObjectBoundsMin") },
            { "ObjectBoundsMax", new SimpleReflectionPropertyHandler<P2Float, IWorldspace, IWorldspaceGetter>("ObjectBoundsMax") },
            { "MapData", new ComplexReflectionPropertyHandler<IWorldspaceMapGetter, IWorldspace, IWorldspaceGetter>("MapData") },
            { "MapImage", new MapImageHandler() },
            { "CloudModel", new CloudModelHandler() },
            { "Flags", new FlagsHandler() },
            { "WorldMapOffsetScale", new SimpleReflectionPropertyHandler<float, IWorldspace, IWorldspaceGetter>("WorldMapOffsetScale") },
            { "WorldMapCellOffset", new WorldMapCellOffsetHandler() },
            { "DistantLodMultiplier", new SimpleReflectionPropertyHandler<float?, IWorldspace, IWorldspaceGetter>("DistantLodMultiplier") },
            { "FixedDimensionsCenterCell", new SimpleReflectionPropertyHandler<P2Int16?, IWorldspace, IWorldspaceGetter>("FixedDimensionsCenterCell") },
            { "InteriorLighting", new SimpleReflectionFormLinkPropertyHandler<ILightingTemplateGetter, IWorldspace, IWorldspaceGetter>("InteriorLighting") },
            { "EncounterZone", new SimpleReflectionFormLinkPropertyHandler<IEncounterZoneGetter, IWorldspace, IWorldspaceGetter>("EncounterZone") },
            { "Parent", new ComplexReflectionPropertyHandler<IWorldspaceParentGetter, IWorldspace, IWorldspaceGetter>("Parent") },
            { "Climate", new SimpleReflectionFormLinkPropertyHandler<IClimateGetter, IWorldspace, IWorldspaceGetter>("Climate") },
            { "LandDefaults", new ComplexReflectionPropertyHandler<IWorldspaceLandDefaultsGetter, IWorldspace, IWorldspaceGetter>("LandDefaults") },
            { "CanopyShadow", new CanopyShadowHandler() },
            { "WaterNoiseTexture", new WaterNoiseTextureHandler() },
            { "HdLodDiffuseTexture", new HdLodDiffuseTextureHandler() },
            { "HdLodNormalTexture", new HdLodNormalTextureHandler() },
            { "WaterEnvironmentMap", new WaterEnvironmentMapHandler() },
        };


        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IWorldspaceGetter worldspaceRecord)
            {
                throw new InvalidOperationException($"Expected IWorldspaceGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = worldspaceRecord
                .ToLink<IWorldspaceGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IWorldspace, IWorldspaceGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
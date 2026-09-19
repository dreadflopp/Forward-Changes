using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Worldspace;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: independent links, assets, translated name, and flags use shared semantic handlers.
    // - Kept specialized: LOD Data, World Map Offset, and Distant LOD use WRLD-aware aggregate/default handlers.
    // - Intentionally excluded: MHDT MaxHeight is generated/no-copy height data; NNAM CanopyShadow is unused/cpIgnore;
    //   object bounds and WRLD child-group/runtime data remain owned by the winning override.
    // - Coupled forwarding: parent inheritance fields and fixed-dimension controls establish complete-record
    //   ownership boundaries so null local values cannot be combined with incompatible inheritance flags.
    // - Rationale: xEdit models these as removable/required structures, and generated Mutagen copies are required
    //   to preserve overlay form links and aggregate payloads.
    public class WorldspaceRecordHandler : AbstractRecordHandler
    {
        private static readonly IReadOnlySet<string> StructuralPropertyNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "Parent",
            "Climate",
            "Water",
            "LodData",
            "LandDefaults",
            "MapData",
            "Flags",
            "FixedDimensionsCenterCell"
        };

        protected override IReadOnlySet<string> AtomicOwnershipTriggerProperties => StructuralPropertyNames;

        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler(typeof(SkyrimMajorRecord.SkyrimMajorRecordFlag), typeof(Worldspace.MajorFlag)) },
            { "Name", new NameHandler() },
            { "Location", new SimpleReflectionFormLinkPropertyHandler<ILocationGetter, IWorldspace, IWorldspaceGetter>("Location") },
            { "Water", new SimpleReflectionFormLinkPropertyHandler<IWaterGetter, IWorldspace, IWorldspaceGetter>("Water") },
            { "LodData", new LodDataHandler() },
            { "Music", new SimpleReflectionFormLinkPropertyHandler<IMusicTypeGetter, IWorldspace, IWorldspaceGetter>("Music") },
            { "MapData", new GeneratedCopyReflectionPropertyHandler<IWorldspaceMapGetter, WorldspaceMap, IWorldspace, IWorldspaceGetter>("MapData", value => value.DeepCopy(), WorldspaceMapMixIn.Equals) },
            { "MapImage", new MapImageHandler() },
            { "CloudModel", new CloudModelHandler() },
            { "Flags", new SimpleReflectionFlagPropertyHandler<Worldspace.Flag, IWorldspace, IWorldspaceGetter>("Flags", preserveUnknownBits: true) },
            { "WorldMapOffset", new WorldMapOffsetHandler() },
            { "DistantLodMultiplier", new DistantLodMultiplierHandler() },
            { "FixedDimensionsCenterCell", new SimpleReflectionPropertyHandler<P2Int16?, IWorldspace, IWorldspaceGetter>("FixedDimensionsCenterCell") },
            { "InteriorLighting", new SimpleReflectionFormLinkPropertyHandler<ILightingTemplateGetter, IWorldspace, IWorldspaceGetter>("InteriorLighting") },
            { "EncounterZone", new SimpleReflectionFormLinkPropertyHandler<IEncounterZoneGetter, IWorldspace, IWorldspaceGetter>("EncounterZone") },
            { "Parent", new GeneratedCopyReflectionPropertyHandler<IWorldspaceParentGetter, WorldspaceParent, IWorldspace, IWorldspaceGetter>("Parent", value => value.DeepCopy(), WorldspaceParentMixIn.Equals) },
            { "Climate", new SimpleReflectionFormLinkPropertyHandler<IClimateGetter, IWorldspace, IWorldspaceGetter>("Climate") },
            { "LandDefaults", new GeneratedCopyReflectionPropertyHandler<IWorldspaceLandDefaultsGetter, WorldspaceLandDefaults, IWorldspace, IWorldspaceGetter>("LandDefaults", value => value.DeepCopy(), WorldspaceLandDefaultsMixIn.Equals) },
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

    }
}

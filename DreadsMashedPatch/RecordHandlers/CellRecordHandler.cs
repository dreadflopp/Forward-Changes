using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.PropertyHandlers.Cell;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: semantic CELL fields use shared scalar, flag, link, and list handlers; obsolete one-field
    //   CELL handlers replaced by these registrations were removed.
    // - Kept specialized: lighting, ownership, encounter-zone, and occlusion structures retain typed handlers.
    // - Intentionally excluded: WaterHeight and Landscape are runtime-managed; NavigationMeshes is navigation data.
    // - Rationale: excluded fields remain exactly as authored by the winning override.
    public class CellRecordHandler : AbstractRecordHandler
    {
        private readonly Dictionary<string, IPropertyHandler> _propertyHandlers;

        public CellRecordHandler()
        {
            // Initialize property handlers for Cell records. Uses reflection-based handlers where applicable (ICell / ICellGetter).
            _propertyHandlers = new Dictionary<string, IPropertyHandler>
            {
                { "EditorID", new EditorIDHandler() },
                { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
                { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
                { "Name", new NameHandler() },
                { "Flags", new SimpleReflectionFlagPropertyHandler<Cell.Flag, ICell, ICellGetter>("Flags") },
                { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Cell.MajorFlag, ICell, ICellGetter>("MajorFlags") },
                { "Regions", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IRegionGetter>, ICell, ICellGetter>("Regions", ListSemantics.SortedKeyed) },
                { "Location", new SimpleReflectionFormLinkPropertyHandler<ILocationGetter, ICell, ICellGetter>("Location") },
                { "Owner", new SimpleReflectionFormLinkPropertyHandler<IOwnerGetter, ICell, ICellGetter>("Owner") },
                { "Water", new SimpleReflectionFormLinkPropertyHandler<IWaterGetter, ICell, ICellGetter>("Water") },
                { "Lighting", new LightingHandler() },
                { "LightingTemplate", new SimpleReflectionFormLinkPropertyHandler<ILightingTemplateGetter, ICell, ICellGetter>("LightingTemplate") },
                { "AcousticSpace", new SimpleReflectionFormLinkPropertyHandler<IAcousticSpaceGetter, ICell, ICellGetter>("AcousticSpace") },
                { "EncounterZone", new SimpleReflectionFormLinkPropertyHandler<IEncounterZoneGetter, ICell, ICellGetter>("EncounterZone") },
                { "Music", new SimpleReflectionFormLinkPropertyHandler<IMusicTypeGetter, ICell, ICellGetter>("Music") },
                { "ImageSpace", new SimpleReflectionFormLinkPropertyHandler<IImageSpaceGetter, ICell, ICellGetter>("ImageSpace") },
                { "SkyAndWeatherFromRegion", new SimpleReflectionFormLinkPropertyHandler<IRegionGetter, ICell, ICellGetter>("SkyAndWeatherFromRegion") },
                { "Grid", new GridHandler() },
                { "MaxHeightData", new MaxHeightDataHandler() },
                { "WaterNoiseTexture", new WaterNoiseTextureHandler() },
                { "WaterVelocity", new WaterVelocityHandler() },
                { "XWCN", new WaterCurrentCountHandler() },
                { "XWCS", new WaterCurrentCountOldHandler() },
                { "OcclusionData", new OcclusionDataHandler() },
                { "LNAM", new LNAMHandler() },
                { "FactionRank", new SimpleReflectionPropertyHandler<int?, ICell, ICellGetter>("FactionRank") },
                { "LockList", new SimpleReflectionFormLinkPropertyHandler<ILockListGetter, ICell, ICellGetter>("LockList") },
                { "WaterEnvironmentMap", new WaterEnvironmentMapHandler() },

            };
        }

        public override Dictionary<string, IPropertyHandler> PropertyHandlers => _propertyHandlers;

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ICellGetter cellRecord)
            {
                throw new InvalidOperationException($"Expected ICellGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = cellRecord
                .ToLink<ICellGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}

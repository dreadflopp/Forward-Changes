using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.Cell;
using ForwardChanges.PropertyHandlers.General;

namespace ForwardChanges.RecordHandlers
{
    public class CellRecordHandler : AbstractRecordHandler
    {
        private readonly Dictionary<string, IPropertyHandler> _propertyHandlers;

        public CellRecordHandler()
        {
            // Initialize property handlers for Cell records
            _propertyHandlers = new Dictionary<string, IPropertyHandler>
            {
                { "EditorID", new EditorIDHandler() },
                { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
                { "Name", new NameHandler() },
                { "Flags", new FlagsHandler() },
                { "MajorFlags", new MajorFlagsHandler() },

                { "Regions", new RegionsHandler() },
                { "Location", new LocationHandler() },
                { "Owner", new OwnerHandler() },
                { "Water", new WaterHandler() },
                //{ "WaterHeight", new WaterHeightHandler() },
                { "Lighting", new LightingHandler() },
                { "LightingTemplate", new LightingTemplateHandler() },
                { "AcousticSpace", new AcousticSpaceHandler() },
                { "EncounterZone", new EncounterZoneHandler() },
                { "Music", new MusicHandler() },
                { "ImageSpace", new ImageSpaceHandler() },
                { "SkyAndWeatherFromRegion", new SkyWeatherHandler() },
                { "Grid", new GridHandler() },
                { "MaxHeightData", new MaxHeightDataHandler() },
                { "WaterNoiseTexture", new WaterNoiseTextureHandler() },
                // { "WaterVelocity", new WaterVelocityHandler() }, // WaterVelocity contains complex binary data (MemorySlice<byte>) that is difficult to deep copy and compare properly. For property forwarding scenarios, this complex binary data is typically not needed.
                { "FactionRank", new FactionRankHandler() },
                { "LockList", new LockListHandler() },
                { "WaterEnvironmentMap", new WaterEnvironmentMapHandler() },
                // { "Landscape", new LandscapeHandler() }, // Landscape is a complex SkyrimMajorRecord with many properties (Flags, VertexNormals, VertexHeightMap, VertexColors, Layers, Textures). Deep copying would be very complex and computationally expensive. For property forwarding scenarios, FormKey comparison would be used, but this is not commonly needed.
                // { "NavigationMeshes", new NavigationMeshesHandler() }, // NavigationMesh is a major record with complex properties. Deep copying would be very complex and computationally expensive. For property forwarding scenarios, FormKey comparison would be used, but this is not commonly needed.

            };

            // NOTE: The following binary data properties are not implemented due to ReadOnlyMemorySlice<byte> compilation issues:
            // - OcclusionData (ReadOnlyMemorySlice<byte>?)
            // - LNAM (ReadOnlyMemorySlice<byte>?)
            // - XWCN (ReadOnlyMemorySlice<byte>?)
            // - XWCS (ReadOnlyMemorySlice<byte>?)
            // These properties require proper handling of binary data types that are not commonly used in property forwarding.

            // NOTE: The following internal/technical properties are not implemented as they are not commonly needed for property forwarding:
            // - Timestamp, UnknownGroupData, PersistentTimestamp, PersistentUnknownGroupData, Persistent
            // - TemporaryTimestamp, TemporaryUnknownGroupData, Temporary
            // These properties are typically internal to the game engine and not relevant for mod compatibility.
            // MajorFlags is now implemented above.
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

        public override IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            return winningContext.GetOrAddAsOverride(state.PatchMod);
        }

        public override void ApplyForwardedProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward)
        {
            foreach (var (propertyName, value) in propertiesToForward)
            {
                if (PropertyHandlers.TryGetValue(propertyName, out var handler))
                {
                    try
                    {
                        Console.WriteLine($"[{propertyName}] Applying value: {handler.FormatValue(value)}, Type: {value?.GetType()}");
                        handler.SetValue(record, value);
                    }
                    catch (Exception ex)
                    {
                        // Property doesn't exist on this cell type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on cell {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}
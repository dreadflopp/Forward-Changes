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
                { "SkyAndWeatherFromRegion", new SkyWeatherHandler() }
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
                        if (value != null)
                        {
                            handler.SetValue(record, value);
                        }
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
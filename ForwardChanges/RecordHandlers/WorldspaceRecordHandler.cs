using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Worldspace;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers
{
    public class WorldspaceRecordHandler : AbstractRecordHandler
    {
        private readonly Dictionary<string, IPropertyHandler> _propertyHandlers;

        public WorldspaceRecordHandler()
        {
            // Initialize property handlers for Worldspace records
            _propertyHandlers = new Dictionary<string, IPropertyHandler>
            {
                { "EditorID", new EditorIDPropertyHandler() },
                { "Name", new NamePropertyHandler() },
                { "MaxHeight", new WorldspaceMaxHeightPropertyHandler() },
                { "Location", new WorldspaceLocationPropertyHandler() },
                { "Water", new WorldspaceWaterPropertyHandler() },
                { "LodWater", new WorldspaceLodWaterPropertyHandler() },
                { "Music", new WorldspaceMusicPropertyHandler() },
                { "ObjectBoundsMin", new WorldspaceObjectBoundsMinPropertyHandler() },
                { "ObjectBoundsMax", new WorldspaceObjectBoundsMaxPropertyHandler() },
                { "MapData", new WorldspaceMapDataPropertyHandler() }
            };
        }

        public override Dictionary<string, IPropertyHandler> PropertyHandlers => _propertyHandlers;

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

        public override IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // Use the context's GetOrAddAsOverride method like Fusion does
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
                        // Property doesn't exist on this worldspace type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on worldspace {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}
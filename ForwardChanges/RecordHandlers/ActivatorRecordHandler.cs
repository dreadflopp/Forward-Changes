using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Activator;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers
{
    public class ActivatorRecordHandler : AbstractRecordHandler
    {
        private readonly Dictionary<string, IPropertyHandler> _propertyHandlers;

        public ActivatorRecordHandler()
        {
            // Initialize property handlers for Activator records
            _propertyHandlers = new Dictionary<string, IPropertyHandler>
            {
                { "EditorID", new EditorIDHandler() },
                { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
                { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() },
                { "ObjectBounds", new ObjectBoundsHandler() },
                { "Name", new NameHandler() },
                { "Model", new ModelHandler() },
                { "Destructible", new DestructibleHandler() },
                { "Keywords", new KeywordListHandler() },
                { "MarkerColor", new MarkerColorHandler() },
                { "LoopingSound", new LoopingSoundHandler() },
                { "ActivationSound", new ActivationSoundHandler() },
                { "WaterType", new WaterTypeHandler() },
                { "ActivateTextOverride", new ActivateTextOverrideHandler() },
                { "Flags", new FlagsHandler() },
                { "MajorFlags", new MajorFlagsHandler() },
                { "InteractionKeyword", new InteractionKeywordHandler() }
            };
        }

        public override Dictionary<string, IPropertyHandler> PropertyHandlers => _propertyHandlers;

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IActivatorGetter activatorRecord)
            {
                throw new InvalidOperationException($"Expected IActivatorGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = activatorRecord
                .ToLink<IActivatorGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IActivator, IActivatorGetter>(state.LinkCache)
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
                        // Property doesn't exist on this activator type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on activator {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}

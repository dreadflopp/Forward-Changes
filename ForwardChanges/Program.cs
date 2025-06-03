using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Strings;
using System.Reflection;
using System.Linq;
using Noggog;
using Mutagen.Bethesda.Plugins.Cache;

namespace ForwardChanges
{
    public interface IRecordTypeHandler
    {
        IEnumerable<IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>> GetWinningContexts(IPatcherState<ISkyrimMod, ISkyrimModGetter> state);
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>? GetOriginalContext(IPatcherState<ISkyrimMod, ISkyrimModGetter> state, FormKey formKey);
        IMajorRecord CreateOverride(IPatcherState<ISkyrimMod, ISkyrimModGetter> state, IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context);
        IEnumerable<IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>> GetAllContexts(IPatcherState<ISkyrimMod, ISkyrimModGetter> state, FormKey formKey);
    }

    public class RecordTypeHandler<TMod, TModGetter, TRecord, TRecordGetter> : IRecordTypeHandler
        where TMod : class, IMod, TModGetter, IContextMod<TMod, TModGetter>
        where TModGetter : class, IModGetter
        where TRecord : class, IMajorRecord, TRecordGetter
        where TRecordGetter : class, IMajorRecordGetter
    {
        public Func<IPatcherState<TMod, TModGetter>, IEnumerable<IModContext<TMod, TModGetter, TRecord, TRecordGetter>>> GetWinningContexts { get; }
        public Func<IPatcherState<TMod, TModGetter>, FormKey, IModContext<TMod, TModGetter, TRecord, TRecordGetter>?> GetOriginalContext { get; }
        public Func<IPatcherState<TMod, TModGetter>, IModContext<TMod, TModGetter, TRecord, TRecordGetter>, TRecord> CreateOverride { get; }

        public RecordTypeHandler(
            Func<IPatcherState<TMod, TModGetter>, IEnumerable<IModContext<TMod, TModGetter, TRecord, TRecordGetter>>> getWinningContexts,
            Func<IPatcherState<TMod, TModGetter>, FormKey, IModContext<TMod, TModGetter, TRecord, TRecordGetter>?> getOriginalContext,
            Func<IPatcherState<TMod, TModGetter>, IModContext<TMod, TModGetter, TRecord, TRecordGetter>, TRecord> createOverride)
        {
            GetWinningContexts = getWinningContexts;
            GetOriginalContext = getOriginalContext;
            CreateOverride = createOverride;
        }

        IEnumerable<IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>> IRecordTypeHandler.GetWinningContexts(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            return GetWinningContexts((IPatcherState<TMod, TModGetter>)state)
                .Select(context => new ModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>(
                    context.ModKey,
                    context.Record,
                    (mod, record) => (IMajorRecord)CreateOverride((IPatcherState<TMod, TModGetter>)state, context),
                    (mod, record, edid, formKey) => (IMajorRecord)CreateOverride((IPatcherState<TMod, TModGetter>)state, context),
                    null));
        }

        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>? IRecordTypeHandler.GetOriginalContext(IPatcherState<ISkyrimMod, ISkyrimModGetter> state, FormKey formKey)
        {
            var context = GetOriginalContext((IPatcherState<TMod, TModGetter>)state, formKey);
            if (context == null) return null;

            return new ModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>(
                context.ModKey,
                context.Record,
                (mod, record) => (IMajorRecord)CreateOverride((IPatcherState<TMod, TModGetter>)state, context),
                (mod, record, edid, formKey) => (IMajorRecord)CreateOverride((IPatcherState<TMod, TModGetter>)state, context),
                null);
        }

        IMajorRecord IRecordTypeHandler.CreateOverride(IPatcherState<ISkyrimMod, ISkyrimModGetter> state, IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            return CreateOverride((IPatcherState<TMod, TModGetter>)state, (IModContext<TMod, TModGetter, TRecord, TRecordGetter>)context);
        }

        IEnumerable<IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>> IRecordTypeHandler.GetAllContexts(IPatcherState<ISkyrimMod, ISkyrimModGetter> state, FormKey formKey)
        {
            var formLink = new FormLink<TRecordGetter>(formKey);
            return formLink.ResolveAllContexts<TMod, TModGetter, TRecord, TRecordGetter>((ILinkCache<TMod, TModGetter>)state.LinkCache)
                .Select(context => new ModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>(
                    context.ModKey,
                    context.Record,
                    (mod, record) => (IMajorRecord)CreateOverride((IPatcherState<TMod, TModGetter>)state, context),
                    (mod, record, edid, formKey) => (IMajorRecord)CreateOverride((IPatcherState<TMod, TModGetter>)state, context),
                    null));
        }
    }

    public class NpcRecordTypeHandler : RecordTypeHandler<ISkyrimMod, ISkyrimModGetter, INpc, INpcGetter>
    {
        public NpcRecordTypeHandler() : base(
            // Get all winning contexts for this type
            state => state.LoadOrder.PriorityOrder.Npc().WinningContextOverrides(),

            // get original context
            (state, formKey) =>
            {
                var formLink = new FormLink<INpcGetter>(formKey);
                return formLink.ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, INpc, INpcGetter>(state.LinkCache).LastOrDefault();
            },

            // create override record
            (state, context) => context.GetOrAddAsOverride(state.PatchMod))
        {
        }
    }

    public class WeaponRecordTypeHandler : RecordTypeHandler<ISkyrimMod, ISkyrimModGetter, IWeapon, IWeaponGetter>
    {
        public WeaponRecordTypeHandler() : base(
            // Get all winning contexts for this type
            state => state.LoadOrder.PriorityOrder.Weapon().WinningContextOverrides(),

            // get original context
            (state, formKey) =>
            {
                var formLink = new FormLink<IWeaponGetter>(formKey);
                return formLink.ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IWeapon, IWeaponGetter>(state.LinkCache).LastOrDefault();
            },

            // create override record
            (state, context) => context.GetOrAddAsOverride(state.PatchMod))
        {
        }
    }

    public class CellRecordTypeHandler : RecordTypeHandler<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>
    {
        public CellRecordTypeHandler() : base(
            // Get all winning contexts for this type
            state => state.LoadOrder.PriorityOrder.Cell().WinningContextOverrides(state.LinkCache),

            // get original context
            (state, formKey) =>
            {
                var formLink = new FormLink<ICellGetter>(formKey);
                return formLink.ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>(state.LinkCache).LastOrDefault();
            },

            // create override record
            (state, context) => context.GetOrAddAsOverride(state.PatchMod))
        {
        }
    }

    public class Program
    {
        public static readonly Dictionary<Type, IRecordTypeHandler> recordTypes = new()
        {
            { typeof(INpcGetter), new NpcRecordTypeHandler() },
            { typeof(IWeaponGetter), new WeaponRecordTypeHandler() },
            { typeof(ICellGetter), new CellRecordTypeHandler() }
        };

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "YourPatcher.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var propertyHandlers = new Dictionary<string, IPropertyHandler>
            {
                { "EditorID", new SimplePropertyHandler() },
                { "Name", new SimplePropertyHandler() },
                { "Location", new SimplePropertyHandler() },
                { "Water", new SimplePropertyHandler() },
                { "MaxHeightData", new SimplePropertyHandler() },
                { "WaterHeight", new SimplePropertyHandler() },
                { "Flags", new SimplePropertyHandler() },
                { "Scale", new SimplePropertyHandler() },
                { "Base", new SimplePropertyHandler() },
                { "Placement.Position", new NestedPropertyHandler("Placement", "Position") },
                { "Placement.Rotation", new NestedPropertyHandler("Placement", "Rotation") }
            };

            foreach (var (recordType, handler) in recordTypes)
            {
                Console.WriteLine("---------------------------------------------------------------------------------");
                Console.WriteLine("---------------------------------------------------------------------------------");
                Console.WriteLine($"Processing {recordType.Name}");
                Console.WriteLine("---------------------------------------------------------------------------------");
                Console.WriteLine("---------------------------------------------------------------------------------");

                // Get all winning contexts for this type
                var winningContexts = handler.GetWinningContexts(state);
                //   .ToDictionary(r => ((IMajorRecordGetter)r).FormKey, r => r);


                bool hasPrintedAvailableProperties = false;
                foreach (var context in winningContexts)
                {
                    if (!hasPrintedAvailableProperties)
                    {
                        Console.WriteLine("Available properties on record type:");
                        foreach (var prop in context.Record.GetType().GetProperties())
                        {
                            Console.WriteLine($"- {prop.Name}");
                        }
                        Console.WriteLine("\nAvailable properties on interface type:");
                        foreach (var prop in recordType.GetProperties())
                        {
                            Console.WriteLine($"- {prop.Name}");
                        }
                        hasPrintedAvailableProperties = true;
                    }

                    Console.WriteLine("\n---------------------------------------------------------------------------------");
                    Console.WriteLine($"Processing record: {context.Record}");
                    Console.WriteLine("---------------------------------------------------------------------------------\n");

                    // Cache property info for this type
                    var propertyCache = new Dictionary<string, PropertyInfo>();
                    foreach (var propName in propertyHandlers.Keys)
                    {
                        var property = context.Record.GetType().GetProperty(propName);
                        if (property != null)
                        {
                            propertyCache[propName] = property;
                        }
                    }

                    // Process each property using cached property info
                    var propertiesToForward = new Dictionary<string, object?>();
                    foreach (var (propName, propertyHandler) in propertyHandlers)
                    {
                        if (propertyCache.TryGetValue(propName, out var property))
                        {
                            var lastChangedValue = propertyHandler.ProcessProperty(property, context, state, recordType);
                            var winningValue = property.GetValue(context.Record);
                            if (!Equals(lastChangedValue, winningValue))
                            {
                                Console.WriteLine($"Winning value: {(winningValue is null ? "[null]" : winningValue)} is different from last changed value: {(lastChangedValue is null ? "[null]" : lastChangedValue)}");
                                Console.WriteLine($"- Forwarding last changed value\n");
                                propertiesToForward[propName] = lastChangedValue;
                            }
                            else
                            {
                                Console.WriteLine($"Winning value: {(winningValue is null ? "[null]" : winningValue)} is the same as last changed value: {(lastChangedValue is null ? "[null]" : lastChangedValue)}");
                                Console.WriteLine($"- Discarding last changed value\n");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"=== Property {propName} not found in record {context.Record.FormKey} ===");
                        }
                    }

                    // forward properties if there are any
                    if (propertiesToForward.Count > 0)
                    {
                        // Create an override of the record using the record type handler
                        var overrideRecord = handler.CreateOverride(state, context);
                        Console.WriteLine($"Created override for record {context.Record.FormKey}");

                        // Set each property that needs to be forwarded
                        foreach (var (propName, value) in propertiesToForward)
                        {
                            if (propertyCache.TryGetValue(propName, out var property))
                            {
                                var oldValue = property.GetValue(overrideRecord);
                                property.SetValue(overrideRecord, value);
                                Console.WriteLine($"{propName}: {(oldValue is null ? "[null]" : oldValue)} -> {(value is null ? "[null]" : value)}");
                            }
                        }
                    }
                }
            }
        }

        public interface IPropertyHandler
        {
            object? ProcessProperty(
                PropertyInfo property,
                IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
                IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
                Type recordType);
        }

        public class SimplePropertyHandler : IPropertyHandler
        {
            public object? ProcessProperty(
                PropertyInfo property,
                IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
                IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
                Type recordType)
            {
                IMajorRecordGetter? majorRecord = context.Record;
                if (majorRecord == null)
                {
                    Console.WriteLine($"Warning: Record {context.Record} is not an IMajorRecordGetter. Cannot access FormKey and other major record properties.");
                    return null;
                }

                // Get the last changed value of the property from the record
                //var winningValue = property.GetValue(context.Record);
                var lastChangedValue = RecordUtils.GetLastChangedValue(property, majorRecord, state, recordType);

                // Return the last valid changed value - caller will decide whether to forward it
                return lastChangedValue;
            }
        }

        public class FormLinkPropertyHandler : IPropertyHandler
        {
            public object? ProcessProperty(PropertyInfo property, IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context, IPatcherState<ISkyrimMod, ISkyrimModGetter> state, Type recordType)
            {
                // Cache the value to avoid multiple property access
                var value = property.GetValue(context.Record);
                if (value != null)
                {
                    // Handle FormLink specific logic
                    Console.WriteLine($"{property.Name}: {value}");
                }
                return null;
            }
        }

        public class CollectionPropertyHandler : IPropertyHandler
        {
            public object? ProcessProperty(PropertyInfo property, IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context, IPatcherState<ISkyrimMod, ISkyrimModGetter> state, Type recordType)
            {
                // Cache the value to avoid multiple property access
                var value = property.GetValue(context.Record);
                if (value is IEnumerable<object> collection)
                {
                    Console.WriteLine($"{property.Name}:");
                    foreach (var item in collection)
                    {
                        Console.WriteLine($"  - {item}");
                    }
                }
                return null;
            }
        }

        public class NestedPropertyHandler : IPropertyHandler
        {
            private readonly string _parentProperty;
            private readonly string _childProperty;

            public NestedPropertyHandler(string parentProperty, string childProperty)
            {
                _parentProperty = parentProperty;
                _childProperty = childProperty;
            }

            public object? ProcessProperty(PropertyInfo property, IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context, IPatcherState<ISkyrimMod, ISkyrimModGetter> state, Type recordType)
            {
                IMajorRecordGetter? majorRecord = context.Record;
                if (majorRecord == null)
                {
                    Console.WriteLine($"Warning: Record {context.Record} is not an IMajorRecordGetter. Cannot access FormKey and other major record properties.");
                    return null;
                }

                // Get parent property
                var parentProp = majorRecord.GetType().GetProperty(_parentProperty);
                if (parentProp == null)
                {
                    Console.WriteLine($"Parent property {_parentProperty} not found on record type {majorRecord.GetType().Name}");
                    return null;
                }

                // Get parent value
                var parentValue = parentProp.GetValue(majorRecord);
                if (parentValue == null)
                {
                    Console.WriteLine($"Parent property {_parentProperty} is null on record {majorRecord}");
                    return null;
                }

                // Get child property
                var childProp = parentValue.GetType().GetProperty(_childProperty);
                if (childProp == null)
                {
                    Console.WriteLine($"Child property {_childProperty} not found on parent type {parentValue.GetType().Name}");
                    return null;
                }

                // Get current and original values
                var winningValue = childProp.GetValue(parentValue);
                var originalValue = RecordUtils.GetOriginalValue(property, majorRecord, state);
                var lastValidChangedValue = RecordUtils.GetLastChangedValue(property, majorRecord, state, recordType);

                // Log the property and its value
                Console.WriteLine($"\nNested Property: {_parentProperty}.{_childProperty}");
                Console.WriteLine($"Type: {childProp.PropertyType.Name}");
                Console.WriteLine($"Current (winning)Value: {winningValue}");
                Console.WriteLine($"Original Value: {originalValue}");
                Console.WriteLine($"Last Valid Changed Value: {lastValidChangedValue}");

                // Compare values
                if (!Equals(winningValue, originalValue))
                {
                    Console.WriteLine("Value has been modified from original value!");
                }
                else if (!Equals(originalValue, lastValidChangedValue))
                {
                    Console.WriteLine("Value has been modified from last valid changed value!");
                }

                return !Equals(lastValidChangedValue, winningValue) ? lastValidChangedValue : null;
            }
        }
    }
}

using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.RecordHandlers.Abstracts
{
    public abstract class AbstractRecordHandler : IRecordHandler
    {
        // Abstract property that all record handlers must implement
        public abstract Dictionary<string, IPropertyHandler> PropertyHandlers { get; }

        protected Dictionary<string, IPropertyContext> PropertyContexts { get; private set; } = [];


        /// <summary>
        /// Initialize the property contexts for the record
        /// </summary>
        /// <param name="originalContext"></param>
        /// <param name="winningContext"></param>
        protected void InitializePropertyContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext)
        {
            PropertyContexts.Clear();
            foreach (var (propertyName, handler) in PropertyHandlers)
            {
                // Let each handler create its own properly typed context
                var propertyContext = handler.CreatePropertyContext();
                PropertyContexts[propertyName] = propertyContext;
                handler.InitializeContext(originalContext, winningContext, propertyContext);
            }
        }

        public void Process(IPatcherState<ISkyrimMod, ISkyrimModGetter> state, IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] filteredWinningContexts)
        {
            foreach (var winningContext in filteredWinningContexts)
            {
                Console.WriteLine(new string('-', 80));
                Console.WriteLine($"Processing: {winningContext.Record.FormKey} ({winningContext.Record.EditorID})");

                // some break early checks if pre-filtering failed
                if (Utility.IsVanilla(winningContext))
                {
                    Console.WriteLine("Breaking early: Winning context is vanilla");
                    continue;
                }

                // Get all contexts for this record in load order using concrete handler
                var recordContexts = GetRecordContexts(winningContext, state);

                if (recordContexts.Length <= 2)
                {
                    Console.WriteLine("Breaking early: 2 or less contexts");
                    continue;
                }

                // Check if the mod before the winning context is vanilla
                var previousContext = recordContexts[1];
                if (Utility.IsVanilla(previousContext))
                {
                    Console.WriteLine("Breaking early: Previous context is vanilla");
                    continue;
                }
                Console.WriteLine($"Record contexts: {recordContexts.Length}");

                // Initialize property states and quick initial check for simple properties
                var originalContext = recordContexts.Last();
                InitializePropertyContexts(originalContext, winningContext);

                // Quick initial check for simple properties
                bool allResolved = true;
                bool hasListProperties = false;
                foreach (var (propName, handler) in PropertyHandlers)
                {
                    if (!handler.IsListHandler)
                    {
                        var originalValue = handler.GetValue(originalContext.Record);
                        var winningValue = handler.GetValue(winningContext.Record);
                        var propContext = PropertyContexts[propName];

                        if (!handler.AreValuesEqual(originalValue, winningValue))
                        {
                            propContext.IsResolved = true;
                            Console.WriteLine($"[{propName}] {winningContext.ModKey} Resolved, nothing to forward");
                        }
                        else
                        {
                            allResolved = false;
                        }
                    }
                    else
                    {
                        hasListProperties = true;
                    }
                }

                // print original and winning values for all properties
                foreach (var (propName, handler) in PropertyHandlers)
                {
                    var originalValue = handler.GetValue(originalContext.Record);
                    var winningValue = handler.GetValue(winningContext.Record);
                    Console.WriteLine($"[{propName}] Original: {originalValue}, Winning: {winningValue}");
                }

                // Pass 1: Process from original to winning (for lists and unresolved properties)
                // Pass 1 is required for lists
                // Check if we have any list properties to process
                if (!hasListProperties)
                {
                    Console.WriteLine("Skipping first pass: No list properties to process");
                }
                else
                {
                    Console.WriteLine("Processing first pass");

                    // iterate from original to winning
                    foreach (var context in recordContexts.Reverse().Skip(1))
                    {
                        // Update the property contexts, skip if resolved
                        foreach (var (propName, handler) in PropertyHandlers)
                        {                            
                            var propContext = PropertyContexts[propName];
                            if (propContext.IsResolved) continue;

                            LogCollector.Add(propName, $"[{propName}] Processing mod: {context.ModKey} with value: {handler.GetValue(context.Record)}");

                            // if not, update the property context
                            handler.UpdatePropertyContext(context, state, propContext);
                        }
                    }

                    LogCollector.PrintAll();
                    LogCollector.Clear();

                    // Process properties after pass 1. Every property should be resolved after pass 1
                    foreach (var (propName, handler) in PropertyHandlers)
                    {
                        var propContext = PropertyContexts[propName];

                        // Mark as resolved if it is processed in pass 1
                        propContext.IsResolved = true;
                        Console.WriteLine($"[{propName}] {winningContext.ModKey}: Marked as resolved after pass 1");
                    }
                    Console.WriteLine("First pass complete");
                }

                // Pass 2: Process from winning to original (for any remaining unresolved properties)
                // This will only run if there are no list properties. It is much more efficent than pass 1
                if (!allResolved)
                {
                    Console.WriteLine("Processing second pass");
                    // reset prop handlers
                    foreach (var (propName, handler) in PropertyHandlers)
                    {
                        if (!PropertyContexts[propName].IsResolved)
                        {
                            handler.InitializeContext(originalContext, winningContext, PropertyContexts[propName]);
                        }
                    }

                    // iterate from winning towards original
                    foreach (var context in recordContexts)
                    {
                        if (allResolved)
                        {
                            break;
                        }

                        // Skip if we've reached the original mod
                        if (context == originalContext)
                        {
                            break;
                        }

                        foreach (var (propName, handler) in PropertyHandlers)
                        {
                            allResolved = true;
                            var propertyContext = PropertyContexts[propName];
                            if (propertyContext.IsResolved)
                            {
                                continue;
                            }

                            allResolved = false;
                            LogCollector.Add(propName, $"[{propName}] Processing mod: {context.ModKey} with value: {handler.GetValue(context.Record)}");
                            handler.UpdatePropertyContext(context, state, propertyContext);

                            // If property has changed, iterate back to check for valid reverts
                            var forwardValue = handler.GetValue(context.Record);
                            var originalValue = handler.GetValue(originalContext.Record);
                            if (!handler.AreValuesEqual(forwardValue, originalValue))
                            {
                                // Find the index of current context
                                var currentIndex = Array.IndexOf(recordContexts, context);

                                // Iterate back towards winning
                                for (int i = currentIndex - 1; i >= 0; i--)
                                {
                                    handler.UpdatePropertyContext(recordContexts[i], state, propertyContext);
                                }

                                // Now we have the real final value, mark as resolved
                                propertyContext.IsResolved = true;
                            }
                        }
                    }
                    LogCollector.Add("Second pass", "Second pass complete");
                }
                else
                {
                    LogCollector.Add("Second pass", "Skipping second pass: All properties resolved");
                }
                LogCollector.PrintAll();
                LogCollector.Clear();


                // Forward changes to the patcher
                var propertiesToForward = PropertyContexts
                    .Where(kvp =>
                    {
                        var propertyContext = kvp.Value;
                        var handler = PropertyHandlers[kvp.Key];
                        if (propertyContext == null || handler == null) return false;

                        var winningValue = handler.GetValue(winningContext.Record);
                        Console.WriteLine($"[{kvp.Key}] Winning value: {winningValue}");

                        // Get the forward value from the property context
                        var forwardValue = propertyContext.GetForwardValue();
                        
                        return !handler.AreValuesEqual(forwardValue, winningValue);
                    })
                    .ToDictionary(kvp => kvp.Key, kvp =>
                    {
                        var propertyContext = kvp.Value;

                        // Get the forward value (could be single value or list)
                        return propertyContext.GetForwardValue();
                    });

                Console.WriteLine($"Properties to forward: {propertiesToForward.Count}");
                if (propertiesToForward.Count > 0)
                {
                    var overrideRecord = GetOverrideRecord(winningContext, state);
                    ApplyForwardedProperties(overrideRecord, propertiesToForward);
                }
            }
        }


        public abstract IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state);

        public abstract IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state);

        public abstract void ApplyForwardedProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward);
    }
}

using System;
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

        /// <summary>
        /// Process the record
        /// </summary>
        /// <param name="state"></param>
        /// <param name="filteredWinningContexts"></param>
        public void Process(IPatcherState<ISkyrimMod, ISkyrimModGetter> state, IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] filteredWinningContexts)
        {
            foreach (var winningContext in filteredWinningContexts)
            {
                Console.WriteLine(new string('-', 80));
                Console.WriteLine($"Processing: {winningContext.Record.FormKey} ({winningContext.Record.EditorID})");

                // some break early checks if the pre-filtering failed
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
                Console.WriteLine($"Winning context: {winningContext.ModKey}");

                // Initialize property states and quick initial check for simple properties
                var originalContext = recordContexts.Last();
                Console.WriteLine($"Original context: {originalContext.ModKey}");
                InitializePropertyContexts(originalContext, winningContext);

                // Quick initial check for simple properties
                // all simple properties (not lists) should be resolved if the original and winning values are different
                bool allResolved = true;
                bool requiresPass1 = false;
                foreach (var (propName, handler) in PropertyHandlers)
                {
                    if (!handler.RequiresFullLoadOrderProcessing)
                    {
                        var originalValue = handler.GetValue(originalContext.Record);
                        var winningValue = handler.GetValue(winningContext.Record);
                        var propContext = PropertyContexts[propName];

                        if (!handler.AreValuesEqual(originalValue, winningValue))
                        {
                            propContext.IsResolved = true;
                            LogCollector.Add(propName, $"[{propName}] {winningContext.Record.FormKey} Resolved, nothing to forward. Original: {handler.FormatValue(originalValue)}, Winning: {handler.FormatValue(winningValue)}");
                        }
                        else
                        {
                            allResolved = false;
                        }
                    }
                    else
                    {
                        requiresPass1 = true;
                    }
                }

                // print original and winning values for all properties
                foreach (var (propName, handler) in PropertyHandlers)
                {
                    var originalValue = handler.GetValue(originalContext.Record);
                    var winningValue = handler.GetValue(winningContext.Record);
                    LogCollector.Add(propName, $"[{propName}] Original: {handler.FormatValue(originalValue)}, Winning: {handler.FormatValue(winningValue)}");
                }
                LogCollector.PrintAll();
                LogCollector.Clear();

                // Pass 1: Process from original to winning (for lists and unresolved properties)
                // Pass 1 is required for lists and flags
                // Check if we have any list properties to process. If not we can skip pass 1.
                if (!requiresPass1)
                {
                    Console.WriteLine("Skipping first pass: No list or flag properties to process");
                }
                else
                {
                    Console.WriteLine("Processing first pass");

                    // iterate from original to winning
                    foreach (var context in recordContexts.Reverse().Skip(1))
                    {
                        // bugfix, skip if context is output mod
                        if (context.ModKey.ToString() == state.PatchMod.ModKey.ToString())
                        {
                            continue;
                        }

                        // Update the property contexts, skip if resolved
                        foreach (var (propName, handler) in PropertyHandlers)
                        {
                            var propContext = PropertyContexts[propName];
                            if (propContext.IsResolved) continue;

                            var mod = state.LoadOrder[context.ModKey].Mod;
                            LogCollector.Add(propName, $"[{propName}] Processing mod: {context.ModKey} with value: {handler.FormatValue(handler.GetValue(context.Record))} with masters: {(mod != null ? string.Join(", ", mod.MasterReferences.Select(m => m.Master.FileName)) : "")}");

                            handler.UpdatePropertyContext(context, state, propContext);
                        }
                    }

                    // Process properties after pass 1. Every property should be resolved after pass 1
                    foreach (var (propName, handler) in PropertyHandlers)
                    {
                        var propContext = PropertyContexts[propName];

                        // Mark as resolved if it is processed in pass 1
                        propContext.IsResolved = true;
                        LogCollector.Add(propName, $"[{propName}] {winningContext.ModKey}: Marked as resolved after pass 1");
                    }
                    LogCollector.PrintAll();
                    LogCollector.Clear();
                    Console.WriteLine("First pass complete");
                }

                // Pass 2: Process from winning to original (for any remaining unresolved properties)
                // This will only run if there are no list or flag properties. It is more efficent than pass 1.
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

                        // bugfix, skip if context is output mod
                        if (context.ModKey.ToString() == state.PatchMod.ModKey.ToString())
                        {
                            continue;
                        }

                        foreach (var (propName, handler) in PropertyHandlers)
                        {
                            // if the property is resolved, skip it
                            allResolved = true;
                            var propertyContext = PropertyContexts[propName];
                            if (propertyContext.IsResolved)
                            {
                                continue;
                            }

                            // if the property is not resolved, update the property context
                            allResolved = false;
                            var mod = state.LoadOrder[context.ModKey].Mod;
                            LogCollector.Add(propName, $"[{propName}] Processing mod: {context.ModKey} with value: {handler.FormatValue(handler.GetValue(context.Record))} with masters: {(mod != null ? string.Join(", ", mod.MasterReferences.Select(m => m.Master.FileName)) : "")}");
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
                                    // bugfix, skip if context is output mod
                                    if (recordContexts[i].ModKey.ToString() == state.PatchMod.ModKey.ToString())
                                    {
                                        continue;
                                    }

                                    handler.UpdatePropertyContext(recordContexts[i], state, propertyContext);
                                }

                                // Now we have the real final value, mark as resolved
                                propertyContext.IsResolved = true;
                            }
                        }
                    }
                    LogCollector.PrintAll();
                    LogCollector.Clear();
                    Console.WriteLine("Second pass complete");
                }
                else
                {
                    Console.WriteLine("Skipping second pass: All properties resolved");
                }

                // Forward changes to the patcher
                var propertiesToForward = PropertyContexts
                    .Where(kvp =>
                    {
                        var propertyContext = kvp.Value;
                        var handler = PropertyHandlers[kvp.Key];
                        if (propertyContext == null || handler == null) return false;

                        var originalValue = handler.GetValue(originalContext.Record);
                        var winningValue = handler.GetValue(winningContext.Record);
                        var forwardValue = propertyContext.GetForwardValue();

                        // Show all values for comparison
                        LogCollector.Add(kvp.Key, $"[{kvp.Key}] Final Decision:");
                        LogCollector.Add(kvp.Key, $"[{kvp.Key}]   Original value: {handler.FormatValue(originalValue)}");
                        LogCollector.Add(kvp.Key, $"[{kvp.Key}]   Winning value: {handler.FormatValue(winningValue)}");
                        LogCollector.Add(kvp.Key, $"[{kvp.Key}]   Forward value: {handler.FormatValue(forwardValue)}");

                        var shouldForward = !handler.AreValuesEqual(forwardValue, winningValue);
                        LogCollector.Add(kvp.Key, $"[{kvp.Key}]   Decision: {(shouldForward ? "Forward changes (different)" : "No changes (same)")}");

                        LogCollector.PrintAll();
                        LogCollector.Clear();

                        return shouldForward;
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

        /// <summary>
        /// Applies flag properties (MajorRecordFlagsRaw and SkyrimMajorRecordFlags) together.
        /// These share the same record header in the file format, so setting one might affect the other.
        /// This method ensures both are set together to preserve values correctly.
        /// 
        /// IMPORTANT: When setting SkyrimMajorRecordFlags, we always preserve MajorRecordFlagsRaw from the record
        /// (which should be the winning value), even if MajorRecordFlagsRaw is not in propertiesToForward.
        /// This prevents SkyrimMajorRecordFlags from overwriting MajorRecordFlagsRaw.
        /// </summary>
        /// <param name="record">The record to apply properties to</param>
        /// <param name="propertiesToForward">Dictionary of properties to forward (will be modified to remove processed flags)</param>
        protected virtual void ApplyFlagProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward)
        {
            bool hasMajorRecordFlagsRaw = propertiesToForward.TryGetValue("MajorRecordFlagsRaw", out var majorRecordFlagsRawValue);
            bool hasSkyrimMajorRecordFlags = propertiesToForward.TryGetValue("SkyrimMajorRecordFlags", out var skyrimMajorRecordFlagsValue);

            // Always handle flags if either is being set, OR if SkyrimMajorRecordFlags is being set (to preserve MajorRecordFlagsRaw)
            if (hasMajorRecordFlagsRaw || hasSkyrimMajorRecordFlags)
            {
                // Read current values from the record (which should be the winning values since we're applying to an override)
                int currentMajorRecordFlagsRaw = record.MajorRecordFlagsRaw;
                Mutagen.Bethesda.Skyrim.SkyrimMajorRecord.SkyrimMajorRecordFlag currentSkyrimMajorRecordFlags = 0;
                
                if (record is ISkyrimMajorRecord skyrimRecord)
                {
                    currentSkyrimMajorRecordFlags = skyrimRecord.SkyrimMajorRecordFlags;
                }

                // Determine what values to set
                int newMajorRecordFlagsRaw = hasMajorRecordFlagsRaw && majorRecordFlagsRawValue is int majorRecordFlagsRawInt
                    ? majorRecordFlagsRawInt
                    : currentMajorRecordFlagsRaw; // Always preserve current value if not being explicitly set

                Mutagen.Bethesda.Skyrim.SkyrimMajorRecord.SkyrimMajorRecordFlag newSkyrimMajorRecordFlags = hasSkyrimMajorRecordFlags && skyrimMajorRecordFlagsValue is Mutagen.Bethesda.Skyrim.SkyrimMajorRecord.SkyrimMajorRecordFlag skyrimFlags
                    ? skyrimFlags
                    : currentSkyrimMajorRecordFlags;

                // Set MajorRecordFlagsRaw first
                record.MajorRecordFlagsRaw = newMajorRecordFlagsRaw;
                if (PropertyHandlers.TryGetValue("MajorRecordFlagsRaw", out var majorRecordFlagsRawHandler))
                {
                    if (hasMajorRecordFlagsRaw)
                    {
                        Console.WriteLine($"[MajorRecordFlagsRaw] Applying value: {majorRecordFlagsRawHandler.FormatValue(newMajorRecordFlagsRaw)}");
                    }
                    else if (hasSkyrimMajorRecordFlags)
                    {
                        // Even if not forwarding MajorRecordFlagsRaw, log that we're preserving it
                        Console.WriteLine($"[MajorRecordFlagsRaw] Preserving value: {majorRecordFlagsRawHandler.FormatValue(newMajorRecordFlagsRaw)} (not in propertiesToForward, but preserving to prevent overwrite)");
                    }
                }

                // Then set SkyrimMajorRecordFlags (this might internally reconstruct flags, so we set MajorRecordFlagsRaw again after)
                if (record is ISkyrimMajorRecord skyrimRecordForFlags)
                {
                    skyrimRecordForFlags.SkyrimMajorRecordFlags = newSkyrimMajorRecordFlags;
                    if (hasSkyrimMajorRecordFlags && PropertyHandlers.TryGetValue("SkyrimMajorRecordFlags", out var skyrimMajorRecordFlagsHandler))
                    {
                        Console.WriteLine($"[SkyrimMajorRecordFlags] Applying value: {skyrimMajorRecordFlagsHandler.FormatValue(newSkyrimMajorRecordFlags)}");
                    }

                    // ALWAYS re-apply MajorRecordFlagsRaw after setting SkyrimMajorRecordFlags to ensure it's preserved
                    // (in case Mutagen's internal logic reconstructed the flags)
                    // This is critical even if MajorRecordFlagsRaw wasn't in propertiesToForward
                    int majorRecordFlagsRawAfterSkyrim = record.MajorRecordFlagsRaw;
                    
                    if (majorRecordFlagsRawAfterSkyrim != newMajorRecordFlagsRaw)
                    {
                        // Mutagen may have set additional bits in MajorRecordFlagsRaw when we set SkyrimMajorRecordFlags
                        // We need to preserve BOTH:
                        // 1. The bits we want from newMajorRecordFlagsRaw (e.g., Persistent = 0x400)
                        // 2. The bits Mutagen set for SkyrimMajorRecordFlags (e.g., InitiallyDisabled = 0x800)
                        // Solution: OR them together to preserve both sets of flags
                        int mergedFlags = newMajorRecordFlagsRaw | majorRecordFlagsRawAfterSkyrim;
                        record.MajorRecordFlagsRaw = mergedFlags;
                    }
                }

                // Remove from dictionary so we don't process them again
                propertiesToForward.Remove("MajorRecordFlagsRaw");
                propertiesToForward.Remove("SkyrimMajorRecordFlags");
            }
        }

        /// <summary>
        /// Applies forwarded properties to the record.
        /// Default implementation handles flag properties specially, then processes all other properties.
        /// Override this method if you need custom property application logic.
        /// </summary>
        /// <param name="record">The record to apply properties to</param>
        /// <param name="propertiesToForward">Dictionary of properties to forward</param>
        public virtual void ApplyForwardedProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward)
        {
            // Handle flag properties first (they need special coordination)
            ApplyFlagProperties(record, propertiesToForward);

            // Process all other properties normally
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
                        // Property doesn't exist on this record type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on record {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}

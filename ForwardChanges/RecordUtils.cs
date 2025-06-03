using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using System.Reflection;

namespace ForwardChanges
{
    public static class RecordUtils
    {
        public static object? GetOriginalValue(PropertyInfo property, IMajorRecordGetter currentRecord, IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // Get the interface type that matches our dictionary
            var recordType = currentRecord.GetType();
            var interfaceType = recordType.GetInterfaces()
                .FirstOrDefault(i => Program.recordTypes.ContainsKey(i));

            if (interfaceType == null)
            {
                Console.WriteLine($"Warning: Could not find interface type for {recordType.Name}");
                return null;
            }

            // Get the original context using the interface type
            var originalContext = Program.recordTypes[interfaceType].GetOriginalContext(state, currentRecord.FormKey);
            if (originalContext?.Record == null) return null;

            // Split the property path if it contains a dot
            var propertyPath = property.Name.Split('.');

            // Start with the record
            object? currentObject = originalContext.Record;

            // Traverse the property path
            foreach (var propName in propertyPath)
            {
                if (currentObject == null) return null;

                var prop = currentObject.GetType().GetProperty(propName);
                if (prop == null)
                {
                    Console.WriteLine($"Property {propName} not found on type {currentObject.GetType().Name}");
                    return null;
                }

                currentObject = prop.GetValue(currentObject);
            }

            return currentObject;
        }

        public static object? GetLastChangedValue(PropertyInfo property, IMajorRecordGetter currentRecord, IPatcherState<ISkyrimMod, ISkyrimModGetter> state, Type recordType)
        {
            Console.WriteLine($"=== Processing {property.Name} ===");

            // get original value
            var originalValue = GetOriginalValue(property, currentRecord, state);
            Console.WriteLine($"Original value: {originalValue}");

            // if original value and current value is not equal, the current value is the last valid changed value
            var currentValue = property.GetValue(currentRecord);
            Console.WriteLine($"Winning value: {currentValue}");

            if (!Equals(currentValue, originalValue))
            {
                Console.WriteLine("Winning value differs from original, it is the last valid changed value");
                return currentValue;
            }

            // Use the provided record type directly
            if (!Program.recordTypes.TryGetValue(recordType, out var handler))
            {
                Console.WriteLine($"Warning: No handler found for record type {recordType.Name}");
                return originalValue;
            }

            // Get all contexts using the type-specific handler
            var contexts = handler.GetAllContexts(state, currentRecord.FormKey).ToList();
            Console.WriteLine($"Found {contexts.Count} contexts for record");

            // Find the first change
            int changeIndex = -1;
            for (int i = 0; i < contexts.Count; i++)
            {
                var contextValue = property.GetValue(contexts[i].Record);
                if (!Equals(contextValue, originalValue))
                {
                    changeIndex = i;
                    Console.WriteLine($"Found first change at index {i} with value {contextValue}");
                    break;
                }
            }

            // If no change found, return original value
            if (changeIndex == -1)
            {
                Console.WriteLine("All values are the same, original value is the last valid changed value");
                return originalValue;
            }

            // Check if any subsequent mods can revert the value
            var changedMod = contexts[changeIndex].ModKey;
            Console.WriteLine($"Changed mod: {changedMod}");
            var changedValue = property.GetValue(contexts[changeIndex].Record);
            Console.WriteLine($"Changed value: {changedValue}");

            Console.WriteLine("Checking subsequent mods for potential reverts:");
            for (int i = changeIndex + 1; i < contexts.Count; i++)
            {
                var context = contexts[i];
                var contextValue = property.GetValue(context.Record);
                Console.WriteLine($"  Checking mod {context.ModKey} with value {contextValue}");

                // If this mod has the original value, check if it can revert the change
                if (Equals(contextValue, originalValue))
                {
                    var mod = state.LoadOrder[context.ModKey].Mod;
                    if (mod != null && mod.MasterReferences.Any(m => m.Master.Equals(changedMod)))
                    {
                        Console.WriteLine($"  Mod {context.ModKey} can revert the change - returning original value");
                        return originalValue;
                    }
                }
            }

            return changedValue;
        }

        private class ChangeInfo
        {
            public object? Value { get; set; }
            public ModKey ModThatMadeChange { get; set; }
        }
    }
}
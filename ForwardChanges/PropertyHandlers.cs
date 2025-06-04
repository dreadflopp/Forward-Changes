using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using System.Reflection;

namespace ForwardChanges
{
    public interface IPropertyHandler
    {
        object? GetLastChangedValue(
            PropertyInfo property,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] recordContexts,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state);

        object? GetWinningValue(
            PropertyInfo property,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext);
    }

    public static class PropertyHandlerUtils
    {
        public static readonly HashSet<string> VanillaMods = new()
        {
            "Skyrim.esm",
            "Update.esm",
            "Dawnguard.esm",
            "HearthFires.esm",
            "Dragonborn.esm"
        };

        public static bool ShouldBreakEarly(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] recordContexts)
        {
            // Break early if only 1 context
            if (recordContexts.Length <= 1)
            {
                Console.WriteLine("Breaking early: Only 1 context found");
                return true;
            }

            // Break early if all mods are vanilla
            if (recordContexts.Length <= 5)
            {
                var allVanilla = recordContexts.All(context => VanillaMods.Contains(context.ModKey.Name));
                if (allVanilla)
                {
                    Console.WriteLine("Breaking early: All mods are vanilla");
                    return true;
                }
            }

            return false;
        }
    }

    public class SimplePropertyHandler : IPropertyHandler
    {
        public object? GetLastChangedValue(
            PropertyInfo property,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] recordContexts,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Console.WriteLine($"\n=== Processing {property.Name} ===");

            if (recordContexts == null || !recordContexts.Any())
            {
                Console.WriteLine("No contexts found for record");
                return null;
            }

            // Check if we can break early
            if (PropertyHandlerUtils.ShouldBreakEarly(recordContexts))
            {
                var earlyBreakValue = property.GetValue(recordContexts.Last().Record);
                Console.WriteLine($"Breaking early - returning original value: {earlyBreakValue}");
                return earlyBreakValue;
            }

            // Get values from original and winning records
            var originalContext = recordContexts.Last();
            var originalValue = property.GetValue(originalContext.Record);
            Console.WriteLine($"Original value: {originalValue}");
            var winningValue = property.GetValue(recordContexts[0].Record);
            Console.WriteLine($"Winning value: {winningValue}");

            if (!Equals(winningValue, originalValue))
            {
                Console.WriteLine("Winning value differs from original, it is the last valid changed value");
                return winningValue;
            }

            Console.WriteLine($"Found {recordContexts.Length} contexts for record");

            // Find the first change from winning to original
            int changeIndex = -1;
            for (int i = 0; i < recordContexts.Length; i++)
            {
                var contextValue = property.GetValue(recordContexts[i].Record);
                Console.WriteLine($"Context value: {contextValue} from {recordContexts[i].ModKey}");
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

            // Cache the changed mod and value
            var changedContext = recordContexts[changeIndex];
            var changedValue = property.GetValue(changedContext.Record);
            Console.WriteLine($"Changed mod: {changedContext.ModKey}");
            Console.WriteLine($"Changed value: {changedValue}");

            Console.WriteLine("Checking mods from change to winning for potential reverts:");
            // Check all mods from the change back to winning (higher priority)
            for (int i = 0; i < changeIndex; i++)
            {
                var higherPriorityContext = recordContexts[i];
                var contextValue = property.GetValue(higherPriorityContext.Record);
                Console.WriteLine($"  Checking mod {higherPriorityContext.ModKey} with value {contextValue}");

                // If this mod has the original value, check if it can revert the change
                if (Equals(contextValue, originalValue))
                {
                    var mod = state.LoadOrder[higherPriorityContext.ModKey].Mod;
                    if (mod != null && mod.MasterReferences.Any(m => m.Master.Equals(changedContext.ModKey)))
                    {
                        Console.WriteLine($"  Mod {higherPriorityContext.ModKey} can revert the change - returning original value");
                        return originalValue;
                    }
                }
            }

            return changedValue;
        }

        public object? GetWinningValue(
            PropertyInfo property,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext)
        {
            return property.GetValue(winningContext.Record);
        }
    }

    public class GroupedPropertyHandler<TGetter> : IPropertyHandler where TGetter : class, IMajorRecordGetter
    {
        private readonly string _parentProperty;
        private readonly string _childProperty;
        private readonly SimplePropertyHandler _simpleHandler;

        public GroupedPropertyHandler(string parentProperty, string childProperty)
        {
            _parentProperty = parentProperty;
            _childProperty = childProperty;
            _simpleHandler = new SimplePropertyHandler();
        }

        public object? GetLastChangedValue(
            PropertyInfo property,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] recordContexts,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Console.WriteLine($"\n=== Processing nested property {_parentProperty}.{_childProperty} ===");

            if (recordContexts == null || !recordContexts.Any())
            {
                Console.WriteLine("No contexts found for record");
                return null;
            }

            // Check if we can break early
            if (PropertyHandlerUtils.ShouldBreakEarly(recordContexts))
            {
                // Get the original value from the last context
                var earlyBreakContext = recordContexts.Last();
                if (earlyBreakContext.Record is TGetter earlyBreakGetter)
                {
                    if (earlyBreakGetter.GetType().GetProperty(_parentProperty)?.GetValue(earlyBreakGetter) is IAIDataGetter earlyBreakAiData)
                    {
                        if (earlyBreakAiData.GetType().GetProperty(_childProperty) is PropertyInfo childProperty)
                        {
                            var earlyBreakValue = childProperty.GetValue(earlyBreakAiData);
                            Console.WriteLine($"Breaking early - returning original value: {earlyBreakValue}");
                            return earlyBreakValue;
                        }
                    }
                }
                return null;
            }

            // Get values from original and winning records
            var originalContext = recordContexts.Last();
            var winningContext = recordContexts[0];

            // Get the parent property value using pattern matching
            if (originalContext.Record is TGetter originalGetter && winningContext.Record is TGetter winningGetter)
            {
                // Get the parent property value using pattern matching
                if (originalGetter.GetType().GetProperty(_parentProperty)?.GetValue(originalGetter) is IAIDataGetter originalAiData &&
                    winningGetter.GetType().GetProperty(_parentProperty)?.GetValue(winningGetter) is IAIDataGetter winningAiData)
                {
                    // Get the child property value using pattern matching
                    if (originalAiData.GetType().GetProperty(_childProperty) is PropertyInfo childProperty)
                    {
                        var originalValue = childProperty.GetValue(originalAiData);
                        Console.WriteLine($"Original value: {originalValue}");
                        var winningValue = childProperty.GetValue(winningAiData);
                        Console.WriteLine($"Winning value: {winningValue}");

                        if (!Equals(winningValue, originalValue))
                        {
                            Console.WriteLine("Winning value differs from original, it is the last valid changed value");
                            return winningValue;
                        }

                        Console.WriteLine($"Found {recordContexts.Length} contexts for record");

                        // Find the first change from winning to original
                        int changeIndex = -1;
                        for (int i = 0; i < recordContexts.Length; i++)
                        {
                            if (recordContexts[i].Record is TGetter npcGetter)
                            {
                                if (npcGetter.GetType().GetProperty(_parentProperty)?.GetValue(npcGetter) is IAIDataGetter aiData)
                                {
                                    var contextValue = childProperty.GetValue(aiData);
                                    Console.WriteLine($"Context value: {contextValue} from {recordContexts[i].ModKey}");
                                    if (!Equals(contextValue, originalValue))
                                    {
                                        changeIndex = i;
                                        Console.WriteLine($"Found first change at index {i} with value {contextValue}");
                                        break;
                                    }
                                }
                            }
                        }

                        // If no change found, return original value
                        if (changeIndex == -1)
                        {
                            Console.WriteLine("All values are the same, original value is the last valid changed value");
                            return originalValue;
                        }

                        // Cache the changed mod and value
                        var changedContext = recordContexts[changeIndex];
                        if (changedContext.Record is TGetter changedGetter)
                        {
                            if (changedGetter.GetType().GetProperty(_parentProperty)?.GetValue(changedGetter) is IAIDataGetter changedAiData)
                            {
                                var changedValue = childProperty.GetValue(changedAiData);
                                Console.WriteLine($"Changed mod: {changedContext.ModKey}");
                                Console.WriteLine($"Changed value: {changedValue}");

                                Console.WriteLine("Checking mods from change to winning for potential reverts:");
                                // Check all mods from the change back to winning (higher priority)
                                for (int i = 0; i < changeIndex; i++)
                                {
                                    var higherPriorityContext = recordContexts[i];
                                    if (higherPriorityContext.Record is TGetter higherGetter)
                                    {
                                        if (higherGetter.GetType().GetProperty(_parentProperty)?.GetValue(higherGetter) is IAIDataGetter higherAiData)
                                        {
                                            var contextValue = childProperty.GetValue(higherAiData);
                                            Console.WriteLine($"  Checking mod {higherPriorityContext.ModKey} with value {contextValue}");

                                            // If this mod has the original value, check if it can revert the change
                                            if (Equals(contextValue, originalValue))
                                            {
                                                var mod = state.LoadOrder[higherPriorityContext.ModKey].Mod;
                                                if (mod != null && mod.MasterReferences.Any(m => m.Master.Equals(changedContext.ModKey)))
                                                {
                                                    Console.WriteLine($"  Mod {higherPriorityContext.ModKey} can revert the change - returning original value");
                                                    return originalValue;
                                                }
                                            }
                                        }
                                    }
                                }

                                return changedValue;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Child property {_childProperty} not found on type {originalAiData.GetType().Name}");
                    }
                }
                else
                {
                    Console.WriteLine($"Parent property {_parentProperty} is null or not found");
                }
            }

            return null;
        }

        public object? GetWinningValue(
            PropertyInfo property,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext)
        {
            if (winningContext.Record is TGetter winningGetter)
            {
                if (winningGetter.GetType().GetProperty(_parentProperty)?.GetValue(winningGetter) is IAIDataGetter winningAiData)
                {
                    if (winningAiData.GetType().GetProperty(_childProperty) is PropertyInfo childProperty)
                    {
                        return childProperty.GetValue(winningAiData);
                    }
                }
            }
            return null;
        }
    }

    public class ProtectionFlagsHandler : IPropertyHandler
    {
        public object? GetLastChangedValue(
            PropertyInfo property,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] recordContexts,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Console.WriteLine("\n=== Processing Protection Flags ===");

            if (recordContexts == null || !recordContexts.Any())
            {
                Console.WriteLine("No contexts found for record");
                return null;
            }

            // Get winning value first
            var winningValue = GetWinningValue(property, recordContexts[0]);
            if (winningValue is NpcConfiguration.Flag flag && flag == NpcConfiguration.Flag.Essential)
            {
                Console.WriteLine("Winning value is Essential - breaking early");
                return winningValue;
            }

            bool foundProtected = false;

            // Iterate through all contexts
            foreach (var context in recordContexts)
            {
                // Skip vanilla mods
                if (PropertyHandlerUtils.VanillaMods.Contains(context.ModKey.Name))
                    continue;

                if (context.Record is INpcGetter npc)
                {
                    var flags = npc.Configuration.Flags;

                    if (flags.HasFlag(NpcConfiguration.Flag.Essential))
                    {
                        Console.WriteLine($"Found Essential flag in {context.ModKey}. Breaking early");
                        return NpcConfiguration.Flag.Essential;
                    }

                    if (flags.HasFlag(NpcConfiguration.Flag.Protected))
                    {
                        Console.WriteLine($"Found Protected flag in {context.ModKey}");
                        foundProtected = true;
                    }
                }
            }

            return foundProtected ? NpcConfiguration.Flag.Protected : winningValue;
        }

        public object? GetWinningValue(
            PropertyInfo property,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext)
        {
            if (winningContext.Record is INpcGetter npc)
            {
                var flags = npc.Configuration.Flags;
                if (flags.HasFlag(NpcConfiguration.Flag.Essential))
                    return NpcConfiguration.Flag.Essential;
                if (flags.HasFlag(NpcConfiguration.Flag.Protected))
                    return NpcConfiguration.Flag.Protected;
            }
            return null;
        }
    }

    public class FormLinkPropertyHandler : IPropertyHandler
    {
        private object? ConvertFormLink(object? value)
        {
            if (value == null)
                return null;

            // Get the generic type of the FormLink
            var formLinkType = value.GetType();
            if (!formLinkType.IsGenericType)
                return value;

            // Use Mutagen's ToLink() method - this is the standard way to convert FormLinks
            var toLinkMethod = formLinkType.GetMethod("ToLink");
            if (toLinkMethod != null)
            {
                try
                {
                    return toLinkMethod.Invoke(value, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to convert FormLink using ToLink(): {ex.Message}");
                    return value;
                }
            }

            // If ToLink() is not available, return the original value
            Console.WriteLine("ToLink() method not found on FormLink type");
            return value;
        }

        public object? GetLastChangedValue(
            PropertyInfo property,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] recordContexts,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (recordContexts == null || !recordContexts.Any())
                return null;

            // Check if we can break early
            if (PropertyHandlerUtils.ShouldBreakEarly(recordContexts))
            {
                var earlyBreakValue = property.GetValue(recordContexts.Last().Record);
                return ConvertFormLink(earlyBreakValue);
            }

            // Get values from original and winning records
            var originalContext = recordContexts.Last();
            var originalValue = property.GetValue(originalContext.Record);
            var winningValue = property.GetValue(recordContexts[0].Record);

            // Compare FormLinks by their FormKey
            var originalFormKey = originalValue?.GetType().GetProperty("FormKey")?.GetValue(originalValue);
            var winningFormKey = winningValue?.GetType().GetProperty("FormKey")?.GetValue(winningValue);

            if (!Equals(winningFormKey, originalFormKey))
                return ConvertFormLink(winningValue);

            // Find the first change from winning to original
            int changeIndex = -1;
            for (int i = 0; i < recordContexts.Length; i++)
            {
                var contextValue = property.GetValue(recordContexts[i].Record);
                var contextFormKey = contextValue?.GetType().GetProperty("FormKey")?.GetValue(contextValue);
                if (!Equals(contextFormKey, originalFormKey))
                {
                    changeIndex = i;
                    break;
                }
            }

            // If no change found, return original value
            if (changeIndex == -1)
                return ConvertFormLink(originalValue);

            // Cache the changed mod and value
            var changedContext = recordContexts[changeIndex];
            var changedValue = property.GetValue(changedContext.Record);

            // Check all mods from the change back to winning (higher priority)
            for (int i = 0; i < changeIndex; i++)
            {
                var higherPriorityContext = recordContexts[i];
                var contextValue = property.GetValue(higherPriorityContext.Record);
                var contextFormKey = contextValue?.GetType().GetProperty("FormKey")?.GetValue(contextValue);

                // If this mod has the original value, check if it can revert the change
                if (Equals(contextFormKey, originalFormKey))
                {
                    var mod = state.LoadOrder[higherPriorityContext.ModKey].Mod;
                    if (mod != null && mod.MasterReferences.Any(m => m.Master.Equals(changedContext.ModKey)))
                        return ConvertFormLink(originalValue);
                }
            }

            return ConvertFormLink(changedValue);
        }

        public object? GetWinningValue(
            PropertyInfo property,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext)
        {
            var value = property.GetValue(winningContext.Record);
            return ConvertFormLink(value);
        }
    }
}
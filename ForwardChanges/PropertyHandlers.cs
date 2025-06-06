using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using System.Reflection;

namespace ForwardChanges
{
    public interface IPropertyAccessStrategy<TGetter> where TGetter : class, IMajorRecordGetter
    {
        object? GetValue(TGetter record);
    }

    public class DirectPropertyAccess<TGetter, TValue> : IPropertyAccessStrategy<TGetter> where TGetter : class, IMajorRecordGetter
    {
        private readonly Func<TGetter, TValue> _accessor;

        public DirectPropertyAccess(Func<TGetter, TValue> accessor)
        {
            _accessor = accessor;
        }

        public object? GetValue(TGetter record) => _accessor(record);
    }

    public class GroupedPropertyAccess<TGetter, TParent, TValue> : IPropertyAccessStrategy<TGetter> where TGetter : class, IMajorRecordGetter
    {
        private readonly Func<TGetter, TParent> _parentAccessor;
        private readonly Func<TParent, TValue> _childAccessor;

        public GroupedPropertyAccess(
            Func<TGetter, TParent> parentAccessor,
            Func<TParent, TValue> childAccessor)
        {
            _parentAccessor = parentAccessor;
            _childAccessor = childAccessor;
        }

        public object? GetValue(TGetter record)
        {
            var parent = _parentAccessor(record);
            return parent != null ? _childAccessor(parent) : null;
        }
    }

    public interface IPropertyHandler
    {
        object? GetForwardValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] recordContexts,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state);

        object? GetWinningValue(
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

    public class PropertyHandler<TGetter> : IPropertyHandler where TGetter : class, IMajorRecordGetter
    {
        private readonly IPropertyAccessStrategy<TGetter> _propertyAccess;
        private readonly string _propertyName;

        public PropertyHandler(string propertyName, IPropertyAccessStrategy<TGetter> propertyAccess)
        {
            _propertyName = propertyName;
            _propertyAccess = propertyAccess;
        }

        public object? GetForwardValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] recordContexts,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Console.WriteLine($"\n=== Processing {_propertyName} ===");

            if (recordContexts == null || !recordContexts.Any())
            {
                Console.WriteLine("No contexts found for record");
                return null;
            }

            // Check if we can break early
            if (PropertyHandlerUtils.ShouldBreakEarly(recordContexts))
            {
                var earlyBreakValue = _propertyAccess.GetValue((TGetter)recordContexts.Last().Record);
                Console.WriteLine($"Breaking early - returning original value: {earlyBreakValue}");
                return earlyBreakValue;
            }

            // Get values from original and winning records
            var originalContext = recordContexts.Last();
            var originalValue = _propertyAccess.GetValue((TGetter)originalContext.Record);
            Console.WriteLine($"Original value: {originalValue}");
            var winningValue = _propertyAccess.GetValue((TGetter)recordContexts[0].Record);
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
                var contextValue = _propertyAccess.GetValue((TGetter)recordContexts[i].Record);
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
            var changedValue = _propertyAccess.GetValue((TGetter)changedContext.Record);
            Console.WriteLine($"Changed mod: {changedContext.ModKey}");
            Console.WriteLine($"Changed value: {changedValue}");

            Console.WriteLine("Checking mods from change to winning for potential reverts:");
            // Check all mods from the change back to winning (higher priority)
            for (int i = 0; i < changeIndex; i++)
            {
                var higherPriorityContext = recordContexts[i];
                var contextValue = _propertyAccess.GetValue((TGetter)higherPriorityContext.Record);
                Console.WriteLine($"  Checking mod {higherPriorityContext.ModKey} with value {contextValue}");

                // If this mod has the original value, check if it can revert the change
                if (Equals(contextValue, originalValue))
                {
                    var mod = state.LoadOrder[higherPriorityContext.ModKey].Mod;
                    if (mod != null && mod.MasterReferences.Any(m => m.Master.ToString() == changedContext.ModKey.ToString()))
                    {
                        Console.WriteLine($"  Mod {higherPriorityContext.ModKey} can revert the change - returning original value");
                        return originalValue;
                    }
                }
            }

            return changedValue;
        }

        public object? GetWinningValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext)
        {
            return _propertyAccess.GetValue((TGetter)winningContext.Record);
        }
    }

    public class FormLinkPropertyHandler<TGetter> : IPropertyHandler where TGetter : class, IMajorRecordGetter
    {
        private readonly IPropertyAccessStrategy<TGetter> _propertyAccess;
        private readonly string _propertyName;

        public FormLinkPropertyHandler(string propertyName, IPropertyAccessStrategy<TGetter> propertyAccess)
        {
            _propertyName = propertyName;
            _propertyAccess = propertyAccess;
        }

        private object? ConvertFormLink(object? value)
        {
            if (value == null)
                return null;

            // Get the generic type of the FormLink
            var formLinkType = value.GetType();

            if (!formLinkType.IsGenericType)
                return value;

            // If it's already a FormLinkNullable, return it
            if (formLinkType.Name == "FormLinkNullable`1")
                return value;

            // If it's a FormLinkNullableGetter, create a new FormLinkNullable
            if (formLinkType.Name == "FormLinkNullableGetter`1")
            {
                try
                {
                    var targetType = formLinkType.GetGenericArguments()[0];
                    var nullableType = typeof(FormLinkNullable<>).MakeGenericType(targetType);
                    var target = System.Activator.CreateInstance(nullableType);

                    // Get the FormKey from the getter
                    var formKey = formLinkType.GetProperty("FormKey")?.GetValue(value);
                    if (formKey != null)
                    {
                        // Set the FormKey on the new FormLinkNullable
                        nullableType.GetProperty("FormKey")?.SetValue(target, formKey);
                    }

                    return target;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to convert FormLinkNullableGetter: {ex.Message}");
                    return value;
                }
            }

            // Use AsNullable() for conversion - this is the recommended way per Mutagen docs
            var asNullableMethod = formLinkType.GetMethod("AsNullable");
            if (asNullableMethod != null)
            {
                try
                {
                    return asNullableMethod.Invoke(value, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to convert FormLink using AsNullable(): {ex.Message}");
                    return value;
                }
            }

            return value;
        }

        public object? GetForwardValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] recordContexts,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Console.WriteLine($"\n=== Processing {_propertyName} ===");

            if (recordContexts == null || !recordContexts.Any())
            {
                Console.WriteLine("No contexts found for record");
                return null;
            }

            // Check if we can break early
            if (PropertyHandlerUtils.ShouldBreakEarly(recordContexts))
            {
                var earlyBreakValue = _propertyAccess.GetValue((TGetter)recordContexts.Last().Record);
                Console.WriteLine($"Breaking early - returning original value: {earlyBreakValue}");
                return ConvertFormLink(earlyBreakValue);
            }

            // Get values from original and winning records
            var originalContext = recordContexts.Last();
            var originalValue = _propertyAccess.GetValue((TGetter)originalContext.Record);
            Console.WriteLine($"Original value: {originalValue}");
            var winningValue = _propertyAccess.GetValue((TGetter)recordContexts[0].Record);
            Console.WriteLine($"Winning value: {winningValue}");

            // Compare FormLinks by their FormKey
            var originalFormKey = originalValue?.GetType().GetProperty("FormKey")?.GetValue(originalValue);
            var winningFormKey = winningValue?.GetType().GetProperty("FormKey")?.GetValue(winningValue);

            if (!Equals(winningFormKey, originalFormKey))
            {
                Console.WriteLine("Winning value differs from original, it is the last valid changed value");
                return ConvertFormLink(winningValue);
            }

            Console.WriteLine($"Found {recordContexts.Length} contexts for record");

            // Find the first change from winning to original
            int changeIndex = -1;
            for (int i = 0; i < recordContexts.Length; i++)
            {
                var contextValue = _propertyAccess.GetValue((TGetter)recordContexts[i].Record);
                var contextFormKey = contextValue?.GetType().GetProperty("FormKey")?.GetValue(contextValue);
                Console.WriteLine($"Context value: {contextValue} from {recordContexts[i].ModKey}");
                if (!Equals(contextFormKey, originalFormKey))
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
                return ConvertFormLink(originalValue);
            }

            // Cache the changed mod and value
            var changedContext = recordContexts[changeIndex];
            var changedValue = _propertyAccess.GetValue((TGetter)changedContext.Record);
            Console.WriteLine($"Changed mod: {changedContext.ModKey}");
            Console.WriteLine($"Changed value: {changedValue}");

            Console.WriteLine("Checking mods from change to winning for potential reverts:");
            // Check all mods from the change back to winning (higher priority)
            for (int i = 0; i < changeIndex; i++)
            {
                var higherPriorityContext = recordContexts[i];
                var contextValue = _propertyAccess.GetValue((TGetter)higherPriorityContext.Record);
                var contextFormKey = contextValue?.GetType().GetProperty("FormKey")?.GetValue(contextValue);
                Console.WriteLine($"  Checking mod {higherPriorityContext.ModKey} with value {contextValue}");

                // If this mod has the original value, check if it can revert the change
                if (Equals(contextFormKey, originalFormKey))
                {
                    var mod = state.LoadOrder[higherPriorityContext.ModKey].Mod;
                    if (mod != null && mod.MasterReferences.Any(m => m.Master.ToString() == changedContext.ModKey.ToString()))
                    {
                        Console.WriteLine($"  Mod {higherPriorityContext.ModKey} can revert the change - returning original value");
                        return ConvertFormLink(originalValue);
                    }
                }
            }

            // If the changed value is null or has a null FormKey, we want to remove the FormLink entirely
            if (changedValue == null || (changedValue is IFormLinkGetter formLinkGetter && formLinkGetter.IsNull))
            {
                return null;
            }

            var result = ConvertFormLink(changedValue);
            Console.WriteLine($"Last changed value: {result}");
            return result;
        }

        public object? GetWinningValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext)
        {
            var value = _propertyAccess.GetValue((TGetter)winningContext.Record);
            return ConvertFormLink(value);
        }
    }

    public class ProtectionFlagsHandler : IPropertyHandler
    {
        public object? GetForwardValue(
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
            var winningValue = GetWinningValue(recordContexts[0]);
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

    public class FactionListHandler : IPropertyHandler
    {
        private class FactionState
        {
            public required IRankPlacementGetter RankPlacement { get; set; }
            public required string LastChangedByMod { get; set; }
            public int OriginalIndex { get; set; }
            public sbyte Rank { get; set; }
            public required string RankChangedByMod { get; set; }
        }

        public object? GetForwardValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] recordContexts,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Console.WriteLine($"\n=== Processing Factions ===");

            if (recordContexts == null || !recordContexts.Any())
            {
                Console.WriteLine("No contexts found for record");
                return null;
            }

            // Check if we can break early
            if (PropertyHandlerUtils.ShouldBreakEarly(recordContexts))
            {
                var earlyBreakValue = GetWinningValue(recordContexts[0]);
                Console.WriteLine($"Breaking early - returning winning value: {GetFactionListString(recordContexts[0])}");
                return earlyBreakValue;
            }

            // Get values from original and winning records
            var originalContext = recordContexts.Last();
            var winningContext = recordContexts[0];
            Console.WriteLine($"Original value: {GetFactionListString(originalContext)}");
            Console.WriteLine($"Winning value: {GetFactionListString(winningContext)}");
            Console.WriteLine($"Found {recordContexts.Length} contexts for record");

            var factionStates = new List<FactionState>();
            var currentOrder = new List<IRankPlacementGetter>();
            List<IRankPlacementGetter>? finalResult = null;

            // Process contexts in reverse order (original first)
            foreach (var context in recordContexts.Reverse())
            {
                Console.WriteLine($"Processing mod: {context.ModKey}");
                Console.WriteLine($"Current mod's masters: {string.Join(", ", state.LoadOrder[context.ModKey].Mod?.MasterReferences.Select(m => m.Master.FileName.ToString()) ?? Enumerable.Empty<string>())}");

                if (context.Record is not INpcGetter npc)
                {
                    Console.WriteLine("Record is not an NPC, skipping");
                    continue;
                }

                var currentFactions = npc.Factions.ToList();
                Console.WriteLine($"Found {currentFactions.Count} factions in {context.ModKey}");
                Console.WriteLine($"Current factions: {GetFactionListString(context)}");

                // First, check for removals - create a copy of the list to avoid modification during iteration
                var factionsToRemove = factionStates
                    .Where(s => s.RankPlacement != null && !currentFactions.Any(f => f.Faction != null && f.Faction.FormKey.Equals(s.RankPlacement.Faction.FormKey)))
                    .ToArray();

                // Collect changes first, then apply them
                var factionsToRemoveFromStates = new List<FactionState>();
                var factionsToRemoveFromOrder = new List<IRankPlacementGetter>();

                foreach (var factionState in factionsToRemove)
                {
                    if (factionState.RankPlacement == null) continue;

                    var currentMod = state.LoadOrder[context.ModKey].Mod;
                    var canRemove = currentMod?.MasterReferences.Any(m => m.Master.ToString() == factionState.LastChangedByMod) == true;

                    if (canRemove)
                    {
                        factionsToRemoveFromStates.Add(factionState);
                        factionsToRemoveFromOrder.Add(factionState.RankPlacement);
                    }
                }

                // Create a copy of currentFactions to avoid modification during iteration
                var currentFactionsArray = currentFactions.Where(f => f.Faction != null).ToArray();

                // Collect changes first, then apply them
                var factionsToAddToStates = new List<FactionState>();
                var factionsToAddToOrder = new List<IRankPlacementGetter>();
                var factionsToUpdate = new List<(FactionState State, string NewMod, sbyte NewRank)>();

                foreach (var faction in currentFactionsArray)
                {
                    if (faction.Faction == null) continue;

                    var existingState = factionStates.FirstOrDefault(s =>
                        s.RankPlacement != null && s.RankPlacement.Faction.FormKey.Equals(faction.Faction.FormKey));

                    if (existingState == null)
                    {
                        // New faction
                        var newState = new FactionState
                        {
                            RankPlacement = faction,
                            LastChangedByMod = context.ModKey.ToString(),
                            OriginalIndex = factionStates.Count,
                            Rank = faction.Rank,
                            RankChangedByMod = context.ModKey.ToString()
                        };
                        factionsToAddToStates.Add(newState);
                        factionsToAddToOrder.Add(faction);
                    }
                    else
                    {
                        // Check if current mod has the last changed mod as master
                        var currentMod = state.LoadOrder[context.ModKey].Mod;
                        var canModify = currentMod?.MasterReferences.Any(m => m.Master.ToString() == existingState.LastChangedByMod) == true;

                        if (canModify)
                        {
                            // Update rank if changed
                            if (faction.Rank != existingState.Rank)
                            {
                                factionsToUpdate.Add((existingState, context.ModKey.ToString(), faction.Rank));
                            }
                        }
                    }
                }

                // Apply removals
                foreach (var factionStateToRemove in factionsToRemoveFromStates)
                {
                    factionStates.Remove(factionStateToRemove);
                }
                foreach (var faction in factionsToRemoveFromOrder)
                {
                    currentOrder.Remove(faction);
                }

                // Apply additions
                foreach (var factionStateToAdd in factionsToAddToStates)
                {
                    factionStates.Add(factionStateToAdd);
                }
                foreach (var faction in factionsToAddToOrder)
                {
                    currentOrder.Add(faction);
                }

                // Apply updates
                foreach (var (factionStateToUpdate, newMod, newRank) in factionsToUpdate)
                {
                    factionStateToUpdate.LastChangedByMod = newMod;
                    factionStateToUpdate.Rank = newRank;
                    factionStateToUpdate.RankChangedByMod = newMod;
                }

                // Get the current result after processing this context
                finalResult = currentOrder
                    .Where(faction => faction.Faction != null && factionStates.Any(s => s.RankPlacement != null && s.RankPlacement.Faction.FormKey.Equals(faction.Faction.FormKey)))
                    .ToList();

                Console.WriteLine($"Final faction list ({finalResult.Count} factions):");
                foreach (var faction in finalResult)
                {
                    if (faction.Faction == null) continue;
                    var factionState = factionStates.First(s => s.RankPlacement != null && s.RankPlacement.Faction.FormKey.Equals(faction.Faction.FormKey));
                    Console.WriteLine($"  {faction.Faction} (Rank: {factionState.Rank}, Last changed by: {factionState.LastChangedByMod})");
                }

                Console.WriteLine($"Last changed value: {GetFactionListString(context)}");
            }

            return finalResult;
        }

        private string GetFactionListString(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is not INpcGetter npc)
                return "Not an NPC";

            var factions = npc.Factions
                .Where(f => f.Faction != null)
                .Select(f => $"{f.Faction} (Rank: {f.Rank})");

            return factions.Any()
                ? string.Join(", ", factions)
                : "No factions";
        }

        public object? GetWinningValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext)
        {
            if (winningContext.Record is INpcGetter npc)
            {
                return npc.Factions.ToList();
            }
            return null;
        }
    }
}
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using System.Reflection;
using System.Collections.Generic;

namespace ForwardChanges.RecordHandlers
{
    public static class NpcRecordHandler
    {
        private static readonly Dictionary<string, IPropertyHandler> propertyHandlers = new()
        {
            { "EditorID", new SimplePropertyHandler() },
            { "Name", new SimplePropertyHandler() },
            { "Class", new SimplePropertyHandler() },
            { "AIData.Confidence", new GroupedPropertyHandler<INpcGetter>("AIData", "Confidence") },
            { "Configuration.Flags", new ProtectionFlagsHandler() },
            { "DeathItem", new FormLinkPropertyHandler() },
            { "CombatOverridePackageList", new SimplePropertyHandler() },
            { "SpectatorOverridePackageList", new SimplePropertyHandler() }
        };

        public static void ProcessNpcRecords(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var winningContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, INpc, INpcGetter>(state.LinkCache);
            foreach (var winningContext in winningContexts)
            {
                Console.WriteLine("\n" + new string('-', 80));
                Console.WriteLine($"Processing NPC: {winningContext.Record.EditorID}");
                Console.WriteLine(new string('-', 80));
                var propertiesToForward = new Dictionary<string, object?>();

                // Get all contexts for this record in load order using ResolveAllContexts
                var recordContexts = winningContext.Record.ToLink<INpcGetter>()
                    .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, INpc, INpcGetter>(state.LinkCache)
                    .ToArray();

                foreach (var (propName, propertyHandler) in propertyHandlers)
                {
                    if (winningContext.Record is INpcGetter npcGetter)
                    {
                        // Get the property info for the winning record
                        if (npcGetter.GetType().GetProperty(propName.Split('.')[0]) is PropertyInfo property)
                        {
                            var lastChangedValue = propertyHandler.GetLastChangedValue(property, recordContexts, state);
                            Console.WriteLine($"Last changed value: {lastChangedValue}");
                            var winningValue = propertyHandler.GetWinningValue(property, winningContext);

                            // Special handling for protection flags
                            if (propName == "Configuration.Flags")
                            {
                                var lastChangedFlag = lastChangedValue as NpcConfiguration.Flag?;
                                var winningFlag = winningValue as NpcConfiguration.Flag?;

                                // If last changed has higher protection, forward it
                                if (lastChangedFlag == NpcConfiguration.Flag.Essential && winningFlag != NpcConfiguration.Flag.Essential)
                                {
                                    Console.WriteLine("Forwarding Essential flag");
                                    propertiesToForward[propName] = lastChangedValue;
                                }
                                else if (lastChangedFlag == NpcConfiguration.Flag.Protected && winningFlag == null)
                                {
                                    Console.WriteLine("Forwarding Protected flag");
                                    propertiesToForward[propName] = lastChangedValue;
                                }
                                continue;
                            }

                            // For all other properties, use normal comparison
                            if (!Equals(winningValue, lastChangedValue))
                            {
                                Console.WriteLine("Values differ, forwarding last changed value");
                                propertiesToForward[propName] = lastChangedValue;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Property {propName} not found on type {npcGetter.GetType().Name}");
                        }
                    }
                }

                if (propertiesToForward.Count > 0)
                {
                    var overrideNpc = state.PatchMod.Npcs.GetOrAddAsOverride(winningContext.Record);
                    ApplyForwardedProperties(overrideNpc, propertiesToForward);
                }
            }
        }

        private static void ApplyForwardedProperties(INpc record, Dictionary<string, object?> propertiesToForward)
        {
            Console.WriteLine();
            foreach (var (propName, value) in propertiesToForward)
            {
                if (TryApplyProtectionFlags(record, propName, value))
                    continue;

                if (TryApplyFormLink(record, propName, value))
                    continue;

                ApplyStandardProperty(record, propName, value);
            }
        }

        private static bool TryApplyProtectionFlags(INpc record, string propName, object? value)
        {
            if (propName != "Configuration.Flags" || value is not NpcConfiguration.Flag flag)
                return false;

            var currentFlags = record.Configuration.Flags;
            if (flag == NpcConfiguration.Flag.Essential)
            {
                // Set Essential and remove Protected
                record.Configuration.Flags = (currentFlags & ~NpcConfiguration.Flag.Protected) | NpcConfiguration.Flag.Essential;
                Console.WriteLine($"Forwarded Configuration.Flags: {currentFlags} -> {record.Configuration.Flags}");
            }
            else if (flag == NpcConfiguration.Flag.Protected)
            {
                // Set Protected only if not Essential
                if (!currentFlags.HasFlag(NpcConfiguration.Flag.Essential))
                {
                    record.Configuration.Flags = currentFlags | NpcConfiguration.Flag.Protected;
                    Console.WriteLine($"Forwarded Configuration.Flags: {currentFlags} -> {record.Configuration.Flags}");
                }
            }
            return true;
        }

        private static bool TryApplyFormLink(INpc record, string propName, object? value)
        {
            var property = record.GetType().GetProperty(propName);
            if (property == null)
                return false;

            var oldValue = property.GetValue(record);
            if (oldValue?.GetType().Name.Contains("FormLink") != true && value?.GetType().Name.Contains("FormLink") != true)
                return false;

            try
            {
                property.SetValue(record, value);
                Console.WriteLine($"Forwarded {propName}: {oldValue?.ToString() ?? "Null"} -> {value?.ToString() ?? "Null"}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting FormLink property {propName}: {ex.Message}");
                return false;
            }
        }

        private static void ApplyStandardProperty(INpc record, string propName, object? value)
        {
            var property = record.GetType().GetProperty(propName);
            if (property != null)
            {
                try
                {
                    var oldValue = property.GetValue(record);
                    property.SetValue(record, value);
                    Console.WriteLine($"Forwarded {propName}: {oldValue?.ToString() ?? "Null"} -> {value?.ToString() ?? "Null"}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting property {propName}: {ex.Message}");
                }
            }
        }
    }
}
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim.Internals;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ForwardChanges.RecordHandlers
{
    public static class NpcRecordHandler
    {
        private static readonly Dictionary<string, IPropertyHandler> propertyHandlers = new()
        {
            // Direct properties - properties directly on the NPC record
            { "EditorID", new PropertyHandler<INpcGetter>("EditorID",
                new DirectPropertyAccess<INpcGetter, string?>(npc => npc.EditorID)) },
            { "Name", new PropertyHandler<INpcGetter>("Name",
                new DirectPropertyAccess<INpcGetter, ITranslatedStringGetter?>(npc => npc.Name)) },
            { "Class", new PropertyHandler<INpcGetter>("Class",
                new DirectPropertyAccess<INpcGetter, IFormLinkGetter<IClassGetter>?>(npc => npc.Class)) },
            { "DeathItem", new FormLinkPropertyHandler<INpcGetter>("DeathItem",
                new DirectPropertyAccess<INpcGetter, IFormLinkGetter<IItemGetter>?>(npc => npc.DeathItem)) },
            { "CombatOverridePackageList", new PropertyHandler<INpcGetter>("CombatOverridePackageList",
                new DirectPropertyAccess<INpcGetter, IFormLinkNullableGetter<IFormListGetter>?>(npc => npc.CombatOverridePackageList)) },
            { "SpectatorOverridePackageList", new PropertyHandler<INpcGetter>("SpectatorOverridePackageList",
                new DirectPropertyAccess<INpcGetter, IFormLinkNullableGetter<IFormListGetter>?>(npc => npc.SpectatorOverridePackageList)) },

            // Grouped properties - properties on groups within the NPC record
            { "AIData.Confidence", new PropertyHandler<INpcGetter>("AIData.Confidence",
                new GroupedPropertyAccess<INpcGetter, IAIDataGetter, Confidence>(
                    npc => npc.AIData,
                    aiData => aiData.Confidence)) },
            { "Configuration.Flags", new ProtectionFlagsHandler() },
            { "Factions", new FactionListHandler() }
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
                    var forwardValue = propertyHandler.GetForwardValue(recordContexts, state);
                    Console.WriteLine($"Last changed value: {forwardValue}");
                    var winningValue = propertyHandler.GetWinningValue(winningContext);

                    // For all other properties, use normal comparison
                    if (!Equals(winningValue, forwardValue))
                    {
                        Console.WriteLine("Values differ, forwarding new value");
                        propertiesToForward[propName] = forwardValue;
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

                if (TryApplyFactionList(record, propName, value))
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
                // Get the FormKey values for better logging
                var oldFormKey = oldValue?.GetType().GetProperty("FormKey")?.GetValue(oldValue);
                var newFormKey = value?.GetType().GetProperty("FormKey")?.GetValue(value);

                Console.WriteLine($"DEBUG: Property type: {property.PropertyType}");
                Console.WriteLine($"DEBUG: Old value type: {oldValue?.GetType()}");
                Console.WriteLine($"DEBUG: New value type: {value?.GetType()}");
                Console.WriteLine($"DEBUG: Old FormKey: {oldFormKey}");
                Console.WriteLine($"DEBUG: New FormKey: {newFormKey}");

                // If the new value is null or has a null FormKey, we want to remove the FormLink entirely
                if (value == null || (value is IFormLinkGetter formLinkGetter && formLinkGetter.IsNull))
                {
                    // Get the existing FormLink from the record
                    var formLink = property.GetValue(record);
                    var formLinkType = formLink?.GetType() ?? property.PropertyType;

                    if (formLinkType.IsGenericType && formLinkType.GetGenericTypeDefinition() == typeof(FormLinkNullable<>))
                    {
                        // Get the specific SetTo method that takes a nullable FormKey
                        var setToMethod = formLinkType.GetMethod("SetTo", new[] { typeof(FormKey?) });

                        if (setToMethod != null)
                        {
                            try
                            {
                                setToMethod.Invoke(formLink, new object?[] { null });
                                Console.WriteLine($"Removed FormLink {propName}: {oldFormKey?.ToString() ?? "Null"} -> Null");
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error invoking SetTo: {ex.Message}");
                            }
                        }
                    }
                }

                property.SetValue(record, value);
                Console.WriteLine($"Forwarded {propName}: {oldFormKey?.ToString() ?? "Null"} -> {newFormKey?.ToString() ?? "Null"}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting FormLink property {propName}: {ex.Message}");
                return false;
            }
        }

        private static bool TryApplyFactionList(INpc record, string propName, object? value)
        {
            if (propName != "Factions" || value is not List<IRankPlacementGetter> factions)
                return false;

            try
            {
                // Create a copy of the factions list to avoid modification during iteration
                var factionsArray = factions.Where(f => f.Faction != null).ToArray();

                // Create a list to store the new factions
                var newFactions = new List<(IFormLinkGetter<IFactionGetter> Faction, sbyte Rank)>();

                // Process each faction
                foreach (var faction in factionsArray)
                {
                    if (faction.Faction == null) continue;
                    newFactions.Add((faction.Faction, faction.Rank));
                }

                // Clear existing factions
                record.Factions.Clear();

                // Add each faction with its rank
                foreach (var (faction, rank) in newFactions)
                {
                    var newFaction = new RankPlacement();
                    newFaction.Faction.SetTo(faction);
                    newFaction.Rank = rank;
                    record.Factions.Add(newFaction);
                }

                Console.WriteLine($"Forwarded {propName}: {newFactions.Count} factions");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting faction list: {ex.Message}");
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
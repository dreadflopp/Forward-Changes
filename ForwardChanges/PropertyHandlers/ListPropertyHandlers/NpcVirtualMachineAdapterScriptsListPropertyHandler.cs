using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Noggog;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class NpcVirtualMachineAdapterScriptsListPropertyHandler : AbstractListPropertyHandler<ScriptEntry>
    {
        public override string PropertyName => "VirtualMachineAdapter.Scripts";

        public NpcVirtualMachineAdapterScriptsListPropertyHandler()
        {
        }

        /// <summary>
        /// Formats a list of scripts into a string.
        /// </summary>
        /// <param name="scripts">The list of scripts to format.</param>
        /// <returns>A string representation of the scripts.</returns>
        public string FormatScriptsList(List<ScriptEntry>? scripts)
        {
            if (scripts == null || scripts.Count == 0)
                return "No scripts";

            return string.Join(", ", scripts.Select(s => FormatItem(s)));
        }

        /// <summary>
        /// Checks if two scripts are equal by name only.
        /// </summary>
        /// <param name="item1">The first script to compare.</param>
        /// <param name="item2">The second script to compare.</param>
        /// <returns>True if the scripts have the same name, false otherwise.</returns>
        protected override bool IsItemEqual(ScriptEntry? item1, ScriptEntry? item2)
        {
            if (ReferenceEquals(item1, item2)) return true;
            if (item1 is null || item2 is null) return false;
            return string.Equals(item1.Name, item2.Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Formats a script into a string.
        /// </summary>
        /// <param name="item">The script to format.</param>
        /// <returns>A string representation of the script.</returns>
        protected override string FormatItem(ScriptEntry? item)
        {
            if (item == null) return "null";
            return item.Name;
        }

        /// <summary>
        /// Sets the scripts of an NPC's VirtualMachineAdapter.
        /// </summary>
        /// <param name="record">The NPC to set the scripts of.</param>
        /// <param name="value">The scripts to set.</param>
        public override void SetValue(IMajorRecord record, List<ScriptEntry>? value)
        {
            if (record is INpc npc)
            {
                if (value == null || value.Count == 0)
                {
                    // Clear scripts - set VirtualMachineAdapter to null if no scripts
                    npc.VirtualMachineAdapter = null;
                }
                else
                {
                    // Ensure VirtualMachineAdapter exists
                    if (npc.VirtualMachineAdapter == null)
                    {
                        npc.VirtualMachineAdapter = new VirtualMachineAdapter();
                    }

                    // Clear existing scripts and add new ones
                    npc.VirtualMachineAdapter.Scripts.Clear();
                    foreach (var script in value)
                    {
                        if (script == null) continue;

                        // Create a deep copy of the script
                        var newScript = new ScriptEntry
                        {
                            Name = script.Name,
                            Flags = script.Flags
                        };

                        // Copy properties (simplified - just copy the structure)
                        if (script.Properties != null)
                        {
                            foreach (var property in script.Properties)
                            {
                                if (property == null) continue;

                                var newProperty = new ScriptProperty
                                {
                                    Name = property.Name,
                                    Flags = property.Flags
                                };

                                // Add the property to the new script
                                newScript.Properties.Add(newProperty);
                            }
                        }

                        npc.VirtualMachineAdapter.Scripts.Add(newScript);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            }
        }

        /// <summary>
        /// Gets the scripts of an NPC's VirtualMachineAdapter.
        /// </summary>
        /// <param name="record">The NPC to get the scripts from.</param>
        /// <returns>The scripts of the NPC.</returns>
        public override List<ScriptEntry>? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
            {
                if (npc.VirtualMachineAdapter?.Scripts == null)
                {
                    return null;
                }

                // Return a copy of the scripts to avoid modification issues
                return npc.VirtualMachineAdapter.Scripts.Select(script => new ScriptEntry
                {
                    Name = script.Name,
                    Flags = script.Flags,
                    Properties = new ExtendedList<ScriptProperty>(script.Properties?.Select(prop => new ScriptProperty
                    {
                        Name = prop.Name,
                        Flags = prop.Flags
                    }) ?? Enumerable.Empty<ScriptProperty>())
                }).ToList();
            }
            Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            return null;
        }
    }
}
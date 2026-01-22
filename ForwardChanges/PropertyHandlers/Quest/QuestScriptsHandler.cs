using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class QuestScriptsHandler : AbstractListPropertyHandler<IScriptEntryGetter>
    {
        public override string PropertyName => "QuestScripts";

        // Static dictionary to persist property ownership trackers across mod processing
        // Key: Record FormKey + Script Name, Value: Property ownership tracker
        private static readonly Dictionary<string, ScriptPropertyOwnershipTracker> _persistentTrackers = new();

        public override List<IScriptEntryGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IQuestGetter questRecord && questRecord.VirtualMachineAdapter != null)
            {
                return questRecord.VirtualMachineAdapter.Scripts?.ToList();
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IScriptEntryGetter>? value)
        {
            if (record is IQuest questRecord && questRecord.VirtualMachineAdapter != null && value != null)
            {
                if (questRecord.VirtualMachineAdapter.Scripts != null)
                {
                    questRecord.VirtualMachineAdapter.Scripts.Clear();
                    foreach (var script in value)
                    {
                        if (script == null) continue;

                        // Create a deep copy of the entire script using DeepCopyIn
                        var newScript = new ScriptEntry();
                        newScript.DeepCopyIn(script);
                        questRecord.VirtualMachineAdapter.Scripts.Add(newScript);
                    }
                }
            }
        }

        public override bool AreValuesEqual(List<IScriptEntryGetter>? value1, List<IScriptEntryGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            // Group scripts by name for efficient matching
            var scripts1ByName = value1
                .Where(s => s != null)
                .GroupBy(s => s.Name)
                .ToDictionary(g => g.Key, g => g.First());

            var scripts2ByName = value2
                .Where(s => s != null)
                .GroupBy(s => s.Name)
                .ToDictionary(g => g.Key, g => g.First());

            // Check that all scripts in value1 have matching scripts in value2 with same properties
            foreach (var script1 in scripts1ByName.Values)
            {
                if (!scripts2ByName.TryGetValue(script1.Name, out var script2))
                {
                    return false; // Script name not found in value2
                }

                // Compare scripts by name, flags, and properties
                if (!AreScriptsEqual(script1, script2))
                {
                    return false; // Scripts differ
                }
            }

            return true;
        }

        private bool AreScriptsEqual(IScriptEntryGetter script1, IScriptEntryGetter script2)
        {
            // Compare name
            if (script1.Name != script2.Name) return false;

            // Compare flags
            if (script1.Flags != script2.Flags) return false;

            // Compare properties (order-independent)
            var props1 = script1.Properties?.ToList() ?? new List<IScriptPropertyGetter>();
            var props2 = script2.Properties?.ToList() ?? new List<IScriptPropertyGetter>();

            if (props1.Count != props2.Count) return false;

            // For each property in script1, find a matching property in script2
            var props2Matched = new HashSet<int>();
            foreach (var prop1 in props1)
            {
                if (prop1 == null) continue;

                bool found = false;
                for (int i = 0; i < props2.Count; i++)
                {
                    if (props2Matched.Contains(i)) continue;
                    if (props2[i] == null) continue;

                    if (AreScriptPropertiesEqual(prop1, props2[i]))
                    {
                        props2Matched.Add(i);
                        found = true;
                        break;
                    }
                }

                if (!found) return false;
            }

            return true;
        }

        protected override bool IsItemEqual(IScriptEntryGetter? item1, IScriptEntryGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Match scripts by name only - properties are handled separately in ProcessHandlerSpecificLogic
            return item1.Name == item2.Name;
        }

        protected override void ProcessHandlerSpecificLogic(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            ListPropertyContext<IScriptEntryGetter> listPropertyContext,
            List<IScriptEntryGetter> recordItems,
            List<ListPropertyValueContext<IScriptEntryGetter>> currentForwardItems)
        {
            var recordMod = state.LoadOrder[context.ModKey].Mod;
            if (recordMod == null) return;

            // Get original scripts to determine which properties were originally present
            var originalScripts = listPropertyContext.OriginalValueContexts?
                .Where(c => c != null && c.Value != null)
                .Select(c => c.Value)
                .ToList() ?? new List<IScriptEntryGetter>();
            var originalScriptsByName = originalScripts
                .Where(s => s != null)
                .GroupBy(s => s.Name)
                .ToDictionary(g => g.Key, g => g.First());

            // Group scripts by name for efficient matching
            var forwardScriptsByName = currentForwardItems
                .Where(i => !i.IsRemoved)
                .GroupBy(i => i.Value.Name)
                .ToDictionary(g => g.Key, g => g.First());

            // Get the record FormKey to create unique keys for trackers
            var recordFormKey = context.Record.FormKey.ToString();

            // Get original mod name (all original items have the same OwnerMod)
            var originalMod = listPropertyContext.OriginalValueContexts?
                .FirstOrDefault()?.OwnerMod ?? "Unknown";

            // Get or create persistent trackers for each script
            // Key: script name, Value: ownership tracker
            var propertyTrackers = new Dictionary<string, ScriptPropertyOwnershipTracker>();

            // Initialize trackers from original scripts (only if not already initialized)
            foreach (var originalScript in originalScripts)
            {
                if (originalScript == null) continue;
                var trackerKey = $"{recordFormKey}|{originalScript.Name}";

                if (!_persistentTrackers.TryGetValue(trackerKey, out var tracker))
                {
                    tracker = new ScriptPropertyOwnershipTracker();
                    tracker.InitializeFromOriginal(originalScript, originalMod);
                    _persistentTrackers[trackerKey] = tracker;
                }

                propertyTrackers[originalScript.Name] = tracker;
            }

            // Process each script in the record
            foreach (var recordScript in recordItems)
            {
                if (recordScript == null) continue;

                // Find matching forward script by name
                if (!forwardScriptsByName.TryGetValue(recordScript.Name, out var forwardContext))
                {
                    // Script doesn't exist in forward - will be handled by ProcessAdditions
                    continue;
                }

                // Check if we have permission to modify this script
                if (!HasPermissionsToModify(recordMod, forwardContext.OwnerMod))
                {
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Cannot modify script '{recordScript.Name}' - no permission (owned by {forwardContext.OwnerMod})");
                    continue;
                }

                // Get original script for this script name (if it exists)
                originalScriptsByName.TryGetValue(recordScript.Name, out var originalScript);

                // Get or create property ownership tracker for this script (persistent across mod processing)
                var trackerKey = $"{recordFormKey}|{recordScript.Name}";

                if (!_persistentTrackers.TryGetValue(trackerKey, out var tracker))
                {
                    tracker = new ScriptPropertyOwnershipTracker();
                    if (originalScript != null)
                    {
                        tracker.InitializeFromOriginal(originalScript, originalMod);
                    }
                    _persistentTrackers[trackerKey] = tracker;
                }

                // IMPORTANT: Also initialize from forward script to track properties added by previous mods
                // This is critical - properties added by previous mods need to be in the tracker
                // But only add properties that aren't already tracked (to preserve ownership)
                tracker.InitializeFromForward(forwardContext.Value, originalScript, forwardContext.OwnerMod);

                // Merge properties: create a new script with merged properties
                var mergedScript = MergeScriptProperties(
                    forwardContext.Value,
                    recordScript,
                    originalScript,
                    context.ModKey.ToString(),
                    recordMod,
                    forwardContext.OwnerMod,
                    tracker);

                if (mergedScript != null)
                {
                    forwardContext.Value = mergedScript;
                }
            }
        }

        private IScriptEntryGetter? MergeScriptProperties(
            IScriptEntryGetter forwardScript,
            IScriptEntryGetter recordScript,
            IScriptEntryGetter? originalScript,
            string newOwnerMod,
            ISkyrimModGetter recordMod,
            string scriptOwnerMod,
            ScriptPropertyOwnershipTracker propertyTracker)
        {
            // Create a new ScriptEntry based on the forward script
            var mergedScript = new ScriptEntry();
            mergedScript.DeepCopyIn(forwardScript);

            // Update flags if they differ
            if (forwardScript.Flags != recordScript.Flags)
            {
                mergedScript.Flags = recordScript.Flags;
            }

            var forwardProperties = forwardScript.Properties?.ToList() ?? new List<IScriptPropertyGetter>();
            var recordProperties = recordScript.Properties?.ToList() ?? new List<IScriptPropertyGetter>();
            var originalProperties = originalScript?.Properties?.ToList() ?? new List<IScriptPropertyGetter>();

            // Find properties to add (exist in record but not in forward)
            foreach (var recordProp in recordProperties)
            {
                if (recordProp == null) continue;

                // Check if this property exists in forward script (by value comparison)
                bool existsInForward = forwardProperties.Any(fp =>
                    fp != null && AreScriptPropertiesEqual(fp, recordProp));

                if (!existsInForward)
                {
                    // Check if property was previously removed
                    var ownership = propertyTracker.GetOwnership(recordProp);
                    bool wasRemoved = ownership?.IsRemoved == true;

                    if (wasRemoved && ownership != null)
                    {
                        // Property was removed - check if we have permission to add it back
                        bool hasPermission = HasPermissionsToModify(recordMod, ownership.OwnerMod);

                        if (hasPermission)
                        {
                            // Add the property back
                            var newProp = CreateScriptPropertyCopy(recordProp);
                            if (newProp != null)
                            {
                                mergedScript.Properties.Add(newProp);
                                propertyTracker.MarkPropertyAddedBack(recordProp, newOwnerMod);
                                LogCollector.Add(PropertyName, $"[{PropertyName}] Adding back property '{recordProp.Name}' to script '{recordScript.Name}' (was removed by {ownership.OwnerMod})");
                            }
                        }
                        else
                        {
                            LogCollector.Add(PropertyName, $"[{PropertyName}] Cannot add back property '{recordProp.Name}' to script '{recordScript.Name}' - no permission (was removed by {ownership.OwnerMod})");
                        }
                    }
                    else
                    {
                        // New property - add it and take ownership
                        var newProp = CreateScriptPropertyCopy(recordProp);
                        if (newProp != null)
                        {
                            mergedScript.Properties.Add(newProp);
                            propertyTracker.MarkPropertyAdded(recordProp, newOwnerMod);
                            LogCollector.Add(PropertyName, $"[{PropertyName}] Adding property '{recordProp.Name}' to script '{recordScript.Name}' (taking ownership as '{newOwnerMod}')");
                        }
                    }
                }
            }

            // Find properties to remove (exist in forward but not in record)
            var propertiesToRemove = new List<IScriptPropertyGetter>();
            foreach (var forwardProp in forwardProperties)
            {
                if (forwardProp == null) continue;

                // Check if this property exists in record script (by value comparison)
                bool existsInRecord = recordProperties.Any(rp =>
                    rp != null && AreScriptPropertiesEqual(rp, forwardProp));

                if (!existsInRecord)
                {
                    // Get ownership info for this property
                    var ownership = propertyTracker.GetOwnership(forwardProp);

                    if (ownership != null)
                    {
                        // Property is tracked - check if we have permission to remove it
                        bool hasPermission = HasPermissionsToModify(recordMod, ownership.OwnerMod);

                        if (hasPermission)
                        {
                            // We have permission - mark for removal
                            propertiesToRemove.Add(forwardProp);
                        }
                        else
                        {
                            // No permission to remove
                            LogCollector.Add(PropertyName, $"[{PropertyName}] Cannot remove property '{forwardProp.Name}' from script '{recordScript.Name}' - no permission (owned by {ownership.OwnerMod})");
                        }
                    }
                    else
                    {
                        // Property not tracked - this shouldn't happen if initialization worked correctly
                        // But be safe: if it exists in original, we can remove it (permission already checked at script level)
                        bool existsInOriginal = originalProperties.Any(op =>
                            op != null && AreScriptPropertiesEqual(op, forwardProp));

                        if (existsInOriginal)
                        {
                            // Property was in original but not tracked - add to tracker and remove
                            propertyTracker.MarkPropertyRemoved(forwardProp, newOwnerMod);
                            propertiesToRemove.Add(forwardProp);
                        }
                        else
                        {
                            LogCollector.Add(PropertyName, $"[{PropertyName}] Cannot remove property '{forwardProp.Name}' from script '{recordScript.Name}' - property not tracked and not in original");
                        }
                    }
                }
            }

            // Remove properties that don't exist in record
            foreach (var propToRemove in propertiesToRemove)
            {
                // Find and remove the matching property from mergedScript
                var propToRemoveFromMerged = mergedScript.Properties.FirstOrDefault(p =>
                    p != null && AreScriptPropertiesEqual(p, propToRemove));

                if (propToRemoveFromMerged != null)
                {
                    var oldOwnership = propertyTracker.GetOwnership(propToRemove);
                    mergedScript.Properties.Remove(propToRemoveFromMerged);
                    propertyTracker.MarkPropertyRemoved(propToRemove, newOwnerMod);
                    LogCollector.Add(PropertyName, $"[{PropertyName}] Removing property '{propToRemove.Name}' from script '{recordScript.Name}' (was owned by '{oldOwnership?.OwnerMod ?? "unknown"}', new owner: '{newOwnerMod}')");
                }
            }

            // Update properties that exist in both but have different values
            // Note: Properties are identified by all their values, so if a property with the same name
            // has different values, it's a different property. We need to find by name first, then check values.
            foreach (var recordProp in recordProperties)
            {
                if (recordProp == null) continue;

                // Find matching property in forward by name
                var forwardProp = forwardProperties.FirstOrDefault(fp =>
                    fp != null && fp.Name == recordProp.Name);

                if (forwardProp != null && !AreScriptPropertiesEqual(forwardProp, recordProp))
                {
                    // Property with same name exists but values differ - replace it
                    // Find the property in mergedScript by matching the forward property (by value)
                    var propToUpdate = mergedScript.Properties.FirstOrDefault(p =>
                        p != null && AreScriptPropertiesEqual(p, forwardProp));

                    if (propToUpdate != null)
                    {
                        // Remove old property and add new one
                        mergedScript.Properties.Remove(propToUpdate);
                        var newProp = CreateScriptPropertyCopy(recordProp);
                        if (newProp != null)
                        {
                            mergedScript.Properties.Add(newProp);
                            LogCollector.Add(PropertyName, $"[{PropertyName}] Updating property '{recordProp.Name}' in script '{recordScript.Name}'");
                        }
                    }
                }
            }

            return mergedScript;
        }

        private ScriptProperty? CreateScriptPropertyCopy(IScriptPropertyGetter source)
        {
            if (source == null) return null;

            // Create a new property based on type and copy all data
            switch (source)
            {
                case IScriptBoolPropertyGetter boolProp:
                    var newBoolProp = new ScriptBoolProperty();
                    newBoolProp.DeepCopyIn(boolProp);
                    return newBoolProp;

                case IScriptIntPropertyGetter intProp:
                    var newIntProp = new ScriptIntProperty();
                    newIntProp.DeepCopyIn(intProp);
                    return newIntProp;

                case IScriptFloatPropertyGetter floatProp:
                    var newFloatProp = new ScriptFloatProperty();
                    newFloatProp.DeepCopyIn(floatProp);
                    return newFloatProp;

                case IScriptStringPropertyGetter stringProp:
                    var newStringProp = new ScriptStringProperty();
                    newStringProp.DeepCopyIn(stringProp);
                    return newStringProp;

                case IScriptObjectPropertyGetter objProp:
                    var newObjProp = new ScriptObjectProperty();
                    newObjProp.DeepCopyIn(objProp);
                    return newObjProp;

                case IScriptBoolListPropertyGetter boolListProp:
                    var newBoolListProp = new ScriptBoolListProperty();
                    newBoolListProp.DeepCopyIn(boolListProp);
                    return newBoolListProp;

                case IScriptIntListPropertyGetter intListProp:
                    var newIntListProp = new ScriptIntListProperty();
                    newIntListProp.DeepCopyIn(intListProp);
                    return newIntListProp;

                case IScriptFloatListPropertyGetter floatListProp:
                    var newFloatListProp = new ScriptFloatListProperty();
                    newFloatListProp.DeepCopyIn(floatListProp);
                    return newFloatListProp;

                case IScriptStringListPropertyGetter stringListProp:
                    var newStringListProp = new ScriptStringListProperty();
                    newStringListProp.DeepCopyIn(stringListProp);
                    return newStringListProp;

                case IScriptObjectListPropertyGetter objListProp:
                    var newObjListProp = new ScriptObjectListProperty();
                    newObjListProp.DeepCopyIn(objListProp);
                    return newObjListProp;

                default:
                    LogCollector.Add(PropertyName, $"WARNING: Unknown script property type '{source.GetType().Name}' - cannot copy");
                    return null;
            }
        }

        private bool AreScriptPropertiesEqual(IScriptPropertyGetter prop1, IScriptPropertyGetter prop2)
        {
            // Compare property name
            if (prop1.Name != prop2.Name)
                return false;

            // Compare property flags
            if (prop1.Flags != prop2.Flags)
                return false;

            // Compare property data based on type - all values must match
            switch (prop1)
            {
                case IScriptBoolPropertyGetter bool1 when prop2 is IScriptBoolPropertyGetter bool2:
                    return bool1.Data == bool2.Data;

                case IScriptIntPropertyGetter int1 when prop2 is IScriptIntPropertyGetter int2:
                    return int1.Data == int2.Data;

                case IScriptFloatPropertyGetter float1 when prop2 is IScriptFloatPropertyGetter float2:
                    return float1.Data == float2.Data;

                case IScriptStringPropertyGetter string1 when prop2 is IScriptStringPropertyGetter string2:
                    return string1.Data == string2.Data;

                case IScriptObjectPropertyGetter obj1 when prop2 is IScriptObjectPropertyGetter obj2:
                    return obj1.Object.FormKey == obj2.Object.FormKey && obj1.Alias == obj2.Alias && obj1.Unused == obj2.Unused;

                case IScriptBoolListPropertyGetter boolList1 when prop2 is IScriptBoolListPropertyGetter boolList2:
                    return AreListsEqual(boolList1.Data, boolList2.Data);

                case IScriptIntListPropertyGetter intList1 when prop2 is IScriptIntListPropertyGetter intList2:
                    return AreListsEqual(intList1.Data, intList2.Data);

                case IScriptFloatListPropertyGetter floatList1 when prop2 is IScriptFloatListPropertyGetter floatList2:
                    return AreListsEqual(floatList1.Data, floatList2.Data);

                case IScriptStringListPropertyGetter stringList1 when prop2 is IScriptStringListPropertyGetter stringList2:
                    return AreListsEqual(stringList1.Data, stringList2.Data);

                case IScriptObjectListPropertyGetter objList1 when prop2 is IScriptObjectListPropertyGetter objList2:
                    return AreObjectListsEqual(objList1.Objects, objList2.Objects);

                default:
                    return false; // Different types or unsupported types
            }
        }

        private bool AreListsEqual<T>(IReadOnlyList<T>? list1, IReadOnlyList<T>? list2)
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;
            if (list1.Count != list2.Count) return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(list1[i], list2[i]))
                    return false;
            }
            return true;
        }

        private bool AreObjectListsEqual(IReadOnlyList<IScriptObjectPropertyGetter>? list1, IReadOnlyList<IScriptObjectPropertyGetter>? list2)
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;
            if (list1.Count != list2.Count) return false;

            for (int i = 0; i < list1.Count; i++)
            {
                var obj1 = list1[i];
                var obj2 = list2[i];
                if (obj1?.Object.FormKey != obj2?.Object.FormKey ||
                    obj1?.Alias != obj2?.Alias ||
                    obj1?.Unused != obj2?.Unused)
                    return false;
            }
            return true;
        }

        protected override string FormatItem(IScriptEntryGetter? item)
        {
            if (item == null) return "null";
            return $"Script({item.Name}, Flags: {item.Flags}, Properties: {item.Properties?.Count ?? 0})";
        }
    }
}
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class QuestScriptsHandler : AbstractListPropertyHandler<IScriptEntryGetter>
    {
        public override string PropertyName => "QuestScripts";

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

        protected override bool IsItemEqual(IScriptEntryGetter? item1, IScriptEntryGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Use our own value-based comparison
            return AreScriptEntriesEqual(item1, item2);
        }

        private bool AreScriptEntriesEqual(IScriptEntryGetter item1, IScriptEntryGetter item2)
        {
            // Compare script name
            if (item1.Name != item2.Name)
                return false;

            // Compare script flags
            if (item1.Flags != item2.Flags)
                return false;

            // Compare properties - handle null cases
            if (item1.Properties == null && item2.Properties == null) return true;
            if (item1.Properties == null || item2.Properties == null)
                return false;

            if (item1.Properties.Count != item2.Properties.Count)
                return false;

            // Compare properties - they may not be in the same order
            // For each property in item1, find a matching property in item2
            var item2Properties = item2.Properties.ToList();

            foreach (var prop1 in item1.Properties)
            {
                if (prop1 == null) continue;

                // Find a matching property in item2
                bool foundMatch = false;
                for (int i = 0; i < item2Properties.Count; i++)
                {
                    var prop2 = item2Properties[i];
                    if (prop2 == null) continue;

                    if (AreScriptPropertiesEqual(prop1, prop2))
                    {
                        // Found a match, remove it from the list to avoid duplicate matches
                        item2Properties.RemoveAt(i);
                        foundMatch = true;
                        break;
                    }
                }

                if (!foundMatch)
                    return false;
            }

            // All properties in item1 had matches, and item2Properties should be empty now
            return item2Properties.Count == 0;
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
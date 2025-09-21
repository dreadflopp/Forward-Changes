using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class QuestFragmentAliasHandler : AbstractListPropertyHandler<IQuestFragmentAliasGetter>
    {
        public override string PropertyName => "QuestFragmentAliases";

        public override List<IQuestFragmentAliasGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IQuestGetter questRecord && questRecord.VirtualMachineAdapter != null)
            {
                return questRecord.VirtualMachineAdapter.Aliases?.ToList();
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IQuestFragmentAliasGetter>? value)
        {
            if (record is IQuest questRecord && questRecord.VirtualMachineAdapter != null && value != null)
            {
                if (questRecord.VirtualMachineAdapter.Aliases != null)
                {
                    questRecord.VirtualMachineAdapter.Aliases.Clear();
                    foreach (var alias in value)
                    {
                        if (alias != null)
                        {
                            // Convert IQuestFragmentAliasGetter to QuestFragmentAlias
                            var newAlias = alias.DeepCopy();
                            questRecord.VirtualMachineAdapter.Aliases.Add(newAlias);
                        }
                    }
                }
            }
        }

        protected override bool IsItemEqual(IQuestFragmentAliasGetter? item1, IQuestFragmentAliasGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare the key properties of QuestFragmentAlias
            if (item1.Version != item2.Version) return false;
            if (item1.ObjectFormat != item2.ObjectFormat) return false;
            if (item1.Scripts.Count != item2.Scripts.Count) return false;

            // Sort scripts by name for consistent comparison
            var scripts1 = item1.Scripts.OrderBy(s => s.Name).ToList();
            var scripts2 = item2.Scripts.OrderBy(s => s.Name).ToList();

            // Compare scripts by content, not reference
            for (int i = 0; i < scripts1.Count; i++)
            {
                if (!AreScriptEntriesEqual(scripts1[i], scripts2[i])) return false;
            }

            // Compare Property using a simple hash-based approach instead of expensive reflection
            if (!ArePropertiesEqual(item1.Property, item2.Property)) return false;

            return true;
        }

        protected override string FormatItem(IQuestFragmentAliasGetter? item)
        {
            if (item == null) return "null";
            return $"FragmentAlias(Version={item.Version}, ObjectFormat={item.ObjectFormat}, Scripts={item.Scripts.Count})";
        }

        private bool AreScriptEntriesEqual(IScriptEntryGetter script1, IScriptEntryGetter script2)
        {
            // Compare script entries by content, not reference
            if (script1.Name != script2.Name) return false;
            if (script1.Flags != script2.Flags) return false;
            if (script1.Properties.Count != script2.Properties.Count) return false;

            // Sort properties by name for consistent comparison
            var props1 = script1.Properties.OrderBy(p => p.Name).ToList();
            var props2 = script2.Properties.OrderBy(p => p.Name).ToList();

            // Compare properties by content
            for (int i = 0; i < props1.Count; i++)
            {
                if (!AreScriptPropertiesEqual(props1[i], props2[i])) return false;
            }

            return true;
        }

        private bool AreScriptPropertiesEqual(IScriptPropertyGetter prop1, IScriptPropertyGetter prop2)
        {
            // Basic property comparison
            if (prop1.Name != prop2.Name) return false;
            if (prop1.Flags != prop2.Flags) return false;

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

        private bool ArePropertiesEqual(object prop1, object prop2)
        {
            if (prop1 == null && prop2 == null) return true;
            if (prop1 == null || prop2 == null) return false;
            if (prop1.GetType() != prop2.GetType()) return false;

            // Compare IScriptObjectPropertyGetter properties directly
            if (prop1 is IScriptObjectPropertyGetter scriptProp1 && prop2 is IScriptObjectPropertyGetter scriptProp2)
            {
                return scriptProp1.Name == scriptProp2.Name &&
                       scriptProp1.Flags == scriptProp2.Flags &&
                       scriptProp1.Object.FormKey == scriptProp2.Object.FormKey &&
                       scriptProp1.Alias == scriptProp2.Alias &&
                       scriptProp1.Unused == scriptProp2.Unused;
            }

            // Fallback to Equals() for other types
            return prop1.Equals(prop2);
        }

    }
}

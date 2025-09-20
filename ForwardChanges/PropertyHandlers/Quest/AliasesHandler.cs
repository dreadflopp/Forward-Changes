using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using System.Reflection;
using System.Linq;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class AliasesHandler : AbstractListPropertyHandler<IQuestAliasGetter>
    {
        public override string PropertyName => "Aliases";

        protected override ListOrdering Ordering => ListOrdering.PreserveModOrder;

        public override List<IQuestAliasGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IQuestGetter questRecord)
            {
                return questRecord.Aliases?.ToList();
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IQuestAliasGetter>? value)
        {
            if (record is IQuest questRecord && value != null)
            {
                if (questRecord.Aliases != null)
                {
                    questRecord.Aliases.Clear();
                    foreach (var alias in value)
                    {
                        if (alias is QuestAlias concreteAlias)
                        {
                            questRecord.Aliases.Add(concreteAlias);
                        }
                        else
                        {
                            // Convert IQuestAliasGetter to QuestAlias
                            var newAlias = alias.DeepCopy();
                            questRecord.Aliases.Add(newAlias);
                        }
                    }
                }
            }
        }

        public override bool AreValuesEqual(List<IQuestAliasGetter>? value1, List<IQuestAliasGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            for (int i = 0; i < value1.Count; i++)
            {
                if (!IsItemEqual(value1[i], value2[i])) return false;
            }
            return true;
        }

        protected override bool IsItemEqual(IQuestAliasGetter? item1, IQuestAliasGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare all quest alias properties for content-based equality
            return AreQuestAliasesEqual(item1, item2);
        }

        private bool AreQuestAliasesEqual(IQuestAliasGetter item1, IQuestAliasGetter item2)
        {
            // Compare basic properties
            if (item1.ID != item2.ID) return false;
            if (item1.Type != item2.Type) return false;
            if (item1.Name != item2.Name) return false;
            if (item1.Flags != item2.Flags) return false;
            if (item1.AliasIDToForceIntoWhenFilled != item2.AliasIDToForceIntoWhenFilled) return false;

            // Compare form links
            if (item1.SpecificLocation.FormKey != item2.SpecificLocation.FormKey) return false;
            if (item1.ForcedReference.FormKey != item2.ForcedReference.FormKey) return false;
            if (item1.UniqueActor.FormKey != item2.UniqueActor.FormKey) return false;
            if (item1.SpectatorOverridePackageList.FormKey != item2.SpectatorOverridePackageList.FormKey) return false;
            if (item1.ObserveDeadBodyOverridePackageList.FormKey != item2.ObserveDeadBodyOverridePackageList.FormKey) return false;
            if (item1.GuardWarnOverridePackageList.FormKey != item2.GuardWarnOverridePackageList.FormKey) return false;
            if (item1.CombatOverridePackageList.FormKey != item2.CombatOverridePackageList.FormKey) return false;
            if (item1.DisplayName.FormKey != item2.DisplayName.FormKey) return false;
            if (item1.VoiceTypes.FormKey != item2.VoiceTypes.FormKey) return false;

            // Compare complex objects using reflection for deep equality
            if (!AreComplexObjectsEqual(item1.Location, item2.Location)) return false;
            if (!AreComplexObjectsEqual(item1.External, item2.External)) return false;
            if (!AreComplexObjectsEqual(item1.CreateReferenceToObject, item2.CreateReferenceToObject)) return false;
            if (!AreComplexObjectsEqual(item1.FindMatchingRefNearAlias, item2.FindMatchingRefNearAlias)) return false;
            if (!AreComplexObjectsEqual(item1.FindMatchingRefFromEvent, item2.FindMatchingRefFromEvent)) return false;

            // Compare conditions
            if (!AreConditionsEqual(item1.Conditions, item2.Conditions)) return false;

            // Compare form link lists
            if (!AreFormLinkListsEqual(item1.Keywords, item2.Keywords)) return false;
            if (!AreFormLinkListsEqual(item1.Spells, item2.Spells)) return false;
            if (!AreFormLinkListsEqual(item1.Factions, item2.Factions)) return false;
            if (!AreFormLinkListsEqual(item1.PackageData, item2.PackageData)) return false;

            // Compare items (container entries)
            if (!AreContainerEntriesEqual(item1.Items, item2.Items)) return false;

            return true;
        }

        private bool AreComplexObjectsEqual(object? obj1, object? obj2)
        {
            // Handle null cases
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;

            // If they're the same reference, they're equal
            if (ReferenceEquals(obj1, obj2)) return true;

            // If they're different types, they're not equal
            if (obj1.GetType() != obj2.GetType()) return false;

            // Use reflection to compare all public properties
            var type = obj1.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                try
                {
                    var value1 = property.GetValue(obj1);
                    var value2 = property.GetValue(obj2);

                    if (!AreValuesEqual(value1, value2)) return false;
                }
                catch
                {
                    // If we can't access a property, assume they're different
                    return false;
                }
            }

            return true;
        }

        private bool AreValuesEqual(object? value1, object? value2)
        {
            // Handle null cases
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // If they're the same reference, they're equal
            if (ReferenceEquals(value1, value2)) return true;

            // Handle form links specially
            if (value1 is IFormLinkGetter formLink1 && value2 is IFormLinkGetter formLink2)
            {
                return formLink1.FormKey == formLink2.FormKey;
            }

            // Handle collections
            if (value1 is System.Collections.IEnumerable enumerable1 && value2 is System.Collections.IEnumerable enumerable2)
            {
                var list1 = enumerable1.Cast<object>().ToList();
                var list2 = enumerable2.Cast<object>().ToList();

                if (list1.Count != list2.Count) return false;

                for (int i = 0; i < list1.Count; i++)
                {
                    if (!AreValuesEqual(list1[i], list2[i])) return false;
                }
                return true;
            }

            // For other types, use Equals
            return value1.Equals(value2);
        }

        private bool AreConditionsEqual(IReadOnlyList<IConditionGetter> conditions1, IReadOnlyList<IConditionGetter> conditions2)
        {
            if (conditions1.Count != conditions2.Count) return false;

            for (int i = 0; i < conditions1.Count; i++)
            {
                if (!AreConditionsEqual(conditions1[i], conditions2[i])) return false;
            }
            return true;
        }

        private bool AreConditionsEqual(IConditionGetter condition1, IConditionGetter condition2)
        {
            // Basic condition comparison - this could be more comprehensive
            if (condition1.Flags != condition2.Flags) return false;
            if (condition1.Data?.GetType() != condition2.Data?.GetType()) return false;

            // For now, use a simple approach - in practice you might want more detailed comparison
            return condition1.Equals(condition2);
        }

        private bool AreFormLinkListsEqual<T>(IReadOnlyList<IFormLinkGetter<T>>? list1, IReadOnlyList<IFormLinkGetter<T>>? list2) where T : class, IMajorRecordGetter
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;
            if (list1.Count != list2.Count) return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i].FormKey != list2[i].FormKey) return false;
            }
            return true;
        }

        private bool AreContainerEntriesEqual(IReadOnlyList<IContainerEntryGetter>? items1, IReadOnlyList<IContainerEntryGetter>? items2)
        {
            if (items1 == null && items2 == null) return true;
            if (items1 == null || items2 == null) return false;
            if (items1.Count != items2.Count) return false;

            for (int i = 0; i < items1.Count; i++)
            {
                var item1 = items1[i];
                var item2 = items2[i];
                if (item1.Item.Item.FormKey != item2.Item.Item.FormKey || item1.Item.Count != item2.Item.Count) return false;
            }
            return true;
        }

        protected override string FormatItem(IQuestAliasGetter? item)
        {
            if (item == null) return "null";
            return $"QuestAlias(ID:{item.ID}, Type:{item.Type}, Name:{item.Name ?? "null"}, Flags:{item.Flags})";
        }
    }
}

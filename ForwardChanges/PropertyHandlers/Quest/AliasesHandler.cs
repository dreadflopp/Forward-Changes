using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;
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

            // Group aliases by ID for efficient matching
            var aliases1ById = value1
                .Where(a => a != null)
                .GroupBy(a => a.ID)
                .ToDictionary(g => g.Key, g => g.First());

            var aliases2ById = value2
                .Where(a => a != null)
                .GroupBy(a => a.ID)
                .ToDictionary(g => g.Key, g => g.First());

            // Check that all aliases in value1 have matching aliases in value2 with same properties
            foreach (var alias1 in aliases1ById.Values)
            {
                if (!aliases2ById.TryGetValue(alias1.ID, out var alias2))
                {
                    return false; // Alias ID not found in value2
                }

                // Compare aliases by all properties (for forwarding decision)
                if (!AreQuestAliasesEqual(alias1, alias2))
                {
                    return false; // Aliases differ
                }
            }

            return true;
        }

        protected override bool IsItemEqual(IQuestAliasGetter? item1, IQuestAliasGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Match aliases by ID only - properties are handled separately in ProcessHandlerSpecificLogic
            return item1.ID == item2.ID;
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

            // Compare complex objects using proper property-based comparison
            if (!AreLocationAliasReferencesEqual(item1.Location, item2.Location)) return false;
            if (!AreExternalAliasReferencesEqual(item1.External, item2.External)) return false;
            if (!AreCreateReferenceToObjectsEqual(item1.CreateReferenceToObject, item2.CreateReferenceToObject)) return false;
            if (!AreFindMatchingRefNearAliasesEqual(item1.FindMatchingRefNearAlias, item2.FindMatchingRefNearAlias)) return false;
            if (!AreFindMatchingRefFromEventsEqual(item1.FindMatchingRefFromEvent, item2.FindMatchingRefFromEvent)) return false;

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

        private bool AreCreateReferenceToObjectsEqual(ICreateReferenceToObjectGetter? obj1, ICreateReferenceToObjectGetter? obj2)
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;

            return obj1.Object.FormKey == obj2.Object.FormKey &&
                   obj1.AliasID == obj2.AliasID &&
                   obj1.Create == obj2.Create &&
                   obj1.Level == obj2.Level;
        }

        private bool AreLocationAliasReferencesEqual(ILocationAliasReferenceGetter? obj1, ILocationAliasReferenceGetter? obj2)
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;

            // TODO: Need interface to see properties
            return obj1.Equals(obj2);
        }

        private bool AreExternalAliasReferencesEqual(IExternalAliasReferenceGetter? obj1, IExternalAliasReferenceGetter? obj2)
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;

            // TODO: Need interface to see properties
            return obj1.Equals(obj2);
        }

        private bool AreFindMatchingRefNearAliasesEqual(IFindMatchingRefNearAliasGetter? obj1, IFindMatchingRefNearAliasGetter? obj2)
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;

            // TODO: Need interface to see properties
            return obj1.Equals(obj2);
        }

        private bool AreFindMatchingRefFromEventsEqual(IFindMatchingRefFromEventGetter? obj1, IFindMatchingRefFromEventGetter? obj2)
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;

            // TODO: Need interface to see properties
            return obj1.Equals(obj2);
        }

        private bool AreConditionsEqual(IReadOnlyList<IConditionGetter> conditions1, IReadOnlyList<IConditionGetter> conditions2)
        {
            if (conditions1.Count != conditions2.Count) return false;

            // Sort conditions by a consistent key for comparison - with ultra-safe FormKey handling
            var sorted1 = conditions1.OrderBy(c =>
            {
                try
                {
                    var formKey = c.Data.Reference.FormKey.IsNull ? FormKey.Null : c.Data.Reference.FormKey;
                    return $"{c.Data.Function}_{c.CompareOperator}_{formKey}";
                }
                catch
                {
                    return $"{c.Data.Function}_{c.CompareOperator}_{FormKey.Null}";
                }
            }).ToList();
            var sorted2 = conditions2.OrderBy(c =>
            {
                try
                {
                    var formKey = c.Data.Reference.FormKey.IsNull ? FormKey.Null : c.Data.Reference.FormKey;
                    return $"{c.Data.Function}_{c.CompareOperator}_{formKey}";
                }
                catch
                {
                    return $"{c.Data.Function}_{c.CompareOperator}_{FormKey.Null}";
                }
            }).ToList();

            for (int i = 0; i < sorted1.Count; i++)
            {
                if (!AreConditionsEqual(sorted1[i], sorted2[i])) return false;
            }
            return true;
        }

        private bool AreConditionsEqual(IConditionGetter condition1, IConditionGetter condition2)
        {
            // Compare all condition properties directly
            if (condition1.CompareOperator != condition2.CompareOperator) return false;
            if (condition1.Flags != condition2.Flags) return false;
            if (condition1.Unknown1.Span.SequenceEqual(condition2.Unknown1.Span) == false) return false;
            if (condition1.Unknown2 != condition2.Unknown2) return false;

            // Compare condition data properties
            var data1 = condition1.Data;
            var data2 = condition2.Data;

            if (data1.Function != data2.Function) return false;
            if (data1.RunOnType != data2.RunOnType) return false;
            if (data1.RunOnTypeIndex != data2.RunOnTypeIndex) return false;
            if (data1.UseAliases != data2.UseAliases) return false;
            if (data1.UsePackageData != data2.UsePackageData) return false;

            // Compare the Reference property
            if (data1.Reference.FormKey != data2.Reference.FormKey) return false;

            // For specific condition types that need special comparison logic
            if (data1 is IGetStageDoneConditionDataGetter stageData1 && data2 is IGetStageDoneConditionDataGetter stageData2)
            {
                // For GetStageDone conditions, compare quest and stage
                if (stageData1.Quest.Link.FormKey != stageData2.Quest.Link.FormKey) return false;
                if (stageData1.Stage != stageData2.Stage) return false;
            }
            else
            {
                // For all other condition types, use reflection to compare specific properties
                if (!CompareConditionDataProperties(data1, data2)) return false;
            }

            return true;
        }

        /// <summary>
        /// Compare condition data properties using reflection-based comparison like AbstractConditionsHandler
        /// </summary>
        private bool CompareConditionDataProperties(IConditionDataGetter data1, IConditionDataGetter data2)
        {
            // Use the same reflection-based approach as AbstractConditionsHandler
            return CompareConditionReferences(data1, data2);
        }

        /// <summary>
        /// Generic method to compare reference information between two condition data objects using reflection.
        /// This is copied from AbstractConditionsHandler to ensure consistency.
        /// </summary>
        private bool CompareConditionReferences(IConditionDataGetter data1, IConditionDataGetter data2)
        {
            try
            {
                // First, try the generic Reference property
                if (data1.Reference.FormKey != data2.Reference.FormKey)
                    return false;

                // If generic references are equal and not null, we're done
                if (!data1.Reference.FormKey.IsNull)
                    return true;

                // If generic references are both null, try to find and compare specific reference properties
                var (ref1, isNull1) = GetConditionReference(data1);
                var (ref2, isNull2) = GetConditionReference(data2);

                return ref1 == ref2 && isNull1 == isNull2;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generic method to extract reference information from any condition data type using reflection.
        /// This is copied from AbstractConditionsHandler to ensure consistency.
        /// </summary>
        private (string reference, bool isNull) GetConditionReference(IConditionDataGetter data)
        {
            try
            {
                // First, try the generic Reference property from the base ConditionData class
                var genericReference = data.Reference;
                if (genericReference != null && !genericReference.FormKey.IsNull)
                {
                    return (genericReference.FormKey.ToString(), false);
                }

                // If the generic Reference is null, try to find reference properties using reflection
                var dataType = data.GetType();
                var properties = dataType.GetProperties();

                // Look for properties that might contain reference information
                // Common patterns: Keyword, Race, Quest, Location, etc.
                foreach (var prop in properties)
                {
                    var propName = prop.Name.ToLowerInvariant();

                    // Skip base class properties and common non-reference properties
                    if (propName == "reference" || propName == "runontype" || propName == "runontypeindex" ||
                        propName == "usealiases" || propName == "usepackagedata" || propName == "function" ||
                        propName.Contains("unused") || propName.Contains("string") || propName.Contains("int"))
                        continue;

                    try
                    {
                        var propValue = prop.GetValue(data);
                        if (propValue == null) continue;

                        // Try to extract FormKey from the property value
                        var formKey = ExtractFormKeyFromProperty(propValue);
                        if (formKey.HasValue && !formKey.Value.IsNull)
                        {
                            return (formKey.Value.ToString(), false);
                        }
                    }
                    catch
                    {
                        // Continue to next property if this one fails
                        continue;
                    }
                }

                // If no reference found, return null
                return ("Null", true);
            }
            catch
            {
                return ("Null", true);
            }
        }

        /// <summary>
        /// Extracts FormKey from a property value, handling various FormLink types.
        /// This is copied from AbstractConditionsHandler to ensure consistency.
        /// </summary>
        private FormKey? ExtractFormKeyFromProperty(object propValue)
        {
            try
            {
                // Handle IFormLinkOrIndex types (like Keyword, Race, etc.)
                if (propValue.GetType().GetInterface("IFormLinkOrIndexGetter`1") != null)
                {
                    // Try to get the Link property and then FormKey
                    var linkProperty = propValue.GetType().GetProperty("Link");
                    if (linkProperty != null)
                    {
                        var link = linkProperty.GetValue(propValue);
                        if (link != null)
                        {
                            var formKeyProperty = link.GetType().GetProperty("FormKey");
                            if (formKeyProperty != null)
                            {
                                var formKeyValue = formKeyProperty.GetValue(link);
                                if (formKeyValue is FormKey formKey)
                                {
                                    return formKey;
                                }
                            }
                        }
                    }
                }

                // Handle direct FormLink types
                var directFormKeyProperty = propValue.GetType().GetProperty("FormKey");
                if (directFormKeyProperty != null)
                {
                    var formKeyValue = directFormKeyProperty.GetValue(propValue);
                    if (formKeyValue is FormKey formKey)
                    {
                        return formKey;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private bool AreFormLinkListsEqual<T>(IReadOnlyList<IFormLinkGetter<T>>? list1, IReadOnlyList<IFormLinkGetter<T>>? list2) where T : class, IMajorRecordGetter
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;
            if (list1.Count != list2.Count) return false;

            // Sort form links by string representation for consistent comparison
            var sorted1 = list1.OrderBy(f => f.FormKey.IsNull ? "NULL" : f.FormKey.ToString()).ToList();
            var sorted2 = list2.OrderBy(f => f.FormKey.IsNull ? "NULL" : f.FormKey.ToString()).ToList();

            for (int i = 0; i < sorted1.Count; i++)
            {
                if (sorted1[i].FormKey != sorted2[i].FormKey) return false;
            }
            return true;
        }

        private bool AreContainerEntriesEqual(IReadOnlyList<IContainerEntryGetter>? items1, IReadOnlyList<IContainerEntryGetter>? items2)
        {
            if (items1 == null && items2 == null) return true;
            if (items1 == null || items2 == null) return false;
            if (items1.Count != items2.Count) return false;

            // Sort container entries by FormKey and Count for consistent comparison
            var sorted1 = items1.OrderBy(i => $"{i.Item.Item.FormKey}_{i.Item.Count}").ToList();
            var sorted2 = items2.OrderBy(i => $"{i.Item.Item.FormKey}_{i.Item.Count}").ToList();

            for (int i = 0; i < sorted1.Count; i++)
            {
                var item1 = sorted1[i];
                var item2 = sorted2[i];
                if (item1.Item.Item.FormKey != item2.Item.Item.FormKey || item1.Item.Count != item2.Item.Count) return false;
            }
            return true;
        }

        protected override string FormatItem(IQuestAliasGetter? item)
        {
            if (item == null) return "null";
            return $"QuestAlias(ID:{item.ID}, Type:{item.Type}, Name:{item.Name ?? "null"}, Flags:{item.Flags})";
        }

        protected override void ProcessHandlerSpecificLogic(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            ListPropertyContext<IQuestAliasGetter> listPropertyContext,
            List<IQuestAliasGetter> recordItems,
            List<ListPropertyValueContext<IQuestAliasGetter>> currentForwardItems)
        {
            var recordMod = state.LoadOrder[context.ModKey].Mod;
            if (recordMod == null) return;

            // Group forward aliases by ID for efficient matching
            var forwardAliasesById = currentForwardItems
                .Where(i => !i.IsRemoved)
                .GroupBy(i => i.Value.ID)
                .ToDictionary(g => g.Key, g => g.First());

            // Process each alias in the record
            foreach (var recordAlias in recordItems)
            {
                if (recordAlias == null) continue;

                // Find matching forward alias by ID
                if (!forwardAliasesById.TryGetValue(recordAlias.ID, out var forwardContext))
                {
                    // Alias doesn't exist in forward - will be handled by ProcessAdditions
                    continue;
                }

                // Check if we have permission to modify this alias
                if (!HasPermissionsToModify(recordMod, forwardContext.OwnerMod))
                {
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Cannot modify alias ID {recordAlias.ID} - no permission (owned by {forwardContext.OwnerMod})");
                    continue;
                }

                // Check if properties differ (using full comparison)
                if (!AreQuestAliasesEqual(recordAlias, forwardContext.Value))
                {
                    // Properties differ - update all properties from record alias to forward alias
                    // Create a new alias with all properties from record alias
                    var updatedAlias = recordAlias.DeepCopy();

                    // Update the forward context with the new alias and take ownership
                    forwardContext.Value = updatedAlias;
                    forwardContext.OwnerMod = context.ModKey.ToString();

                    LogCollector.Add(PropertyName, $"[{PropertyName}] Updating alias ID {recordAlias.ID} (taking ownership as '{context.ModKey}')");
                }
            }
        }
    }
}

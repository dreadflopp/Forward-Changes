using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Synthesis;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.Contexts;
using System.Linq;

namespace DreadsMashedPatch.PropertyHandlers.Quest
{
    public class AliasesHandler : AbstractListPropertyHandler<IQuestAliasGetter>
    {
        public override string PropertyName => "Aliases";

        public override ListSemantics Semantics => ListSemantics.AlignedOrdered;

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

            return obj1.AliasID == obj2.AliasID &&
                   obj1.Keyword.FormKeyNullable == obj2.Keyword.FormKeyNullable &&
                   obj1.RefType.FormKeyNullable == obj2.RefType.FormKeyNullable;
        }

        private bool AreExternalAliasReferencesEqual(IExternalAliasReferenceGetter? obj1, IExternalAliasReferenceGetter? obj2)
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;

            return obj1.Quest.FormKeyNullable == obj2.Quest.FormKeyNullable &&
                   obj1.AliasID == obj2.AliasID;
        }

        private bool AreFindMatchingRefNearAliasesEqual(IFindMatchingRefNearAliasGetter? obj1, IFindMatchingRefNearAliasGetter? obj2)
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;

            return obj1.AliasID == obj2.AliasID &&
                   obj1.Type == obj2.Type;
        }

        private bool AreFindMatchingRefFromEventsEqual(IFindMatchingRefFromEventGetter? obj1, IFindMatchingRefFromEventGetter? obj2)
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;

            return obj1.FromEvent == obj2.FromEvent &&
                   obj1.EventData == obj2.EventData;
        }

        private bool AreConditionsEqual(IReadOnlyList<IConditionGetter> conditions1, IReadOnlyList<IConditionGetter> conditions2)
        {
            if (conditions1.Count != conditions2.Count) return false;

            // xEdit represents CTDA as an ordered array. Reordering can change OR-group semantics.
            for (int i = 0; i < conditions1.Count; i++)
            {
                if (!AreConditionsEqual(conditions1[i], conditions2[i])) return false;
            }
            return true;
        }

        private bool AreConditionsEqual(IConditionGetter condition1, IConditionGetter condition2)
        {
            return ConditionMixIn.Equals(condition1, condition2);
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

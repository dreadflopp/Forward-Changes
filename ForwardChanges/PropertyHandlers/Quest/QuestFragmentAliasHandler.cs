using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
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
            if (!item1.Property.Equals(item2.Property)) return false;
            if (item1.Scripts.Count != item2.Scripts.Count) return false;

            // Compare scripts by content, not reference
            for (int i = 0; i < item1.Scripts.Count; i++)
            {
                if (!AreScriptEntriesEqual(item1.Scripts[i], item2.Scripts[i])) return false;
            }

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

            // Compare properties by content
            for (int i = 0; i < script1.Properties.Count; i++)
            {
                if (!AreScriptPropertiesEqual(script1.Properties[i], script2.Properties[i])) return false;
            }

            return true;
        }

        private bool AreScriptPropertiesEqual(IScriptPropertyGetter prop1, IScriptPropertyGetter prop2)
        {
            // Basic property comparison
            if (prop1.Name != prop2.Name) return false;
            if (prop1.Flags != prop2.Flags) return false;

            // For now, use a simple approach - in practice you might want more detailed comparison
            // This could be enhanced based on the specific property types
            return prop1.Equals(prop2);
        }
    }
}

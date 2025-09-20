using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class QuestScriptFragmentHandler : AbstractListPropertyHandler<IQuestScriptFragmentGetter>
    {
        public override string PropertyName => "QuestScriptFragments";

        public override List<IQuestScriptFragmentGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IQuestGetter questRecord && questRecord.VirtualMachineAdapter != null)
            {
                return questRecord.VirtualMachineAdapter.Fragments?.ToList();
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IQuestScriptFragmentGetter>? value)
        {
            if (record is IQuest questRecord && questRecord.VirtualMachineAdapter != null && value != null)
            {
                if (questRecord.VirtualMachineAdapter.Fragments != null)
                {
                    questRecord.VirtualMachineAdapter.Fragments.Clear();
                    foreach (var fragment in value)
                    {
                        if (fragment != null)
                        {
                            // Convert IQuestScriptFragmentGetter to QuestScriptFragment
                            var newFragment = fragment.DeepCopy();
                            questRecord.VirtualMachineAdapter.Fragments.Add(newFragment);
                        }
                    }
                }
            }
        }

        protected override bool IsItemEqual(IQuestScriptFragmentGetter? item1, IQuestScriptFragmentGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            return item1.Stage == item2.Stage &&
                   item1.Unknown == item2.Unknown &&
                   item1.StageIndex == item2.StageIndex &&
                   item1.Unknown2 == item2.Unknown2 &&
                   item1.ScriptName == item2.ScriptName &&
                   item1.FragmentName == item2.FragmentName;
        }

        protected override string FormatItem(IQuestScriptFragmentGetter? item)
        {
            if (item == null) return "null";
            return $"Fragment(Stage={item.Stage}, Script={item.ScriptName}, Fragment={item.FragmentName})";
        }
    }
}

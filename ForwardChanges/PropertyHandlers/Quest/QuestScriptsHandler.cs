using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyHandlers.Quest;

/// <summary>
/// Handles the script list on a quest VMAD. Name-only identity, complete-value
/// comparison, and atomic forwarding are shared with all other VMAD handlers.
/// </summary>
public class QuestScriptsHandler : AbstractScriptListPropertyHandler
{
    public override string PropertyName => "VirtualMachineAdapter.Scripts";

    public override List<IScriptEntryGetter>? GetValue(IMajorRecordGetter record)
    {
        return record is IQuestGetter quest
            ? quest.VirtualMachineAdapter?.Scripts.ToList()
            : null;
    }

    public override void SetValue(IMajorRecord record, List<IScriptEntryGetter>? value)
    {
        if (record is not IQuest quest || value == null) return;

        quest.VirtualMachineAdapter ??= new QuestAdapter();
        var destinationScripts = quest.VirtualMachineAdapter.Scripts.ToList();
        quest.VirtualMachineAdapter.Scripts.Clear();
        foreach (var script in value)
        {
            var destinationScript = destinationScripts.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, script.Name, StringComparison.Ordinal));
            var copy = PapyrusUnusedDataPolicy.CopyScript(script, destinationScript);
            quest.VirtualMachineAdapter.Scripts.Add(copy);
        }
    }
}

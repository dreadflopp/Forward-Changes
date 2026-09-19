using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.Quest;

/// <summary>
/// Tracks whether a quest VMAD exists independently from its mergeable contents.
/// A missing VMAD is a parent-level state, not a collection of zero/null leaf values.
/// </summary>
public sealed class QuestVirtualMachineAdapterPresenceHandler : AbstractPropertyHandler<bool>
{
    public override string PropertyName => "VirtualMachineAdapter.Presence";

    public override bool GetValue(IMajorRecordGetter record)
    {
        return record is IQuestGetter quest && quest.VirtualMachineAdapter != null;
    }

    public override void SetValue(IMajorRecord record, bool value)
    {
        if (record is not IQuest quest)
        {
            throw new InvalidOperationException(
                $"Expected {nameof(IQuest)} but got {record.GetType().Name} for {PropertyName}");
        }

        if (!value)
        {
            quest.VirtualMachineAdapter = null;
            return;
        }

        quest.VirtualMachineAdapter ??= new QuestAdapter();
    }

    public override string FormatValue(object? value)
    {
        return value is true ? "Present" : "Absent";
    }
}

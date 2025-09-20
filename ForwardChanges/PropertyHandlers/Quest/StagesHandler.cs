using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class StagesHandler : AbstractListPropertyHandler<IQuestStageGetter>
    {
        public override string PropertyName => "Stages";

        public override List<IQuestStageGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IQuestGetter questRecord)
            {
                return questRecord.Stages?.ToList();
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IQuestStageGetter>? value)
        {
            if (record is IQuest questRecord && value != null)
            {
                if (questRecord.Stages != null)
                {
                    questRecord.Stages.Clear();
                    foreach (var stage in value)
                    {
                        if (stage is QuestStage concreteStage)
                        {
                            questRecord.Stages.Add(concreteStage);
                        }
                        else
                        {
                            // Convert IQuestStageGetter to QuestStage
                            var newStage = stage.DeepCopy();
                            questRecord.Stages.Add(newStage);
                        }
                    }
                }
            }
        }

        public override bool AreValuesEqual(List<IQuestStageGetter>? value1, List<IQuestStageGetter>? value2)
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

        protected override bool IsItemEqual(IQuestStageGetter? item1, IQuestStageGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare all quest stage properties for content-based equality
            if (item1.Index != item2.Index) return false;
            if (item1.Flags != item2.Flags) return false;
            if (item1.Unknown != item2.Unknown) return false;

            // Compare log entries by content
            if (!AreLogEntriesEqual(item1.LogEntries, item2.LogEntries)) return false;

            return true;
        }

        protected override string FormatItem(IQuestStageGetter? item)
        {
            return item != null ? $"Index:{item.Index}, Flags:{item.Flags}, LogEntries:{item.LogEntries.Count}" : "null";
        }

        private bool AreLogEntriesEqual(IReadOnlyList<IQuestLogEntryGetter> logEntries1, IReadOnlyList<IQuestLogEntryGetter> logEntries2)
        {
            if (logEntries1.Count != logEntries2.Count) return false;

            for (int i = 0; i < logEntries1.Count; i++)
            {
                if (!AreLogEntriesEqual(logEntries1[i], logEntries2[i])) return false;
            }
            return true;
        }

        private bool AreLogEntriesEqual(IQuestLogEntryGetter logEntry1, IQuestLogEntryGetter logEntry2)
        {
            // For now, use a simple approach - in practice you might want more detailed comparison
            // This could be enhanced based on the specific properties of IQuestLogEntryGetter
            return logEntry1.Equals(logEntry2);
        }
    }
}

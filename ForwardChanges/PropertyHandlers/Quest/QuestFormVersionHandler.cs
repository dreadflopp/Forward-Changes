using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class QuestFormVersionHandler : AbstractPropertyHandler<byte>
    {
        public override string PropertyName => "QuestFormVersion";

        public override void SetValue(IMajorRecord record, byte value)
        {
            if (record is IQuest questRecord)
            {
                questRecord.QuestFormVersion = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IQuest for {PropertyName}");
            }
        }

        public override byte GetValue(IMajorRecordGetter record)
        {
            if (record is IQuestGetter questRecord)
            {
                return questRecord.QuestFormVersion;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IQuestGetter for {PropertyName}");
            }
            return 0;
        }
    }
}

using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class UnknownHandler : AbstractPropertyHandler<int>
    {
        public override string PropertyName => "Unknown";

        public override void SetValue(IMajorRecord record, int value)
        {
            if (record is IQuest questRecord)
            {
                questRecord.Unknown = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IQuest for {PropertyName}");
            }
        }

        public override int GetValue(IMajorRecordGetter record)
        {
            if (record is IQuestGetter questRecord)
            {
                return questRecord.Unknown;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IQuestGetter for {PropertyName}");
            }
            return 0;
        }
    }
}

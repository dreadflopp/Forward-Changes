using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class NextAliasIDHandler : AbstractPropertyHandler<uint?>
    {
        public override string PropertyName => "NextAliasID";

        public override void SetValue(IMajorRecord record, uint? value)
        {
            if (record is IQuest questRecord)
            {
                questRecord.NextAliasID = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IQuest for {PropertyName}");
            }
        }

        public override uint? GetValue(IMajorRecordGetter record)
        {
            if (record is IQuestGetter questRecord)
            {
                return questRecord.NextAliasID;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IQuestGetter for {PropertyName}");
            }
            return null;
        }
    }
}

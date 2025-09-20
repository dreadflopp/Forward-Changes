using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class TypeHandler : AbstractPropertyHandler<Mutagen.Bethesda.Skyrim.Quest.TypeEnum>
    {
        public override string PropertyName => "Type";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Quest.TypeEnum value)
        {
            if (record is IQuest questRecord)
            {
                questRecord.Type = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IQuest for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.Quest.TypeEnum GetValue(IMajorRecordGetter record)
        {
            if (record is IQuestGetter questRecord)
            {
                return questRecord.Type;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IQuestGetter for {PropertyName}");
            }
            return Mutagen.Bethesda.Skyrim.Quest.TypeEnum.None;
        }
    }
}

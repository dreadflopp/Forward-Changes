using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class DescriptionHandler : AbstractPropertyHandler<string>
    {
        public override string PropertyName => "Description";

        public override void SetValue(IMajorRecord record, string? value)
        {
            if (record is IQuest questRecord)
            {
                if (value != null)
                {
                    var translatedString = new TranslatedString(Language.English);
                    translatedString.String = value;
                    questRecord.Description = translatedString;
                }
                else
                {
                    questRecord.Description = null;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IQuest for {PropertyName}");
            }
        }

        public override string? GetValue(IMajorRecordGetter record)
        {
            if (record is IQuestGetter questRecord)
            {
                return questRecord.Description?.String;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IQuestGetter for {PropertyName}");
            }
            return null;
        }
    }
}

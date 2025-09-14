using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class PromptHandler : AbstractPropertyHandler<ITranslatedStringGetter?>
    {
        public override string PropertyName => "Prompt";

        public override void SetValue(IMajorRecord record, ITranslatedStringGetter? value)
        {
            if (record is IDialogResponses dialogResponseRecord)
            {
                if (value == null)
                {
                    dialogResponseRecord.Prompt = null;
                }
                else
                {
                    // Deep copy the translated string
                    var newPrompt = new TranslatedString(Language.English);
                    newPrompt.String = value.String;
                    dialogResponseRecord.Prompt = newPrompt;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogResponses for {PropertyName}");
            }
        }

        public override ITranslatedStringGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogResponsesGetter dialogResponseRecord)
            {
                return dialogResponseRecord.Prompt;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogResponsesGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(ITranslatedStringGetter? value1, ITranslatedStringGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.String == value2.String;
        }

        public override string FormatValue(object? value)
        {
            if (value is not ITranslatedStringGetter translatedString)
            {
                return value?.ToString() ?? "null";
            }

            var text = translatedString.String ?? "null";
            return text.Length > 50 ? $"{text.Substring(0, 50)}..." : text;
        }
    }
}
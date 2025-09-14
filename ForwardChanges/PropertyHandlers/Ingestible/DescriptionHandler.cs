using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingestible
{
    public class DescriptionHandler : AbstractPropertyHandler<ITranslatedStringGetter?>
    {
        public override string PropertyName => "Description";

        public override ITranslatedStringGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IIngestibleGetter ingestibleRecord)
            {
                return ingestibleRecord.Description;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngestibleGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, ITranslatedStringGetter? value)
        {
            if (record is IIngestible ingestibleRecord)
            {
                if (value != null)
                {
                    // Create a deep copy of the translated string
                    var newDescription = new TranslatedString(Language.English);
                    newDescription.String = value.String;
                    ingestibleRecord.Description = newDescription;
                }
                else
                {
                    ingestibleRecord.Description = null;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngestible for {PropertyName}");
            }
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
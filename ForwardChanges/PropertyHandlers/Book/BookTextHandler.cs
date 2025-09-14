using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Book
{
    public class BookTextHandler : AbstractPropertyHandler<ITranslatedStringGetter>
    {
        public override string PropertyName => "BookText";

        public override void SetValue(IMajorRecord record, ITranslatedStringGetter? value)
        {
            if (record is IBook bookRecord)
            {
                if (value == null)
                {
                    bookRecord.BookText = new TranslatedString(Language.English);
                }
                else
                {
                    // Create a deep copy of the translated string
                    var newBookText = new TranslatedString(Language.English);
                    newBookText.String = value.String;
                    bookRecord.BookText = newBookText;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IBook for {PropertyName}");
            }
        }

        public override ITranslatedStringGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IBookGetter bookRecord)
            {
                return bookRecord.BookText;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IBookGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(ITranslatedStringGetter? value1, ITranslatedStringGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare the string values
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
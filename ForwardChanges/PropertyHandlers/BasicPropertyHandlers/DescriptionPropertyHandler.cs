using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class BookDescriptionPropertyHandler : AbstractPropertyHandler<ITranslatedStringGetter>
    {
        public override string PropertyName => "Description";

        public override void SetValue(IMajorRecord record, ITranslatedStringGetter? value)
        {
            if (record is IBook bookRecord)
            {
                if (value == null)
                {
                    bookRecord.Description = null;
                }
                else
                {
                    // Create a deep copy of the translated string
                    var newDescription = new TranslatedString(Language.English);
                    newDescription.String = value.String;
                    bookRecord.Description = newDescription;
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
                return bookRecord.Description;
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
    }
}
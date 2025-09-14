using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Book
{
    public class TypeHandler : AbstractPropertyHandler<Mutagen.Bethesda.Skyrim.Book.BookType?>
    {
        public override string PropertyName => "Type";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Book.BookType? value)
        {
            if (record is IBook bookRecord)
            {
                bookRecord.Type = value ?? default(Mutagen.Bethesda.Skyrim.Book.BookType);
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IBook for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.Book.BookType? GetValue(IMajorRecordGetter record)
        {
            if (record is IBookGetter bookRecord)
            {
                return bookRecord.Type;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IBookGetter for {PropertyName}");
            }
            return null;
        }
    }
}
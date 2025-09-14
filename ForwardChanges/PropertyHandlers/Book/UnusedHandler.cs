using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Book
{
    public class UnusedHandler : AbstractPropertyHandler<ushort>
    {
        public override string PropertyName => "Unused";

        public override void SetValue(IMajorRecord record, ushort value)
        {
            if (record is IBook bookRecord)
            {
                bookRecord.Unused = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IBook for {PropertyName}");
            }
        }

        public override ushort GetValue(IMajorRecordGetter record)
        {
            if (record is IBookGetter bookRecord)
            {
                return bookRecord.Unused;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IBookGetter for {PropertyName}");
            }
            return 0;
        }
    }
}
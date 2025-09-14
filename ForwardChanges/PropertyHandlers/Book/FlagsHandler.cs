using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Book
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Book.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Book.Flag value)
        {
            if (record is IBook bookRecord)
            {
                bookRecord.Flags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IBook for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.Book.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IBookGetter bookRecord)
            {
                return bookRecord.Flags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IBookGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.Book.Flag);
        }

        protected override Mutagen.Bethesda.Skyrim.Book.Flag[] GetAllFlags()
        {
            // Return all enum values - this will be determined at runtime
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Book.Flag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Book.Flag flags, Mutagen.Bethesda.Skyrim.Book.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.Book.Flag SetFlag(Mutagen.Bethesda.Skyrim.Book.Flag flags, Mutagen.Bethesda.Skyrim.Book.Flag flag, bool value)
        {
            if (value)
            {
                return flags | flag;
            }
            else
            {
                return flags & ~flag;
            }
        }

        protected override string FormatFlag(Mutagen.Bethesda.Skyrim.Book.Flag flag)
        {
            return flag.ToString();
        }
    }
}
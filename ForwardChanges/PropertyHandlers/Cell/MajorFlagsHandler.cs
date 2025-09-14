using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class MajorFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Cell.MajorFlag>
    {
        public override string PropertyName => "MajorFlags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Cell.MajorFlag value)
        {
            if (record is ICell cell)
            {
                cell.MajorFlags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ICell for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.Cell.MajorFlag GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cell)
            {
                return cell.MajorFlags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ICellGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.Cell.MajorFlag);
        }

        protected override Mutagen.Bethesda.Skyrim.Cell.MajorFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Cell.MajorFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Cell.MajorFlag flags, Mutagen.Bethesda.Skyrim.Cell.MajorFlag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.Cell.MajorFlag SetFlag(Mutagen.Bethesda.Skyrim.Cell.MajorFlag flags, Mutagen.Bethesda.Skyrim.Cell.MajorFlag flag, bool value)
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
    }
}

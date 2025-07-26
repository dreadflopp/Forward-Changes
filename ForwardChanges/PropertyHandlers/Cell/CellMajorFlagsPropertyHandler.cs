using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class CellMajorFlagsPropertyHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Cell.MajorFlag>
    {
        public override string PropertyName => "MajorFlags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Cell.MajorFlag value)
        {
            if (record is ICell cell)
            {
                cell.MajorFlags = value;
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
                return new Mutagen.Bethesda.Skyrim.Cell.MajorFlag();
            }
        }

        protected override Mutagen.Bethesda.Skyrim.Cell.MajorFlag[] GetAllFlags()
        {
            return new Mutagen.Bethesda.Skyrim.Cell.MajorFlag[]
            {
                Mutagen.Bethesda.Skyrim.Cell.MajorFlag.Persistent,
                Mutagen.Bethesda.Skyrim.Cell.MajorFlag.OffLimits,
                Mutagen.Bethesda.Skyrim.Cell.MajorFlag.CantWait
            };
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Cell.MajorFlag flags, Mutagen.Bethesda.Skyrim.Cell.MajorFlag flag)
        {
            return flags.HasFlag(flag);
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
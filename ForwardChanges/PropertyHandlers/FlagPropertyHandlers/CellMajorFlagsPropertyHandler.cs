using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.FlagPropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.FlagPropertyHandlers
{
    public class CellMajorFlagsPropertyHandler : AbstractFlagPropertyHandler<Cell.MajorFlag>
    {
        public override string PropertyName => "MajorFlags";

        public override void SetValue(IMajorRecord record, Cell.MajorFlag value)
        {
            if (record is ICell cell)
            {
                cell.MajorFlags = value;
            }
        }

        public override Cell.MajorFlag GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cell)
            {
                return cell.MajorFlags;
            }
            else
            {
                return new Cell.MajorFlag();
            }
        }

        protected override Cell.MajorFlag[] GetAllFlags()
        {
            return new Cell.MajorFlag[]
            {
                Cell.MajorFlag.Persistent,
                Cell.MajorFlag.OffLimits,
                Cell.MajorFlag.CantWait
            };
        }

        protected override bool IsFlagSet(Cell.MajorFlag flags, Cell.MajorFlag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Cell.MajorFlag SetFlag(Cell.MajorFlag flags, Cell.MajorFlag flag, bool value)
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
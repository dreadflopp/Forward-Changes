using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.FlagPropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.FlagPropertyHandlers
{
    public class CellFlagsPropertyHandler : AbstractFlagPropertyHandler<Cell.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, Cell.Flag value)
        {
            if (record is ICell cell)
            {
                cell.Flags = value;
            }
        }

        public override Cell.Flag GetValue(IMajorRecordGetter record)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Cell.Flag result;
            if (record is ICellGetter cell)
            {
                result = cell.Flags;
            }
            else
            {
                result = new Cell.Flag();
            }
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > 10) // Log if it takes more than 10ms
            {
                Console.WriteLine($"[CellFlagsPropertyHandler] GetValue took {stopwatch.ElapsedMilliseconds}ms for {record.FormKey}");
            }

            return result;
        }

        protected override Cell.Flag[] GetAllFlags()
        {
            return new Cell.Flag[]
            {
                Cell.Flag.IsInteriorCell,
                Cell.Flag.HasWater,
                Cell.Flag.CantTravelFromHere,
                Cell.Flag.NoLodWater,
                Cell.Flag.PublicArea,
                Cell.Flag.HandChanged,
                Cell.Flag.ShowSky,
                Cell.Flag.UseSkyLighting
            };
        }

        protected override bool IsFlagSet(Cell.Flag flags, Cell.Flag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Cell.Flag SetFlag(Cell.Flag flags, Cell.Flag flag, bool value)
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
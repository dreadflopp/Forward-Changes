using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Cell.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Cell.Flag value)
        {
            if (record is ICell cell)
            {
                cell.Flags = value;
            }
        }

        public override Mutagen.Bethesda.Skyrim.Cell.Flag GetValue(IMajorRecordGetter record)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Mutagen.Bethesda.Skyrim.Cell.Flag result;
            if (record is ICellGetter cell)
            {
                result = cell.Flags;
            }
            else
            {
                result = new Mutagen.Bethesda.Skyrim.Cell.Flag();
            }
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > 10) // Log if it takes more than 10ms
            {
                Console.WriteLine($"[CellFlagsPropertyHandler] GetValue took {stopwatch.ElapsedMilliseconds}ms for {record.FormKey}");
            }

            return result;
        }

        protected override Mutagen.Bethesda.Skyrim.Cell.Flag[] GetAllFlags()
        {
            return new Mutagen.Bethesda.Skyrim.Cell.Flag[]
            {
                Mutagen.Bethesda.Skyrim.Cell.Flag.IsInteriorCell,
                Mutagen.Bethesda.Skyrim.Cell.Flag.HasWater,
                Mutagen.Bethesda.Skyrim.Cell.Flag.CantTravelFromHere,
                Mutagen.Bethesda.Skyrim.Cell.Flag.NoLodWater,
                Mutagen.Bethesda.Skyrim.Cell.Flag.PublicArea,
                Mutagen.Bethesda.Skyrim.Cell.Flag.HandChanged,
                Mutagen.Bethesda.Skyrim.Cell.Flag.ShowSky,
                Mutagen.Bethesda.Skyrim.Cell.Flag.UseSkyLighting
            };
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Cell.Flag flags, Mutagen.Bethesda.Skyrim.Cell.Flag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Mutagen.Bethesda.Skyrim.Cell.Flag SetFlag(Mutagen.Bethesda.Skyrim.Cell.Flag flags, Mutagen.Bethesda.Skyrim.Cell.Flag flag, bool value)
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


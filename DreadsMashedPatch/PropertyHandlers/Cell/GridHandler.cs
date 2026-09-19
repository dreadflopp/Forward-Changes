using System;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class GridHandler : AbstractPropertyHandler<ICellGridGetter?>
    {
        public override string PropertyName => "Grid";

        public override void SetValue(IMajorRecord record, ICellGridGetter? value)
        {
            if (record is ICell cellRecord)
            {
                if (value == null)
                {
                    cellRecord.Grid = null;
                    return;
                }

                // Create a new CellGrid instance and copy properties
                var newGrid = new CellGrid();

                // Copy Point
                newGrid.Point = value.Point;

                // Copy Flags
                newGrid.Flags = value.Flags;

                cellRecord.Grid = newGrid;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ICell for {PropertyName}");
            }
        }

        public override ICellGridGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cellRecord)
            {
                return cellRecord.Grid;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ICellGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(ICellGridGetter? value1, ICellGridGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare Point
            if (value1.Point != value2.Point) return false;

            // Compare Flags
            if (value1.Flags != value2.Flags) return false;

            return true;
        }

        public override string FormatValue(object? value)
        {
            if (value is not ICellGridGetter grid)
            {
                return value?.ToString() ?? "null";
            }

            return $"Point: ({grid.Point.X}, {grid.Point.Y}), Flags: {grid.Flags}";
        }
    }
}
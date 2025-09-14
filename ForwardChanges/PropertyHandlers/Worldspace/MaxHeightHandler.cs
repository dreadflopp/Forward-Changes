using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class MaxHeightHandler : AbstractPropertyHandler<IWorldspaceMaxHeightGetter?>
    {
        public override string PropertyName => "MaxHeight";

        public override void SetValue(IMajorRecord record, IWorldspaceMaxHeightGetter? value)
        {
            if (record is IWorldspace worldspaceRecord)
            {
                if (value == null)
                {
                    worldspaceRecord.MaxHeight = null;
                }
                else
                {
                    // Deep copy
                    var newMaxHeight = new WorldspaceMaxHeight();
                    newMaxHeight.DeepCopyIn(value);
                    worldspaceRecord.MaxHeight = newMaxHeight;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspace for {PropertyName}");
            }
        }

        public override IWorldspaceMaxHeightGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IWorldspaceGetter worldspaceRecord)
            {
                return worldspaceRecord.MaxHeight;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspaceGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IWorldspaceMaxHeightGetter? value1, IWorldspaceMaxHeightGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties using value-based comparison
            return value1.Min.Equals(value2.Min) &&
                   value1.Max.Equals(value2.Max) &&
                   AreByteArraysEqual(value1.CellData, value2.CellData);
        }

        private bool AreByteArraysEqual(ReadOnlyMemorySlice<byte> data1, ReadOnlyMemorySlice<byte> data2)
        {
            if (data1.Length != data2.Length) return false;
            return data1.Span.SequenceEqual(data2.Span);
        }

        public override string FormatValue(object? value)
        {
            if (value is not IWorldspaceMaxHeightGetter maxHeight)
            {
                return value?.ToString() ?? "null";
            }

            var cellDataInfo = maxHeight.CellData.Length == 0 ? "empty" : $"{maxHeight.CellData.Length} bytes";
            return $"Min: {maxHeight.Min}, Max: {maxHeight.Max}, CellData: {cellDataInfo}";
        }
    }
}


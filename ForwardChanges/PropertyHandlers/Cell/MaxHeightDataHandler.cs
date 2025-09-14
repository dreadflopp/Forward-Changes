using System;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class MaxHeightDataHandler : AbstractPropertyHandler<ICellMaxHeightDataGetter?>
    {
        public override string PropertyName => "MaxHeightData";

        public override void SetValue(IMajorRecord record, ICellMaxHeightDataGetter? value)
        {
            if (record is ICell cellRecord)
            {
                if (value == null)
                {
                    cellRecord.MaxHeightData = null;
                    return;
                }

                // Try using DeepCopyIn first, but handle HeightMap separately if needed
                var newMaxHeightData = new CellMaxHeightData();
                newMaxHeightData.DeepCopyIn(value);

                cellRecord.MaxHeightData = newMaxHeightData;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ICell for {PropertyName}");
            }
        }

        public override ICellMaxHeightDataGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cellRecord)
            {
                return cellRecord.MaxHeightData;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ICellGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(ICellMaxHeightDataGetter? value1, ICellMaxHeightDataGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare Offset with bitwise comparison to distinguish between 0.0f and -0.0f
            var offset1Bits = BitConverter.SingleToInt32Bits(value1.Offset);
            var offset2Bits = BitConverter.SingleToInt32Bits(value2.Offset);

            if (offset1Bits != offset2Bits)
            {
                return false;
            }

            // Compare HeightMaps
            var heightMap1 = value1.HeightMap;
            var heightMap2 = value2.HeightMap;

            if (heightMap1 == null && heightMap2 == null) return true;
            if (heightMap1 == null || heightMap2 == null) return false;

            if (heightMap1.Width != heightMap2.Width || heightMap1.Height != heightMap2.Height)
            {
                return false;
            }

            // Use byte-by-byte comparison for HeightMap (built-in Equals does reference comparison)
            for (int x = 0; x < heightMap1.Width; x++)
            {
                for (int y = 0; y < heightMap1.Height; y++)
                {
                    if (heightMap1[x, y] != heightMap2[x, y])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private byte GetMinValue(IReadOnlyArray2d<byte> heightMap)
        {
            if (heightMap == null) return 0;
            byte min = byte.MaxValue;
            for (int x = 0; x < heightMap.Width; x++)
            {
                for (int y = 0; y < heightMap.Height; y++)
                {
                    if (heightMap[x, y] < min) min = heightMap[x, y];
                }
            }
            return min;
        }

        private byte GetMaxValue(IReadOnlyArray2d<byte> heightMap)
        {
            if (heightMap == null) return 0;
            byte max = byte.MinValue;
            for (int x = 0; x < heightMap.Width; x++)
            {
                for (int y = 0; y < heightMap.Height; y++)
                {
                    if (heightMap[x, y] > max) max = heightMap[x, y];
                }
            }
            return max;
        }

        public override string FormatValue(object? value)
        {
            if (value is not ICellMaxHeightDataGetter maxHeightData)
            {
                return value?.ToString() ?? "null";
            }

            // Show Offset and HeightMap summary
            var heightMap = maxHeightData.HeightMap;
            if (heightMap == null)
            {
                return $"Offset: {maxHeightData.Offset}, HeightMap: null";
            }

            // Calculate HeightMap summary statistics
            var min = byte.MaxValue;
            var max = byte.MinValue;
            var sum = 0;
            var count = 0;

            for (int x = 0; x < heightMap.Width; x++)
            {
                for (int y = 0; y < heightMap.Height; y++)
                {
                    var val = heightMap[x, y];
                    if (val < min) min = val;
                    if (val > max) max = val;
                    sum += val;
                    count++;
                }
            }

            var avg = count > 0 ? (double)sum / count : 0;

            return $"Offset: {maxHeightData.Offset}, HeightMap: {heightMap.Width}x{heightMap.Height} (min:{min}, max:{max}, avg:{avg:F1})";
        }
    }
}
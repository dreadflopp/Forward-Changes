using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class WaterVelocityHandler : AbstractPropertyHandler<ICellWaterVelocityGetter?>
    {
        public override string PropertyName => "WaterVelocity";

        public override void SetValue(IMajorRecord record, ICellWaterVelocityGetter? value)
        {
            var cellRecord = TryCastRecord<ICell>(record, PropertyName);
            if (cellRecord != null)
            {
                if (value != null)
                {
                    // Create a new CellWaterVelocity and copy the values
                    var newWaterVelocity = new CellWaterVelocity();
                    newWaterVelocity.DeepCopyIn(value);
                    cellRecord.WaterVelocity = newWaterVelocity;
                }
                else
                {
                    cellRecord.WaterVelocity = null;
                }
            }
        }

        public override ICellWaterVelocityGetter? GetValue(IMajorRecordGetter record)
        {
            var cellRecord = TryCastRecord<ICellGetter>(record, PropertyName);
            return cellRecord?.WaterVelocity;
        }

        public override bool AreValuesEqual(ICellWaterVelocityGetter? value1, ICellWaterVelocityGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties of ICellWaterVelocityGetter
            return value1.Offset.Equals(value2.Offset) &&
                   value1.Unknown == value2.Unknown &&
                   value1.Angle.Equals(value2.Angle) &&
                   AreByteArraysEqual(value1.Unknown2, value2.Unknown2);
        }

        private static bool AreByteArraysEqual(ReadOnlyMemorySlice<byte> bytes1, ReadOnlyMemorySlice<byte> bytes2)
        {
            return bytes1.Span.SequenceEqual(bytes2.Span);
        }

        public override string FormatValue(object? value)
        {
            if (value is ICellWaterVelocityGetter waterVelocity)
            {
                return $"WaterVelocity(Offset:{waterVelocity.Offset}, Unknown:{waterVelocity.Unknown}, Angle:{waterVelocity.Angle}, Unknown2:{waterVelocity.Unknown2.Length} bytes)";
            }
            return value?.ToString() ?? "null";
        }
    }
}

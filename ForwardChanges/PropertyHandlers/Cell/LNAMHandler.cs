using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class LNAMHandler : AbstractPropertyHandler<ReadOnlyMemorySlice<byte>?>
    {
        public override string PropertyName => "LNAM";

        public override void SetValue(IMajorRecord record, ReadOnlyMemorySlice<byte>? value)
        {
            var cellRecord = TryCastRecord<ICell>(record, PropertyName);
            if (cellRecord != null)
            {
                cellRecord.LNAM = value?.ToArray();
            }
        }

        public override ReadOnlyMemorySlice<byte>? GetValue(IMajorRecordGetter record)
        {
            var cellRecord = TryCastRecord<ICellGetter>(record, PropertyName);
            return cellRecord?.LNAM;
        }

        public override bool AreValuesEqual(ReadOnlyMemorySlice<byte>? value1, ReadOnlyMemorySlice<byte>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare byte arrays using SequenceEqual
            return value1.Value.Span.SequenceEqual(value2.Value.Span);
        }

        public override string FormatValue(object? value)
        {
            if (value is ReadOnlyMemorySlice<byte> slice)
            {
                return $"LNAM({slice.Length} bytes)";
            }
            return value?.ToString() ?? "null";
        }
    }
}

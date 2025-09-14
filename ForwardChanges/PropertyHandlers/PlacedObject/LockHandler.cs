using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Translations;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class LockHandler : AbstractPropertyHandler<ILockDataGetter?>
    {
        public override string PropertyName => "Lock";

        public override void SetValue(IMajorRecord record, ILockDataGetter? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                if (value != null)
                {
                    // Create new LockData and copy properties
                    var newLockData = new LockData
                    {
                        Level = value.Level,
                        Unused = value.Unused.ToArray(),
                        Key = new FormLink<IKeyGetter>(value.Key.FormKey),
                        Flags = value.Flags,
                        Unused2 = value.Unused2.ToArray()
                    };
                    placedObjectRecord.Lock = newLockData;
                }
                else
                {
                    placedObjectRecord.Lock = null;
                }
            }
        }

        public override ILockDataGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.Lock;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        public override bool AreValuesEqual(ILockDataGetter? value1, ILockDataGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties
            if (value1.Level != value2.Level) return false;
            if (!value1.Unused.Span.SequenceEqual(value2.Unused.Span)) return false;
            if (!value1.Key.FormKey.Equals(value2.Key.FormKey)) return false;
            if (value1.Flags != value2.Flags) return false;
            if (!value1.Unused2.Span.SequenceEqual(value2.Unused2.Span)) return false;

            return true;
        }
    }
}
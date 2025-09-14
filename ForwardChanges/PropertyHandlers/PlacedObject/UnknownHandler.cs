using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class UnknownHandler : AbstractPropertyHandler<short>
    {
        public override string PropertyName => "Unknown";

        public override void SetValue(IMajorRecord record, short value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.Unknown = value;
            }
        }

        public override short GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.Unknown;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return 0;
        }
    }
}


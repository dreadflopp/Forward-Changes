using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class IsOpenByDefaultHandler : AbstractPropertyHandler<bool>
    {
        public override string PropertyName => "IsOpenByDefault";

        public override void SetValue(IMajorRecord record, bool value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.IsOpenByDefault = value;
            }
        }

        public override bool GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.IsOpenByDefault;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return false;
        }
    }
}
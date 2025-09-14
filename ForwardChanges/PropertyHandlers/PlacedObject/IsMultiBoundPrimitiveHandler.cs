using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class IsMultiBoundPrimitiveHandler : AbstractPropertyHandler<bool>
    {
        public override string PropertyName => "IsMultiBoundPrimitive";

        public override void SetValue(IMajorRecord record, bool value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.IsMultiBoundPrimitive = value;
            }
        }

        public override bool GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.IsMultiBoundPrimitive;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return false;
        }
    }
}
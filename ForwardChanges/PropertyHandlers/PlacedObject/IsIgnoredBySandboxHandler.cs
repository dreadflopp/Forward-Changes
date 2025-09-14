using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class IsIgnoredBySandboxHandler : AbstractPropertyHandler<bool>
    {
        public override string PropertyName => "IsIgnoredBySandbox";

        public override void SetValue(IMajorRecord record, bool value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.IsIgnoredBySandbox = value;
            }
        }

        public override bool GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.IsIgnoredBySandbox;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return false;
        }
    }
}
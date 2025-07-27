using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class UnknownHandler : AbstractPropertyHandler<short>
    {
        public override string PropertyName => "Unknown";

        public override short GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.Unknown;
            }

            Console.WriteLine($"Error: Record does not implement IPlacedObjectGetter for {PropertyName}");
            return 0;
        }

        public override void SetValue(IMajorRecord record, short value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.Unknown = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IPlacedObject for {PropertyName}");
            }
        }


    }
}


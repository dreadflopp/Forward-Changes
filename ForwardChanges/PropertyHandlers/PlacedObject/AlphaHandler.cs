using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class AlphaHandler : AbstractPropertyHandler<IAlphaGetter?>
    {
        public override string PropertyName => "Alpha";

        public override void SetValue(IMajorRecord record, IAlphaGetter? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                if (value != null)
                {
                    // Create new Alpha and copy properties
                    var newAlpha = new Alpha
                    {
                        Cutoff = value.Cutoff,
                        Base = value.Base
                    };
                    placedObjectRecord.Alpha = newAlpha;
                }
                else
                {
                    placedObjectRecord.Alpha = null;
                }
            }
        }

        public override IAlphaGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.Alpha;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        public override bool AreValuesEqual(IAlphaGetter? value1, IAlphaGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties
            if (value1.Cutoff != value2.Cutoff) return false;
            if (value1.Base != value2.Base) return false;

            return true;
        }
    }
}
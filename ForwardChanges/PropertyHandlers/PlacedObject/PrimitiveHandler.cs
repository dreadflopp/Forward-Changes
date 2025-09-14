using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class PrimitiveHandler : AbstractPropertyHandler<IPlacedPrimitiveGetter?>
    {
        public override string PropertyName => "Primitive";

        public override void SetValue(IMajorRecord record, IPlacedPrimitiveGetter? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                if (value != null)
                {
                    // Create new PlacedPrimitive and copy properties
                    var newPrimitive = new PlacedPrimitive
                    {
                        Bounds = value.Bounds,
                        Color = value.Color,
                        Unknown = value.Unknown,
                        Type = value.Type
                    };
                    placedObjectRecord.Primitive = newPrimitive;
                }
                else
                {
                    placedObjectRecord.Primitive = null;
                }
            }
        }

        public override IPlacedPrimitiveGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.Primitive;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        public override bool AreValuesEqual(IPlacedPrimitiveGetter? value1, IPlacedPrimitiveGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties
            if (!value1.Bounds.Equals(value2.Bounds)) return false;
            if (value1.Color != value2.Color) return false;
            if (value1.Unknown != value2.Unknown) return false;
            if (value1.Type != value2.Type) return false;

            return true;
        }
    }
}
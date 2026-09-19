using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Light
{
    public class ObjectBoundsHandler : AbstractPropertyHandler<IObjectBoundsGetter>
    {
        public override string PropertyName => "ObjectBounds";

        public override IObjectBoundsGetter GetValue(IMajorRecordGetter record)
        {
            if (record is ILightGetter lightRecord)
            {
                return lightRecord.ObjectBounds;
            }
            return new ObjectBounds();
        }

        public override void SetValue(IMajorRecord record, IObjectBoundsGetter? value)
        {
            if (record is Mutagen.Bethesda.Skyrim.Light lightRecord && value != null)
            {
                // Create a new ObjectBounds and copy the values
                var newBounds = new ObjectBounds
                {
                    First = value.First,
                    Second = value.Second
                };
                lightRecord.ObjectBounds = newBounds;
            }
        }

        public override bool AreValuesEqual(IObjectBoundsGetter? value1, IObjectBoundsGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.First.Equals(value2.First) && value1.Second.Equals(value2.Second);
        }

        public override string FormatValue(object? value)
        {
            if (value is IObjectBoundsGetter bounds)
            {
                return $"ObjectBounds(First: {bounds.First}, Second: {bounds.Second})";
            }
            return value?.ToString() ?? "ObjectBounds()";
        }
    }
}



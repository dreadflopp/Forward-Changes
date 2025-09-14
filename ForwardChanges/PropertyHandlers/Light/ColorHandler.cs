using System;
using System.Drawing;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Light
{
    public class ColorHandler : AbstractPropertyHandler<Color>
    {
        public override string PropertyName => "Color";

        public override Color GetValue(IMajorRecordGetter record)
        {
            if (record is ILightGetter lightRecord)
            {
                return lightRecord.Color;
            }
            return Color.Empty;
        }

        public override void SetValue(IMajorRecord record, Color value)
        {
            if (record is Mutagen.Bethesda.Skyrim.Light lightRecord)
            {
                lightRecord.Color = value;
            }
        }

        public override bool AreValuesEqual(Color value1, Color value2)
        {
            return value1.Equals(value2);
        }

        public override string FormatValue(object? value)
        {
            if (value is Color color)
            {
                return $"Color(A:{color.A}, R:{color.R}, G:{color.G}, B:{color.B})";
            }
            return value?.ToString() ?? "Color.Empty";
        }
    }
}


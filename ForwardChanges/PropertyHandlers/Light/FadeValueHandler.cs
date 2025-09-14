using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Light
{
    public class FadeValueHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "FadeValue";

        public override float GetValue(IMajorRecordGetter record)
        {
            if (record is ILightGetter lightRecord)
            {
                return lightRecord.FadeValue;
            }
            return 0.0f;
        }

        public override void SetValue(IMajorRecord record, float value)
        {
            if (record is Mutagen.Bethesda.Skyrim.Light lightRecord)
            {
                lightRecord.FadeValue = value;
            }
        }

        public override bool AreValuesEqual(float value1, float value2)
        {
            return value1 == value2;
        }

        public override string FormatValue(object? value)
        {
            return value?.ToString() ?? "0.0";
        }
    }
}



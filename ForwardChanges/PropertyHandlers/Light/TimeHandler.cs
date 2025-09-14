using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Light
{
    public class TimeHandler : AbstractPropertyHandler<int>
    {
        public override string PropertyName => "Time";

        public override int GetValue(IMajorRecordGetter record)
        {
            if (record is ILightGetter lightRecord)
            {
                return lightRecord.Time;
            }
            return 0;
        }

        public override void SetValue(IMajorRecord record, int value)
        {
            if (record is Mutagen.Bethesda.Skyrim.Light lightRecord)
            {
                lightRecord.Time = value;
            }
        }

        public override bool AreValuesEqual(int value1, int value2)
        {
            return value1 == value2;
        }

        public override string FormatValue(object? value)
        {
            return value?.ToString() ?? "0";
        }
    }
}



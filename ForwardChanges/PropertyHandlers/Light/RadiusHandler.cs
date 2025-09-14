using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Light
{
    public class RadiusHandler : AbstractPropertyHandler<uint>
    {
        public override string PropertyName => "Radius";

        public override uint GetValue(IMajorRecordGetter record)
        {
            if (record is ILightGetter lightRecord)
            {
                return lightRecord.Radius;
            }
            return 0;
        }

        public override void SetValue(IMajorRecord record, uint value)
        {
            if (record is Mutagen.Bethesda.Skyrim.Light lightRecord)
            {
                lightRecord.Radius = value;
            }
        }

        public override bool AreValuesEqual(uint value1, uint value2)
        {
            return value1 == value2;
        }

        public override string FormatValue(object? value)
        {
            return value?.ToString() ?? "0";
        }
    }
}



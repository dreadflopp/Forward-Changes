using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using System;

namespace ForwardChanges.PropertyHandlers.Static
{
    public class MaxAngleHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "MaxAngle";

        public override void SetValue(IMajorRecord record, float value)
        {
            var staticRecord = TryCastRecord<IStatic>(record, PropertyName);
            if (staticRecord != null)
            {
                staticRecord.MaxAngle = value;
            }
        }

        public override float GetValue(IMajorRecordGetter record)
        {
            var staticRecord = TryCastRecord<IStaticGetter>(record, PropertyName);
            if (staticRecord != null)
            {
                return staticRecord.MaxAngle;
            }
            return 30f; // Default value
        }

        public override bool AreValuesEqual(float value1, float value2)
        {
            return Math.Abs(value1 - value2) < 0.0001f;
        }
    }
}

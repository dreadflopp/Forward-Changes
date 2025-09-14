using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingestible
{
    public class AddictionChanceHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "AddictionChance";

        public override float GetValue(IMajorRecordGetter record)
        {
            if (record is IIngestibleGetter ingestibleRecord)
            {
                return ingestibleRecord.AddictionChance;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngestibleGetter for {PropertyName}");
            }
            return 0f;
        }

        public override void SetValue(IMajorRecord record, float value)
        {
            if (record is IIngestible ingestibleRecord)
            {
                ingestibleRecord.AddictionChance = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngestible for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(float value1, float value2)
        {
            return Math.Abs(value1 - value2) < 0.001f;
        }
    }
}